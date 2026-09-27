using System;
using System.Collections.Generic;
using WorkbookTextGuard.Commands;
using Xunit;

namespace WorkbookTextGuard.Tests;

public class CalculationGuardTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static (CalculationGuard guard, List<int> waits) BuildGuard(
        IEnumerable<CalculationState> stateSequence,
        long timeoutMs,
        int pollIntervalMs = 200)
    {
        var states = new Queue<CalculationState>(stateSequence);
        long elapsed = 0;
        var waits = new List<int>();

        var guard = new CalculationGuard(
            getState:       () => states.Count > 0 ? states.Dequeue() : CalculationState.Done,
            wait:           ms => { waits.Add(ms); elapsed += ms; },
            elapsedMs:      () => elapsed,
            timeoutMs:      timeoutMs,
            pollIntervalMs: pollIntervalMs);

        return (guard, waits);
    }

    // Enum aliases for readability
    private const CalculationState Done        = CalculationState.Done;
    private const CalculationState Calculating = CalculationState.Calculating;
    private const CalculationState Pending     = CalculationState.Pending;

    // ── Direct done ───────────────────────────────────────────────────────────

    [Fact]
    public void WaitForCompletion_ImmediatelyDone_ReturnsTrue()
    {
        var (guard, waits) = BuildGuard(new[] { Done }, timeoutMs: 5000);

        bool result = guard.WaitForCompletion();

        Assert.True(result);
        Assert.False(guard.TimedOut);
        Assert.True(guard.IsSuccessful);
        Assert.Empty(waits);
    }

    // ── IsSuccessful is false before first poll ───────────────────────────────

    [Fact]
    public void IsSuccessful_BeforeFirstPoll_False()
    {
        var (guard, _) = BuildGuard(new[] { Done }, timeoutMs: 5000);
        // Do NOT call WaitForCompletion
        Assert.False(guard.IsSuccessful);
    }

    // ── Calculating then done ─────────────────────────────────────────────────

    [Fact]
    public void WaitForCompletion_CalculatingThenDone_ReturnsTrue()
    {
        var (guard, waits) = BuildGuard(
            new[] { Calculating, Calculating, Done },
            timeoutMs: 5000,
            pollIntervalMs: 100);

        bool result = guard.WaitForCompletion();

        Assert.True(result);
        Assert.False(guard.TimedOut);
        Assert.True(guard.IsSuccessful);
        Assert.Equal(new[] { 100, 100 }, waits);
    }

    // ── Pending then done ─────────────────────────────────────────────────────

    [Fact]
    public void WaitForCompletion_PendingThenDone_ReturnsTrue()
    {
        var (guard, waits) = BuildGuard(
            new[] { Pending, Done },
            timeoutMs: 5000,
            pollIntervalMs: 100);

        bool result = guard.WaitForCompletion();

        Assert.True(result);
        Assert.True(guard.IsSuccessful);
        Assert.Equal(new[] { 100 }, waits);
    }

    // ── Never done (timeout) ──────────────────────────────────────────────────

    [Fact]
    public void WaitForCompletion_NeverDone_ReturnsFalse_TimedOut()
    {
        long elapsed = 0;
        var waits = new List<int>();

        var guard = new CalculationGuard(
            getState:       () => Calculating,
            wait:           ms => { waits.Add(ms); elapsed += ms; },
            elapsedMs:      () => elapsed,
            timeoutMs:      500,
            pollIntervalMs: 200);

        bool result = guard.WaitForCompletion();

        Assert.False(result);
        Assert.True(guard.TimedOut);
        Assert.False(guard.IsSuccessful);
        Assert.Equal(new[] { 200, 200, 100 }, waits);
    }

    // ── Deadline: elapsed >= timeoutMs triggers timeout ───────────────────────

    [Fact]
    public void WaitForCompletion_ElapsedEqualsTimeout_TimesOut()
    {
        // elapsed starts at exactly timeoutMs — deadline already reached before first poll
        var guard = new CalculationGuard(
            getState:       () => Calculating,
            wait:           _ => { },
            elapsedMs:      () => 500, // always at deadline
            timeoutMs:      500,
            pollIntervalMs: 200);

        bool result = guard.WaitForCompletion();

        Assert.False(result);
        Assert.True(guard.TimedOut);
    }

    // ── TimedOut is permanent — late Done cannot override ─────────────────────

    [Fact]
    public void TimedOut_Permanent_LateCompletionCannotOverride()
    {
        long elapsed = 0;
        int callCount = 0;

        var guard = new CalculationGuard(
            getState:    () => { callCount++; return callCount == 1 ? Calculating : Done; },
            wait:        ms => { elapsed += ms; },
            elapsedMs:   () => elapsed + 1000, // always past deadline
            timeoutMs:   500,
            pollIntervalMs: 200);

        bool result = guard.WaitForCompletion();

        Assert.False(result);
        Assert.True(guard.TimedOut);
        Assert.False(guard.IsSuccessful);
    }

    // ── IsSuccessful after success ────────────────────────────────────────────

    [Fact]
    public void IsSuccessful_AfterSuccess_True()
    {
        var (guard, _) = BuildGuard(new[] { Done }, timeoutMs: 5000);
        guard.WaitForCompletion();
        Assert.True(guard.IsSuccessful);
    }

    [Fact]
    public void IsSuccessful_AfterTimeout_False()
    {
        var guard = new CalculationGuard(
            getState:    () => Calculating,
            wait:        _ => { },
            elapsedMs:   () => 9999,
            timeoutMs:   100,
            pollIntervalMs: 50);

        guard.WaitForCompletion();
        Assert.False(guard.IsSuccessful);
    }

    // ── State error ───────────────────────────────────────────────────────────

    [Fact]
    public void WaitForCompletion_StateError_ThrowsAndSetsTimedOut()
    {
        var guard = new CalculationGuard(
            getState:    () => throw new InvalidOperationException("COM error"),
            wait:        _ => { },
            elapsedMs:   () => 0,
            timeoutMs:   5000,
            pollIntervalMs: 200);

        Assert.Throws<InvalidOperationException>(() => guard.WaitForCompletion());
        Assert.True(guard.TimedOut);
        Assert.False(guard.IsSuccessful);
    }

    // ── Poll interval bounded to remaining time ───────────────────────────────

    [Fact]
    public void WaitForCompletion_UsesConfiguredPollInterval()
    {
        var (guard, waits) = BuildGuard(
            new[] { Calculating, Done },
            timeoutMs: 5000,
            pollIntervalMs: 42);

        guard.WaitForCompletion();

        Assert.Single(waits);
        Assert.Equal(42, waits[0]);
    }

    // ── Save must not be called on timeout ────────────────────────────────────

    [Fact]
    public void TimedOut_SaveNotCalled_Verified()
    {
        bool saveCalled = false;
        var guard = new CalculationGuard(
            getState:    () => Calculating,
            wait:        _ => { },
            elapsedMs:   () => 9999,
            timeoutMs:   100,
            pollIntervalMs: 50);

        guard.WaitForCompletion();

        if (!guard.TimedOut)
            saveCalled = true;

        Assert.False(saveCalled, "SaveAs must not be called when guard has timed out.");
    }

    [Theory]
    [InlineData(499L, true)]
    [InlineData(500L, false)]
    [InlineData(501L, false)]
    public void WaitForCompletion_BlockingDoneRead_ChecksDeadlineAfterRead(
        long elapsedAfterRead, bool expectedSuccess)
    {
        long elapsed = 0;
        int reads = 0;
        var waits = new List<int>();
        var guard = new CalculationGuard(
            getState: () => { reads++; elapsed = elapsedAfterRead; return Done; },
            wait: ms => waits.Add(ms),
            elapsedMs: () => elapsed,
            timeoutMs: 500);

        Assert.Equal(expectedSuccess, guard.WaitForCompletion());
        Assert.Equal(expectedSuccess, guard.IsSuccessful);
        Assert.Equal(!expectedSuccess, guard.TimedOut);
        Assert.Equal(1, reads);
        Assert.Empty(waits);
    }

    [Fact]
    public void WaitForCompletion_DeadlineReached_DoesNotReadState()
    {
        int reads = 0;
        var guard = new CalculationGuard(
            getState: () => { reads++; return Done; },
            wait: _ => throw new InvalidOperationException("Unexpected wait"),
            elapsedMs: () => 500,
            timeoutMs: 500);

        Assert.False(guard.WaitForCompletion());
        Assert.Equal(0, reads);
        Assert.True(guard.TimedOut);
        Assert.False(guard.IsSuccessful);
    }

    [Fact]
    public void WaitForCompletion_TimedOut_RemainsLatchedAcrossRepeatedCalls()
    {
        long elapsed = 0;
        int reads = 0;
        var guard = new CalculationGuard(
            getState: () => { reads++; elapsed = 500; return Done; },
            wait: _ => throw new InvalidOperationException("Unexpected wait"),
            elapsedMs: () => elapsed,
            timeoutMs: 500);

        Assert.False(guard.WaitForCompletion());
        // Even a reset clock and an available Done must not undo the timeout.
        elapsed = 0;
        Assert.False(guard.WaitForCompletion());
        Assert.False(guard.WaitForCompletion());
        Assert.Equal(1, reads);
        Assert.True(guard.TimedOut);
        Assert.False(guard.IsSuccessful);
    }

    [Fact]
    public void WaitForCompletion_StateError_RemainsLatchedWhenLaterStateIsDone()
    {
        int reads = 0;
        var guard = new CalculationGuard(
            getState: () => ++reads == 1
                ? throw new InvalidOperationException("COM error")
                : Done,
            wait: _ => { },
            elapsedMs: () => 0,
            timeoutMs: 500);

        Assert.Throws<InvalidOperationException>(() => guard.WaitForCompletion());
        Assert.False(guard.WaitForCompletion());
        Assert.Equal(1, reads);
        Assert.True(guard.TimedOut);
        Assert.False(guard.IsSuccessful);
    }

    // ── Enum values ───────────────────────────────────────────────────────────

    [Fact]
    public void CalculationState_EnumValues_Correct()
    {
        Assert.Equal(0, (int)CalculationState.Done);
        Assert.Equal(1, (int)CalculationState.Calculating);
        Assert.Equal(2, (int)CalculationState.Pending);
    }
}