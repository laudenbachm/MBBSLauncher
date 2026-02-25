// MBBSLauncher - Restart Manager (Disabled in v1.5)
using System;
using MBBSLauncher.Models;

namespace MBBSLauncher.Core
{
    public class RestartManager
    {
        public event EventHandler<RestartCountdownEventArgs>? CountdownTick;
        public event EventHandler<RestartEventArgs>? RestartAttempted;
        public event EventHandler? RestartCancelled;

        public RestartManager(AutoRestartSettings settings) { }
        public void StartMonitoring(System.Diagnostics.Process process) { }
        public void StopMonitoring() { }
        public void SignalManualShutdown() { }
        public void CancelRestart() { }
        public RestartAction GetRestartAction(Func<bool> launchBBSCallback)
        {
            return new RestartAction { ShouldRestart = false, Reason = "Auto-restart is disabled" };
        }
    }

    public class RestartAction
    {
        public bool ShouldRestart { get; set; }
        public string Reason { get; set; } = string.Empty;
        public Func<System.Threading.Tasks.Task>? RestartCallback { get; set; }
    }

    public class RestartCountdownEventArgs : EventArgs
    {
        public int SecondsRemaining { get; set; }
        public int TotalSeconds { get; set; }
        public int AttemptNumber { get; set; }
        public int MaxAttempts { get; set; }
    }

    public class RestartEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public int AttemptNumber { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
