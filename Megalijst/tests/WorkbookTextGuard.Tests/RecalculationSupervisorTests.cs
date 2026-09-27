using System;
using System.Threading;
using System.Threading.Tasks;
using WorkbookTextGuard.Commands;
using Xunit;

namespace WorkbookTextGuard.Tests;

public class RecalculationSupervisorTests
{
    [Fact]
    public void Complete_WorkerFinishedWithSuccessfulWorkflow_AllowsSuccess()
    {
        var supervisor = new RecalculationSupervisor();
        supervisor.RecordWorkerResult(WorkflowResult.Success());

        Assert.True(supervisor.Complete(workerFinished: true).Succeeded);
    }

    [Fact]
    public void Complete_WorkerFinishedWithoutResult_FailsClosed()
    {
        var supervisor = new RecalculationSupervisor();

        Assert.False(supervisor.Complete(workerFinished: true).Succeeded);
    }

    [Fact]
    public void Complete_WorkerFailed_PreservesFailure()
    {
        var supervisor = new RecalculationSupervisor();
        supervisor.RecordWorkerResult(WorkflowResult.Failure("SaveAs failed: Disk full"));

        var decision = supervisor.Complete(workerFinished: true);

        Assert.False(decision.Succeeded);
        Assert.Equal("SaveAs failed: Disk full", decision.FailureReason);
    }

    [Fact]
    public void Complete_TimeoutAfterSuccessfulSaveDuringCleanup_PermanentlyFails()
    {
        var supervisor = new RecalculationSupervisor();
        bool saved = false;
        var workflow = MakeWorkflow(() => saved = true);
        supervisor.RecordWorkerResult(workflow.Execute());
        Assert.True(saved);

        // Save has completed, but the initial Join timed out during Close/Quit.
        var decision = supervisor.Complete(workerFinished: false);
        supervisor.RecordWorkerResult(WorkflowResult.Success());

        AssertTimeout(decision);
        Assert.Same(decision, supervisor.Complete(workerFinished: true));
    }

    [Fact]
    public async Task Complete_TimeoutWhileSaveBlocked_LateSaveCannotRestoreSuccess()
    {
        var supervisor = new RecalculationSupervisor();
        var saveEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var releaseSave = new ManualResetEventSlim();
        bool saved = false;
        var workflow = MakeWorkflow(() =>
        {
            saveEntered.SetResult(true);
            if (!releaseSave.Wait(TimeSpan.FromSeconds(10)))
                throw new TimeoutException("Test did not release synthetic SaveAs.");
            saved = true;
        });
        var worker = Task.Run(() =>
        {
            var result = workflow.Execute();
            supervisor.RecordWorkerResult(result);
            return result;
        });

        WorkflowResult decision;
        try
        {
            await saveEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            decision = supervisor.Complete(workerFinished: false);
            AssertTimeout(decision);
        }
        finally
        {
            releaseSave.Set();
            await worker;
        }

        // The synthetic worker really did finish SaveAs successfully after the timeout.
        Assert.True((await worker).Succeeded);
        Assert.True(saved);
        Assert.Same(decision, supervisor.Complete(workerFinished: true));
        AssertTimeout(supervisor.Complete(workerFinished: true));
    }

    [Fact]
    public async Task Complete_TimeoutRacingWorkerSuccess_AlwaysFails()
    {
        for (int i = 0; i < 100; i++)
        {
            var supervisor = new RecalculationSupervisor();
            var start = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var worker = Task.Run(async () =>
            {
                await start.Task;
                supervisor.RecordWorkerResult(WorkflowResult.Success());
            });
            var timeout = Task.Run(async () =>
            {
                await start.Task;
                return supervisor.Complete(workerFinished: false);
            });
            start.SetResult(true);
            await Task.WhenAll(worker, timeout);

            AssertTimeout(await timeout);
            Assert.Same(await timeout, supervisor.Complete(workerFinished: true));
        }
    }

    private static RecalculationWorkflow MakeWorkflow(Action save)
    {
        var guard = new CalculationGuard(
            getState: () => CalculationState.Done,
            wait: _ => throw new InvalidOperationException("Done should not wait."),
            elapsedMs: () => 0,
            timeoutMs: 100);
        return new RecalculationWorkflow(() => { }, save, guard);
    }

    private static void AssertTimeout(WorkflowResult result)
    {
        Assert.False(result.Succeeded);
        Assert.Equal("Supervisor timeout", result.FailureReason);
    }
}
