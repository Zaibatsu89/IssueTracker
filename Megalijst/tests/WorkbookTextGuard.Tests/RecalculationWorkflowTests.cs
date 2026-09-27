using System;
using System.Collections.Generic;
using WorkbookTextGuard.Commands;
using Xunit;

namespace WorkbookTextGuard.Tests;

public class RecalculationWorkflowTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static CalculationGuard MakeGuard(
        CalculationState[] states,
        long timeoutMs = 5000,
        int pollMs = 100)
    {
        var queue = new Queue<CalculationState>(states);
        long elapsed = 0;
        return new CalculationGuard(
            getState:       () => queue.Count > 0 ? queue.Dequeue() : CalculationState.Done,
            wait:           ms => { elapsed += ms; },
            elapsedMs:      () => elapsed,
            timeoutMs:      timeoutMs,
            pollIntervalMs: pollMs);
    }

    private static CalculationGuard MakeTimeoutGuard()
    {
        long elapsed = 0;
        return new CalculationGuard(
            getState:       () => CalculationState.Calculating,
            wait:           ms => { elapsed += ms; },
            elapsedMs:      () => elapsed + 99999,
            timeoutMs:      100,
            pollIntervalMs: 50);
    }

    // ── Successful workflow ───────────────────────────────────────────────────

    [Fact]
    public void Execute_ImmediatelyDone_SaveCalled_Succeeds()
    {
        bool saveCalled = false;
        bool calcCalled = false;

        var guard = MakeGuard(new[] { CalculationState.Done });
        var workflow = new RecalculationWorkflow(
            triggerCalculation: () => { calcCalled = true; },
            saveOutput:         () => { saveCalled = true; },
            guard:              guard);

        var result = workflow.Execute();

        Assert.True(result.Succeeded);
        Assert.True(calcCalled);
        Assert.True(saveCalled);
        Assert.Null(result.FailureReason);
    }

    [Fact]
    public void Execute_CalculatingThenDone_SaveCalled_Succeeds()
    {
        bool saveCalled = false;
        var guard = MakeGuard(new[]
        {
            CalculationState.Calculating,
            CalculationState.Calculating,
            CalculationState.Done
        });
        var workflow = new RecalculationWorkflow(
            triggerCalculation: () => { },
            saveOutput:         () => { saveCalled = true; },
            guard:              guard);

        var result = workflow.Execute();

        Assert.True(result.Succeeded);
        Assert.True(saveCalled);
    }

    // ── Timeout: Save must NOT be called ─────────────────────────────────────

    [Fact]
    public void Execute_Timeout_SaveNotCalled_Fails()
    {
        bool saveCalled = false;
        var guard = MakeTimeoutGuard();
        var workflow = new RecalculationWorkflow(
            triggerCalculation: () => { },
            saveOutput:         () => { saveCalled = true; },
            guard:              guard);

        var result = workflow.Execute();

        Assert.False(result.Succeeded);
        Assert.False(saveCalled, "SaveAs must not be called when calculation timed out.");
        Assert.NotNull(result.FailureReason);
        Assert.Contains("timed out", result.FailureReason, StringComparison.OrdinalIgnoreCase);
    }

    // The elapsed clock must include time spent inside CalculateFullRebuild.
    [Fact]
    public void Execute_TriggerConsumesDeadline_NoStateReadOrSave()
    {
        var order = new List<string>();
        long elapsed = 0;
        var guard = new CalculationGuard(
            getState: () => { order.Add("state"); return CalculationState.Done; },
            wait: ms => elapsed += ms,
            elapsedMs: () => elapsed,
            timeoutMs: 100);
        var workflow = new RecalculationWorkflow(
            triggerCalculation: () => { order.Add("trigger"); elapsed = 100; },
            saveOutput: () => order.Add("save"),
            guard: guard);

        var result = workflow.Execute();

        Assert.False(result.Succeeded);
        Assert.True(guard.TimedOut);
        Assert.Equal(new[] { "trigger" }, order);
    }

    [Fact]
    public void Execute_StateReadThrows_NoSaveOrSuccess()
    {
        var order = new List<string>();
        var guard = new CalculationGuard(
            getState: () => { order.Add("state"); throw new InvalidOperationException("State COM error"); },
            wait: _ => order.Add("wait"),
            elapsedMs: () => 0,
            timeoutMs: 100);
        var workflow = new RecalculationWorkflow(
            triggerCalculation: () => order.Add("trigger"),
            saveOutput: () => order.Add("save"),
            guard: guard);

        var result = workflow.Execute();

        Assert.False(result.Succeeded);
        Assert.Contains("State COM error", result.FailureReason, StringComparison.Ordinal);
        Assert.Equal(new[] { "trigger", "state" }, order);
    }

    // ── CalculateFullRebuild exception ────────────────────────────────────────

    [Fact]
    public void Execute_TriggerThrows_SaveNotCalled_Fails()
    {
        var order = new List<string>();
        var guard = new CalculationGuard(
            getState: () => { order.Add("state"); return CalculationState.Done; },
            wait: _ => order.Add("wait"),
            elapsedMs: () => 0,
            timeoutMs: 100);
        var workflow = new RecalculationWorkflow(
            triggerCalculation: () => { order.Add("trigger"); throw new InvalidOperationException("COM error"); },
            saveOutput: () => order.Add("save"),
            guard: guard);

        var result = workflow.Execute();

        Assert.False(result.Succeeded);
        Assert.Equal(new[] { "trigger" }, order);
        Assert.Contains("COM error", result.FailureReason, StringComparison.OrdinalIgnoreCase);
    }

    // ── SaveAs exception ──────────────────────────────────────────────────────

    [Fact]
    public void Execute_SaveThrows_Fails()
    {
        var guard = MakeGuard(new[] { CalculationState.Done });
        var workflow = new RecalculationWorkflow(
            triggerCalculation: () => { },
            saveOutput:         () => throw new IOException("Disk full"),
            guard:              guard);

        var result = workflow.Execute();

        Assert.False(result.Succeeded);
        Assert.Contains("Disk full", result.FailureReason, StringComparison.OrdinalIgnoreCase);
    }

    // ── Calculation order: trigger → wait → save ──────────────────────────────

    [Fact]
    public void Execute_OrderIsCorrect_TriggerBeforeStateReadsBeforeSave()
    {
        var order = new List<string>();
        var states = new Queue<CalculationState>(new[]
        {
            CalculationState.Pending, CalculationState.Calculating, CalculationState.Done
        });
        long elapsed = 0;
        var guard = new CalculationGuard(
            getState: () =>
            {
                var state = states.Dequeue();
                order.Add($"state:{state}");
                return state;
            },
            wait: ms => { order.Add("wait"); elapsed += ms; },
            elapsedMs: () => elapsed,
            timeoutMs: 5000,
            pollIntervalMs: 100);
        var workflow = new RecalculationWorkflow(
            triggerCalculation: () => order.Add("trigger"),
            saveOutput: () => order.Add("save"),
            guard: guard);

        var result = workflow.Execute();

        Assert.True(result.Succeeded);
        Assert.Equal(new[]
        {
            "trigger", "state:Pending", "wait", "state:Calculating", "wait", "state:Done", "save"
        }, order);
        Assert.Empty(states);
    }

    // ── Pending state (value=2) also causes waiting ───────────────────────────

    [Fact]
    public void Execute_PendingThenDone_SaveCalled()
    {
        bool saveCalled = false;
        var guard = MakeGuard(new[]
        {
            CalculationState.Pending,
            CalculationState.Done
        });
        var workflow = new RecalculationWorkflow(
            triggerCalculation: () => { },
            saveOutput:         () => { saveCalled = true; },
            guard:              guard);

        var result = workflow.Execute();

        Assert.True(result.Succeeded);
        Assert.True(saveCalled);
    }
}