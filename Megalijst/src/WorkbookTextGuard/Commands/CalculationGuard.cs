using System;

namespace WorkbookTextGuard.Commands;

/// <summary>
/// Isolates the calculation-state polling and timeout logic for the Excel COM adapter.
/// Designed for testability: clock and wait are injected, COM state is abstracted.
///
/// States:
///   - Initial: not yet polled
///   - Succeeded: WaitForCompletion returned true (Done within deadline)
///   - TimedOut: deadline exceeded — permanent, cannot be reversed by late Done
///
/// Contract:
/// - Deadline is exceeded when elapsed >= timeoutMs (inclusive boundary).
/// - Deadline is checked BEFORE and AFTER each potentially blocking state read.
/// - A timeout is permanent: late Done after timeout never produces IsSuccessful=true.
/// - SaveAs and success reporting MUST NOT occur when TimedOut is true.
/// </summary>
internal sealed class CalculationGuard
{
    private readonly Func<CalculationState> _getState;
    private readonly Action<int> _wait;
    private readonly Func<long> _elapsedMs;
    private readonly long _timeoutMs;
    private readonly int _pollIntervalMs;

    private volatile bool _timedOut;
    private volatile bool _succeeded;

    public bool TimedOut => _timedOut;

    /// <summary>
    /// True only after WaitForCompletion confirmed Done within the deadline.
    /// False before first poll, after timeout, and after any error.
    /// </summary>
    public bool IsSuccessful => _succeeded && !_timedOut;

    public CalculationGuard(
        Func<CalculationState> getState,
        Action<int> wait,
        Func<long> elapsedMs,
        long timeoutMs,
        int pollIntervalMs = 200)
    {
        _getState       = getState;
        _wait           = wait;
        _elapsedMs      = elapsedMs;
        _timeoutMs      = timeoutMs;
        _pollIntervalMs = pollIntervalMs;
    }

    /// <summary>
    /// Poll until calculation is done or timeout is reached.
    /// Returns true if calculation completed within the deadline, false on timeout.
    /// Once this method returns false, TimedOut is permanently true.
    /// </summary>
    public bool WaitForCompletion()
    {
        if (_timedOut) return false;

        while (true)
        {
            // Check deadline BEFORE potentially blocking state read
            if (_elapsedMs() >= _timeoutMs)
            {
                _timedOut = true;
                return false;
            }

            CalculationState state;
            try { state = _getState(); }
            catch (Exception ex)
            {
                _timedOut = true;
                throw new InvalidOperationException(
                    $"CalculationGuard: error reading calculation state: {ex.Message}", ex);
            }

            // A blocking state read may have consumed the remaining deadline.
            if (_elapsedMs() >= _timeoutMs)
            {
                _timedOut = true;
                return false;
            }

            if (state == CalculationState.Done)
            {
                _succeeded = true;
                return true;
            }

            // Bound wait to remaining time
            long remaining = _timeoutMs - _elapsedMs();
            int waitMs = remaining > 0
                ? (int)Math.Min(_pollIntervalMs, remaining)
                : 0;

            if (waitMs > 0)
                _wait(waitMs);
        }
    }
}

internal enum CalculationState
{
    Done        = 0,  // xlDone
    Calculating = 1,  // xlCalculating
    Pending     = 2   // xlPending
}