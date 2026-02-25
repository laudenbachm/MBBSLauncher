// MBBSLauncher - Auto Restart Settings (Disabled in v1.5)
namespace MBBSLauncher.Models
{
    public class AutoRestartSettings
    {
        public bool Enabled { get; set; } = false;
        public int RestartDelaySeconds { get; set; } = 30;
        public int MaxRestartAttempts { get; set; } = 3;
        public int RestartWindowMinutes { get; set; } = 10;
        public bool ShowNotifications { get; set; } = true;
    }
}
