using System;

namespace WorkbookTextGuard.Commands;

/// <summary>
/// Orchestrates the calculate → wait → save sequence for Excel COM recalculation.
/// Injects all side-effecting operations so the workflow can be tested without Excel.
///
/// Contract:
/// - Calls the injected calculation trigger first.
/// - Waits via <see cref="CalculationGuard"/> until Done or timeout.
/// - Calls the injected save operation ONLY if the guard reports success.
/// - Never calls SaveOutput after a timeout or exception.
/// - Returns a <see cref="WorkflowResult"/> describing the outcome.
/// - Supervisor timeout is enforced externally; this class does not spawn threads.
/// </summary>
internal sealed class RecalculationWorkflow
{
    private readonly Action _triggerCalculation;
    private readonly Action _saveOutput;
    private readonly CalculationGuard _guard;

    public RecalculationWorkflow(
        Action triggerCalculation,
        Action saveOutput,
        CalculationGuard guard)
    {
        _triggerCalculation = triggerCalculation;
        _saveOutput         = saveOutput;
        _guard              = guard;
    }

    /// <summary>
    /// Execute the workflow. Returns a <see cref="WorkflowResult"/> with outcome details.
    /// </summary>
    public WorkflowResult Execute()
    {
        try
        {
            _triggerCalculation();
        }
        catch (Exception ex)
        {
            return WorkflowResult.Failure($"CalculateFullRebuild failed: {ex.Message}");
        }

        bool completed;
        try
        {
            completed = _guard.WaitForCompletion();
        }
        catch (Exception ex)
        {
            return WorkflowResult.Failure($"Calculation state polling failed: {ex.Message}");
        }

        if (!completed || _guard.TimedOut)
            return WorkflowResult.Failure("Calculation timed out before completion. SaveAs was not called.");

        try
        {
            _saveOutput();
        }
        catch (Exception ex)
        {
            return WorkflowResult.Failure($"SaveAs failed: {ex.Message}");
        }

        return WorkflowResult.Success();
    }
}

/// <summary>
/// Publishes worker outcomes and freezes the supervisor's reporting decision after Join.
/// A hard timeout wins even if the worker already saved, is still cleaning up, or
/// publishes success concurrently. Late worker updates cannot change a frozen decision.
/// </summary>
internal sealed class RecalculationSupervisor
{
    private readonly object _sync = new();
    private WorkflowResult? _workerResult;
    private WorkflowResult? _decision;

    public void RecordWorkerResult(WorkflowResult result)
    {
        lock (_sync)
        {
            if (_decision is null)
                _workerResult = result;
        }
    }

    /// <param name="workerFinished">The result of the supervisor's initial timed Join,
    /// not a subsequent cleanup wait. True implies all worker writes and cleanup completed.</param>
    public WorkflowResult Complete(bool workerFinished)
    {
        lock (_sync)
        {
            return _decision ??= !workerFinished
                ? WorkflowResult.Failure("Supervisor timeout")
                : _workerResult ?? WorkflowResult.Failure("Worker completed without a workflow result.");
        }
    }
}

internal sealed class WorkflowResult
{
    public bool Succeeded { get; private init; }
    public string? FailureReason { get; private init; }

    public static WorkflowResult Success() => new() { Succeeded = true };
    public static WorkflowResult Failure(string reason) => new() { Succeeded = false, FailureReason = reason };
}