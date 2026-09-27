// This file is only compiled on Windows targets.
// It implements Excel COM recalculation via a supervisor + STA worker pattern.
// No Office Interop package is used; all COM calls are made via dynamic dispatch.
#if WINDOWS
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;

namespace WorkbookTextGuard.Commands;

/// <summary>
/// Windows-only Excel COM adapter.
/// Architecture:
///   - Supervisor thread: owns Job Object handle, enforces hard timeouts, kills on failure.
///   - STA worker thread: all COM calls (Open, CalculateFullRebuild, Save, Close, Quit).
/// No GetActiveObject; always starts a fresh Excel instance.
/// </summary>
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
internal static class ExcelComAdapter
{
    // Hard timeouts (milliseconds)
    private const int ActivationTimeoutMs  =  10_000;
    private const int OpenTimeoutMs        =  30_000;
    private const int CalcTimeoutMs        = 300_000;
    private const int SaveTimeoutMs        =  30_000;
    private const int CloseQuitTimeoutMs   =  15_000;
    private const int HwndPollIntervalMs   =      50;
    private const int HwndPollTotalMs      =   2_000;

    // COM HRESULT: VBA_E_IGNORE — Excel busy, retry
    private const int VBA_E_IGNORE = unchecked((int)0x800AC472);

    public static int Recalculate(string inputPath, string outputPath, string reportPath)
    {
        string absInput  = Path.GetFullPath(inputPath);
        string absOutput = Path.GetFullPath(outputPath);

        Console.WriteLine($"[recalculate] Input:  {absInput}");
        Console.WriteLine($"[recalculate] Output: {absOutput}");

        // Work on a separate copy so the original is never touched by Excel
        string workCopy = absOutput + ".tmp_calc";
        File.Copy(absInput, workCopy, overwrite: true);

        string inputSha256 = HashFile(absInput);
        Console.WriteLine($"[recalculate] Input SHA-256: {inputSha256}");

        var workerResult = new WorkerResult();

        // STA worker thread
        var workerThread = new Thread(() =>
        {
            try { RunComWorker(workCopy, absOutput, workerResult); }
            catch (Exception ex)
            {
                workerResult.Supervisor.RecordWorkerResult(WorkflowResult.Failure($"Worker exception: {ex.Message}"));
            }
        });
        workerThread.SetApartmentState(ApartmentState.STA);
        workerThread.IsBackground = true;
        workerThread.Start();

        // Supervisor: wait for worker with total timeout
        int totalMs = ActivationTimeoutMs + OpenTimeoutMs + CalcTimeoutMs + SaveTimeoutMs + CloseQuitTimeoutMs;
        bool finished = workerThread.Join(totalMs);
        // Freeze the reporting decision BEFORE containment/cleanup waits. A worker
        // returning from SaveAs (or cleanup) after this deadline cannot restore success.
        WorkflowResult decision = workerResult.Supervisor.Complete(finished);

        if (!finished)
        {
            Console.Error.WriteLine("[recalculate] Supervisor: worker timed out. Attempting to kill Excel process.");
            workerResult.KillExcel("Supervisor timeout");
            workerThread.Join(5_000);
        }

        // Clean up temp copy
        try { if (File.Exists(workCopy)) File.Delete(workCopy); } catch { /* best-effort */ }

        if (!decision.Succeeded)
        {
            Console.Error.WriteLine($"[recalculate] Failed: {decision.FailureReason}");
            RecalculateCommand.WriteFailureReport(reportPath, absInput, decision.FailureReason ?? "Unknown error");
            return 2;
        }

        string outputSha256 = HashFile(absOutput);
        Console.WriteLine($"[recalculate] Output SHA-256: {outputSha256}");

        WriteSuccessReport(reportPath, absInput, inputSha256, absOutput, outputSha256, workerResult.ExcelVersion);
        Console.WriteLine($"[recalculate] Report written: {reportPath}");
        Console.WriteLine("[recalculate] Recalculation complete. Run 'check --require-calculated --calculation-report' to verify.");
        return 0;
    }

    private static void RunComWorker(string workCopy, string outputPath, WorkerResult result)
    {
        dynamic? excel = null;
        dynamic? workbook = null;
        int excelPid = 0;
        IntPtr jobHandle = IntPtr.Zero;

        try
        {
            // ── Activate Excel ───────────────────────────────────────────────
            var excelType = Type.GetTypeFromProgID("Excel.Application")
                ?? throw new InvalidOperationException("Excel is not installed or not registered.");

            excel = Activator.CreateInstance(excelType)
                ?? throw new InvalidOperationException("Failed to create Excel.Application COM instance.");

            // Pre-open settings
            excel.DisplayAlerts       = false;
            excel.AskToUpdateLinks    = false;
            excel.EnableEvents        = false;
            excel.ScreenUpdating      = false;
            excel.Visible             = false;
            excel.AutomationSecurity  = 3; // msoAutomationSecurityForceDisable

            // ── Poll for HWND (max 2 seconds, 50ms intervals) ────────────────
            var hwndSw = Stopwatch.StartNew();
            IntPtr hwnd = IntPtr.Zero;
            while (hwndSw.ElapsedMilliseconds < HwndPollTotalMs)
            {
                try
                {
                    long h = (long)excel.Hwnd;
                    if (h != 0) { hwnd = new IntPtr(h); break; }
                }
                catch (COMException ce) when (ce.HResult == VBA_E_IGNORE)
                {
                    Thread.Sleep(HwndPollIntervalMs);
                    continue;
                }
                Thread.Sleep(HwndPollIntervalMs);
            }

            if (hwnd == IntPtr.Zero)
                throw new TimeoutException("Excel HWND not obtained within 2 seconds.");

            // ── Establish PID from HWND ──────────────────────────────────────
            GetWindowThreadProcessId(hwnd, out uint pid);
            if (pid == 0)
                throw new InvalidOperationException("Could not determine Excel PID from HWND.");

            excelPid = (int)pid;
            result.ExcelPid = excelPid;

            var excelProcess = Process.GetProcessById(excelPid);
            result.ExcelProcess = excelProcess;

            // ── Job Object registration ──────────────────────────────────────
            jobHandle = CreateJobObject(IntPtr.Zero, null);
            if (jobHandle != IntPtr.Zero)
            {
                var info = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION();
                info.BasicLimitInformation.LimitFlags = JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;
                int infoSize = Marshal.SizeOf(info);
                IntPtr infoPtr = Marshal.AllocHGlobal(infoSize);
                try
                {
                    Marshal.StructureToPtr(info, infoPtr, false);
                    SetInformationJobObject(jobHandle, 9 /*ExtendedLimitInformation*/, infoPtr, (uint)infoSize);
                }
                finally { Marshal.FreeHGlobal(infoPtr); }

                IntPtr procHandle = OpenProcess(PROCESS_ALL_ACCESS, false, (uint)excelPid);
                if (procHandle != IntPtr.Zero)
                {
                    bool assigned = AssignProcessToJobObject(jobHandle, procHandle);
                    CloseHandle(procHandle);
                    result.ContainmentMode = assigned ? "JOB_CONTAINED" : "SUPERVISOR_ONLY";
                    if (!assigned)
                        Console.WriteLine("[recalculate] Job Object assignment failed (possibly nested job). Using SUPERVISOR_ONLY mode.");
                }
                else
                {
                    result.ContainmentMode = "SUPERVISOR_ONLY";
                    Console.WriteLine("[recalculate] Could not open Excel process handle. Using SUPERVISOR_ONLY mode.");
                }
            }
            else
            {
                result.ContainmentMode = "SUPERVISOR_ONLY";
                Console.WriteLine("[recalculate] CreateJobObject failed. Using SUPERVISOR_ONLY mode.");
            }

            result.ExcelVersion = (string)excel.Version;
            Console.WriteLine($"[recalculate] Excel {result.ExcelVersion} activated (PID={excelPid}, mode={result.ContainmentMode}).");

            // ── Open workbook ────────────────────────────────────────────────
            workbook = excel.Workbooks.Open(
                workCopy,
                UpdateLinks: 0,
                ReadOnly: false,
                Format: Type.Missing,
                Password: Type.Missing,
                WriteResPassword: Type.Missing,
                IgnoreReadOnlyRecommended: true,
                Origin: Type.Missing,
                Delimiter: Type.Missing,
                Editable: Type.Missing,
                Notify: false,
                Converter: Type.Missing,
                AddToMru: false,
                Local: Type.Missing,
                CorruptLoad: Type.Missing);

            // Include time spent inside the potentially blocking trigger in the
            // calculation deadline, not just the subsequent CalculationState reads.
            var calculationClock = Stopwatch.StartNew();
            var guard = new CalculationGuard(
                getState: () => (CalculationState)(int)excel.CalculationState,
                wait: Thread.Sleep,
                elapsedMs: () => calculationClock.ElapsedMilliseconds,
                timeoutMs: CalcTimeoutMs);
            var workflow = new RecalculationWorkflow(
                triggerCalculation: () => excel.CalculateFullRebuild(),
                saveOutput: () => workbook.SaveAs(
                    Filename: outputPath,
                    FileFormat: 51, // xlOpenXMLWorkbook (.xlsx)
                    Password: Type.Missing,
                    WriteResPassword: Type.Missing,
                    ReadOnlyRecommended: false,
                    CreateBackup: false,
                    AccessMode: 1, // xlNoChange
                    ConflictResolution: Type.Missing,
                    AddToMru: false,
                    TextCodepage: Type.Missing,
                    TextVisualLayout: Type.Missing,
                    Local: Type.Missing),
                guard: guard);

            // Only a successful trigger -> guard -> SaveAs workflow can publish success.
            // Reporting additionally requires completion (including cleanup) in time.
            result.Supervisor.RecordWorkerResult(workflow.Execute());
        }
        finally
        {
            // ── Cleanup: Close → Quit → Release RCWs ────────────────────────
            if (workbook is not null)
            {
                try { workbook.Close(SaveChanges: false); }
                catch (Exception ex) { Console.Error.WriteLine($"[recalculate] Workbook.Close error: {ex.Message}"); }
                finally
                {
                    try { Marshal.FinalReleaseComObject(workbook); } catch { }
                    workbook = null;
                }
            }

            if (excel is not null)
            {
                try { excel.Quit(); }
                catch (Exception ex) { Console.Error.WriteLine($"[recalculate] Application.Quit error: {ex.Message}"); }
                finally
                {
                    try { Marshal.FinalReleaseComObject(excel); } catch { }
                    excel = null;
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            if (jobHandle != IntPtr.Zero)
            {
                CloseHandle(jobHandle);
                jobHandle = IntPtr.Zero;
            }
        }
    }

    private static string HashFile(string path)
    {
        using var fs = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(fs)).ToLowerInvariant();
    }

    private static void WriteSuccessReport(
        string reportPath, string inputFile, string inputSha256,
        string outputFile, string outputSha256, string? excelVersion)
    {
        var report = new
        {
            success      = true,
            inputFile,
            inputSha256,
            outputFile,
            outputSha256,
            excelVersion,
            generatedUtc = DateTime.UtcNow.ToString("o")
        };
        File.WriteAllText(reportPath,
            JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }),
            System.Text.Encoding.UTF8);
    }

    // ── P/Invoke ─────────────────────────────────────────────────────────────

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string? lpName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetInformationJobObject(IntPtr hJob, int JobObjectInfoClass, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AssignProcessToJobObject(IntPtr hJob, IntPtr hProcess);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    private const uint PROCESS_ALL_ACCESS = 0x1F0FFF;
    private const uint JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x2000;

    [StructLayout(LayoutKind.Sequential)]
    private struct JOBOBJECT_BASIC_LIMIT_INFORMATION
    {
        public long PerProcessUserTimeLimit;
        public long PerJobUserTimeLimit;
        public uint LimitFlags;
        public UIntPtr MinimumWorkingSetSize;
        public UIntPtr MaximumWorkingSetSize;
        public uint ActiveProcessLimit;
        public UIntPtr Affinity;
        public uint PriorityClass;
        public uint SchedulingClass;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct IO_COUNTERS
    {
        public ulong ReadOperationCount, WriteOperationCount, OtherOperationCount;
        public ulong ReadTransferCount, WriteTransferCount, OtherTransferCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
    {
        public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
        public IO_COUNTERS IoInfo;
        public UIntPtr ProcessMemoryLimit;
        public UIntPtr JobMemoryLimit;
        public UIntPtr PeakProcessMemoryUsed;
        public UIntPtr PeakJobMemoryUsed;
    }
}

/// <summary>Shared state between supervisor and STA worker thread.</summary>
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
internal sealed class WorkerResult
{
    public RecalculationSupervisor Supervisor { get; } = new();
    public string? ExcelVersion { get; set; }
    public int ExcelPid { get; set; }
    public Process? ExcelProcess { get; set; }
    public string ContainmentMode { get; set; } = "NONE";

    public void KillExcel(string reason)
    {
        if (ExcelProcess is null) return;
        try
        {
            if (!ExcelProcess.HasExited)
            {
                ExcelProcess.Kill(entireProcessTree: false);
                Console.Error.WriteLine($"[recalculate] Killed Excel PID={ExcelPid}: {reason}");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[recalculate] Failed to kill Excel PID={ExcelPid}: {ex.Message}");
        }
    }
}
#endif