// MBBS Launcher - App Manager Form
// Created by Mark Laudenbach with Love in Iowa
// https://github.com/laudenbachm/MBBS-Launcher
//
// File: Forms/AppManagerForm.cs
// Version: v1.70
//
// Change History:
// 26.02.11.1 - Initial creation for v1.6 Beta
// 26.02.19.1 - v1.60 - Layout fix: added blank-line top padding in program list to prevent clipping
// 26.02.19.2 - v1.70 - Added opacity/transparency slider with INI persistence
// 26.02.19.3 - v1.70 - Fix: SaveSettings guarded with _initialized flag to prevent Opacity=0 being written during init
// 26.02.19.5 - v1.70 - Resizable form: Anchor-based layout, WndProc bottom-edge resize, LastHeight INI persistence
// 26.02.19.6 - v1.70 - Fix countdown timer not showing: solid BackColor + DPI-aware label sizing
//                      Designer controls are auto-scaled by AutoScaleMode.Font; runtime controls are not.
//                      At 150%+ DPI "Launch 0:30" overflows fixed-width labels, clipping the time.
//                      Fix: compute DeviceDpi/96 scale factor and apply to all row coordinates.
// 26.02.19.8 - v1.70 - Fix status label truncation: "Launch" wraps at space, hiding "0:30".
//                      Root cause: 110px status label + Consolas 11pt + "Launch 0:30" = wrap at space.
//                      Fix: widen form to 310px, increase statusW base from 110 to 140.
// 26.02.19.7 - v1.70 - Fix BBS showing Crashed instead of Stopped on clean shutdown

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MBBSLauncher.Core;
using MBBSLauncher.Models;

namespace MBBSLauncher.Forms
{
    /// <summary>
    /// App Manager - Shows real-time status of BBS and auto-launch programs.
    /// Displays countdowns, running status, and crash detection.
    /// </summary>
    public partial class AppManagerForm : Form
    {
        #region Win32 API for Dragging

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        private const int WM_NCHITTEST = 0x84;
        private const int HTBOTTOM = 15;

        #endregion

        private readonly AutoLaunchManager _autoLaunchManager;
        private readonly ConfigManager _config;
        private readonly List<ManagedProgram> _programs;
        private readonly Dictionary<string, Label> _statusLabels;
        private readonly Timer _updateTimer;
        private readonly Panel _programListPanel;
        private Button? _cancelButton;

        // Configuration
        private bool _alwaysOnTop;
        private bool _autoHide;
        private bool _initialized = false; // Guard to prevent SaveSettings during init
        private int _lastContentHeight = 130; // Y position after last program row, used to place cancel button

        // BBS monitoring
        private bool _bbsWasRunning = false;

        public AppManagerForm(AutoLaunchManager autoLaunchManager, ConfigManager config)
        {
            _autoLaunchManager = autoLaunchManager;
            _config = config;
            _programs = new List<ManagedProgram>();
            _statusLabels = new Dictionary<string, Label>();

            InitializeComponent();

            // Create program list panel — anchored to all 4 sides so it stretches when user resizes form.
            // Width uses the form's CURRENT ClientSize (already DPI-scaled by AutoScaleMode.Font)
            // so the panel fills the available width at any DPI rather than being pinned to 260px.
            _programListPanel = new Panel
            {
                Location = new Point(10, 35),
                Size = new Size(this.ClientSize.Width - 20, 140),
                BackColor = Color.Transparent,
                AutoScroll = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            this.Controls.Add(_programListPanel);

            // Subscribe to auto-launch events
            _autoLaunchManager.CountdownTick += OnCountdownTick;
            _autoLaunchManager.ProgramLaunched += OnProgramLaunched;
            _autoLaunchManager.AllLaunchesCancelled += OnAllLaunchesCancelled;

            // Update timer (check running status every 2 seconds)
            _updateTimer = new Timer { Interval = 2000 };
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();

            LoadSettings();
            _initialized = true; // Allow SaveSettings from here on
            InitializePrograms();
            UpdateDisplay();
        }

        /// <summary>
        /// Loads configuration settings.
        /// </summary>
        private void LoadSettings()
        {
            // Load window position
            int x = _config.GetInt("AppManager", "LastX", Screen.PrimaryScreen!.WorkingArea.Width - this.Width - 20);
            int y = _config.GetInt("AppManager", "LastY", 20);
            this.Location = new Point(x, y);

            // Load saved height (resizable form)
            int lastHeight = _config.GetInt("AppManager", "LastHeight", 300);
            lastHeight = Math.Clamp(lastHeight, this.MinimumSize.Height, 800);
            this.ClientSize = new Size(this.ClientSize.Width, lastHeight);

            // Load options
            _alwaysOnTop = _config.GetBool("AppManager", "AlwaysOnTop", true);
            _autoHide = _config.GetBool("AppManager", "AutoHide", false);

            // Load opacity (20–100, default 60)
            int opacityPct = _config.GetInt("AppManager", "Opacity", 60);
            opacityPct = Math.Clamp(opacityPct, 20, 100);
            opacityTrackBar.Value = opacityPct;
            opacityValueLabel.Text = $"{opacityPct}%";
            this.Opacity = opacityPct / 100.0;

            this.TopMost = _alwaysOnTop;
        }

        /// <summary>
        /// Saves configuration settings.
        /// </summary>
        private void SaveSettings()
        {
            if (!_initialized) return; // Don't save during InitializeComponent/LoadSettings

            _config.SetValue("AppManager", "LastX", this.Location.X.ToString());
            _config.SetValue("AppManager", "LastY", this.Location.Y.ToString());
            _config.SetValue("AppManager", "LastHeight", this.ClientSize.Height.ToString());
            _config.SetValue("AppManager", "AlwaysOnTop", _alwaysOnTop.ToString().ToLower());
            _config.SetValue("AppManager", "AutoHide", _autoHide.ToString().ToLower());
            _config.SetValue("AppManager", "Opacity", opacityTrackBar.Value.ToString());
            _config.SaveConfig();
        }

        /// <summary>
        /// Initializes the program list.
        /// </summary>
        private void InitializePrograms()
        {
            _programs.Clear();

            // Add BBS
            string bbsPath = _config.GetValue("Paths", "BBSPath", "");
            if (!string.IsNullOrEmpty(bbsPath))
            {
                string bbsExe = Path.Combine(bbsPath, "wgsappgo.exe");
                if (File.Exists(bbsExe))
                {
                    _programs.Add(new ManagedProgram
                    {
                        Name = "The Major BBS",
                        Path = bbsExe,
                        ProcessName = "wgserver", // wgsappgo launches wgserver
                        IsBBS = true,
                        Status = ProcessHelper.IsProcessRunning("wgserver")
                            ? ProgramStatus.Running
                            : ProgramStatus.Stopped
                    });
                }
            }

            // Add auto-launch programs
            var autoLaunchPrograms = _autoLaunchManager.GetEnabledPrograms();
            foreach (var program in autoLaunchPrograms)
            {
                _programs.Add(new ManagedProgram
                {
                    Name = program.Name,
                    Path = program.Path,
                    Arguments = program.Arguments,
                    ProcessName = ManagedProgram.GetProcessNameFromPath(program.Path),
                    IsBBS = false,
                    AutoLaunchId = program.Id,
                    LaunchMinimized = program.LaunchMinimized,
                    Status = ProcessHelper.IsProcessRunning(
                        ManagedProgram.GetProcessNameFromPath(program.Path))
                        ? ProgramStatus.Running
                        : ProgramStatus.Stopped
                });
            }
        }

        /// <summary>
        /// Updates the display with current program status.
        /// </summary>
        private void UpdateDisplay()
        {
            // Dispose cancel button before clearing panel so UpdateCancelButton creates a fresh one
            if (_cancelButton != null)
            {
                _cancelButton.Dispose();
                _cancelButton = null;
            }
            _programListPanel.Controls.Clear();
            _statusLabels.Clear();

            // Designer controls are auto-scaled by AutoScaleMode.Font at runtime, but controls
            // created in code are NOT. At 150%+ display scale, 11pt Consolas characters are wide
            // enough to overflow fixed pixel widths — "0:30" gets clipped, leaving only "Launch".
            // Fix: scale all coordinates by DeviceDpi/96 so labels grow with the display scale.
            float sf = Math.Max(1f, this.DeviceDpi / 96f);
            int startY  = (int)(22 * sf);
            int rowH    = (int)(26 * sf);
            int labelH  = (int)(24 * sf);
            int nameW   = (int)(140 * sf);
            int statusX = (int)(145 * sf);
            int statusW = (int)(140 * sf);
            int sepW    = (int)(250 * sf);
            int sepGap  = (int)(6  * sf);

            int yPos = startY;

            foreach (var program in _programs)
            {
                // Program name label
                var nameLabel = new Label
                {
                    AutoSize = false,
                    Text = program.Name,
                    Location = new Point(5, yPos),
                    Size = new Size(nameW, labelH),
                    ForeColor = program.IsBBS ? Color.White : Color.LightGray,
                    Font = new Font("Consolas", 11, FontStyle.Bold),
                    BackColor = Color.FromArgb(0, 0, 128),
                    Tag = program
                };

                // Right-click menu for non-BBS programs
                if (!program.IsBBS)
                {
                    nameLabel.MouseUp += ProgramLabel_MouseUp;
                    nameLabel.Cursor = Cursors.Hand;
                }

                _programListPanel.Controls.Add(nameLabel);

                // Status label — solid BackColor so changing Text triggers a clean repaint.
                var statusLabel = new Label
                {
                    AutoSize = false,
                    Text = program.GetStatusText(),
                    Location = new Point(statusX, yPos),
                    Size = new Size(statusW, labelH),
                    ForeColor = GetStatusColor(program.Status),
                    Font = new Font("Consolas", 11, FontStyle.Regular),
                    BackColor = Color.FromArgb(0, 0, 128),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Tag = program
                };

                _programListPanel.Controls.Add(statusLabel);
                _statusLabels[program.Name] = statusLabel;

                yPos += rowH;

                // Add separator after BBS
                if (program.IsBBS)
                {
                    var separator = new Label
                    {
                        Location = new Point(5, yPos),
                        Size = new Size(sepW, 2),
                        BackColor = Color.Cyan,
                        BorderStyle = BorderStyle.None
                    };
                    _programListPanel.Controls.Add(separator);
                    yPos += sepGap;
                }
            }

            // Track last content Y for cancel button placement within the panel
            _lastContentHeight = yPos;
            UpdateCancelButton();
        }

        /// <summary>
        /// Gets the color for a program status.
        /// </summary>
        private Color GetStatusColor(ProgramStatus status)
        {
            return status switch
            {
                ProgramStatus.Running => Color.FromArgb(0, 255, 0), // Bright green
                ProgramStatus.Pending => Color.FromArgb(255, 255, 0), // Bright yellow
                ProgramStatus.Stopped => Color.Gray,
                ProgramStatus.Crashed => Color.Red,
                _ => Color.White
            };
        }

        /// <summary>
        /// Right-click handler for program labels.
        /// </summary>
        private void ProgramLabel_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            if (sender is Label label && label.Tag is ManagedProgram program)
            {
                ShowProgramContextMenu(program, label, e.Location);
            }
        }

        /// <summary>
        /// Shows context menu for a program.
        /// </summary>
        private void ShowProgramContextMenu(ManagedProgram program, Control control, Point location)
        {
            var menu = new ContextMenuStrip();

            if (program.CanLaunch)
            {
                var launchItem = new ToolStripMenuItem("Launch Now");
                launchItem.Click += (s, e) => LaunchProgram(program);
                menu.Items.Add(launchItem);
            }

            if (program.CanCancelLaunch)
            {
                var cancelItem = new ToolStripMenuItem("Cancel Launch");
                cancelItem.Click += (s, e) => CancelProgramLaunch(program);
                menu.Items.Add(cancelItem);
            }

            if (program.CanStop)
            {
                var stopItem = new ToolStripMenuItem("Stop Application");
                stopItem.Click += (s, e) => StopProgram(program);
                menu.Items.Add(stopItem);
            }

            if (menu.Items.Count == 0)
            {
                var noActionsItem = new ToolStripMenuItem("No actions available");
                noActionsItem.Enabled = false;
                menu.Items.Add(noActionsItem);
            }

            menu.Show(control, location);
        }

        /// <summary>
        /// Launches a program immediately.
        /// </summary>
        private void LaunchProgram(ManagedProgram program)
        {
            try
            {
                string? workingDir = Path.GetDirectoryName(program.Path);
                var process = ProcessHelper.LaunchProgram(
                    program.Path,
                    workingDir,
                    string.IsNullOrWhiteSpace(program.Arguments) ? null : program.Arguments,
                    program.LaunchMinimized);

                if (process != null)
                {
                    program.Status = ProgramStatus.Running;
                    UpdateStatusLabel(program);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error launching {program.Name}:\n\n{ex.Message}",
                    "Launch Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Cancels a pending program launch.
        /// </summary>
        private void CancelProgramLaunch(ManagedProgram program)
        {
            if (!string.IsNullOrEmpty(program.AutoLaunchId))
            {
                _autoLaunchManager.CancelProgramLaunch(program.AutoLaunchId);
                program.Status = ProgramStatus.Stopped;
                program.SecondsRemaining = 0;
                UpdateStatusLabel(program);
                UpdateCancelButton();
            }
        }

        /// <summary>
        /// Stops a running program.
        /// </summary>
        private void StopProgram(ManagedProgram program)
        {
            var result = MessageBox.Show(
                $"Stop {program.Name}?\n\nThis will force-close the application.",
                "Confirm Stop",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                int killed = ProcessHelper.ForceKillProcess(program.ProcessName);
                if (killed > 0)
                {
                    program.Status = ProgramStatus.Stopped;
                    UpdateStatusLabel(program);
                }
            }
        }

        /// <summary>
        /// Updates the cancel button visibility.
        /// </summary>
        private void UpdateCancelButton()
        {
            bool hasPending = _programs.Any(p => p.Status == ProgramStatus.Pending);

            if (hasPending && _cancelButton == null)
            {
                float sf = Math.Max(1f, this.DeviceDpi / 96f);
                // Create cancel button inside the scrollable panel so it appears below program rows
                _cancelButton = new Button
                {
                    Text = "Cancel Auto-Launches",
                    Location = new Point((int)(30 * sf), _lastContentHeight + 4),
                    Size = new Size((int)(200 * sf), (int)(25 * sf)),
                    BackColor = Color.FromArgb(64, 64, 128),
                    ForeColor = Color.White,
                    Font = new Font("Consolas", 9, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                _cancelButton.FlatAppearance.BorderColor = Color.Cyan;
                _cancelButton.Click += CancelButton_Click;
                _programListPanel.Controls.Add(_cancelButton);
            }
            else if (!hasPending && _cancelButton != null)
            {
                _programListPanel.Controls.Remove(_cancelButton);
                _cancelButton.Dispose();
                _cancelButton = null;
            }
        }

        /// <summary>
        /// Cancel button click handler.
        /// </summary>
        private void CancelButton_Click(object? sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Cancel all pending auto-launches?",
                "Confirm Cancel",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _autoLaunchManager.StopAllLaunches();
            }
        }

        /// <summary>
        /// Handles countdown tick events from AutoLaunchManager.
        /// </summary>
        private void OnCountdownTick(object? sender, AutoLaunchCountdownEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => OnCountdownTick(sender, e)));
                return;
            }

            var program = _programs.FirstOrDefault(p => p.AutoLaunchId == e.ProgramId);
            if (program != null)
            {
                program.Status = ProgramStatus.Pending;
                program.SecondsRemaining = e.SecondsRemaining;
                UpdateStatusLabel(program);
            }
        }

        /// <summary>
        /// Handles program launched events from AutoLaunchManager.
        /// </summary>
        private void OnProgramLaunched(object? sender, AutoLaunchEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => OnProgramLaunched(sender, e)));
                return;
            }

            var program = _programs.FirstOrDefault(p => p.AutoLaunchId == e.ProgramId);
            if (program != null)
            {
                program.Status = e.Success ? ProgramStatus.Running : ProgramStatus.Stopped;
                program.SecondsRemaining = 0;
                UpdateStatusLabel(program);
                UpdateCancelButton();
            }

            // Check if all launches complete and auto-hide enabled
            if (_autoHide && !_programs.Any(p => p.Status == ProgramStatus.Pending))
            {
                this.Hide();
            }
        }

        /// <summary>
        /// Handles all launches cancelled event.
        /// </summary>
        private void OnAllLaunchesCancelled(object? sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => OnAllLaunchesCancelled(sender, e)));
                return;
            }

            foreach (var program in _programs.Where(p => p.Status == ProgramStatus.Pending))
            {
                program.Status = ProgramStatus.Stopped;
                program.SecondsRemaining = 0;
                UpdateStatusLabel(program);
            }

            UpdateCancelButton();
        }

        /// <summary>
        /// Updates a single program's status label.
        /// </summary>
        private void UpdateStatusLabel(ManagedProgram program)
        {
            if (_statusLabels.TryGetValue(program.Name, out var label))
            {
                label.Text = program.GetStatusText();
                label.ForeColor = GetStatusColor(program.Status);
            }
        }

        /// <summary>
        /// Timer tick - check running status of all programs.
        /// </summary>
        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            foreach (var program in _programs)
            {
                // Skip programs with pending countdown
                if (program.Status == ProgramStatus.Pending)
                    continue;

                bool isRunning = ProcessHelper.IsProcessRunning(program.ProcessName);

                // Detect stopped (was running, now stopped — could be clean shutdown or crash)
                if (program.Status == ProgramStatus.Running && !isRunning)
                {
                    program.Status = ProgramStatus.Stopped;
                    UpdateStatusLabel(program);

                    // If BBS stopped, restore main window
                    if (program.IsBBS)
                    {
                        OnBBSCrashed();
                    }
                }
                // Update running status
                else if (program.Status != ProgramStatus.Running && isRunning)
                {
                    program.Status = ProgramStatus.Running;
                    UpdateStatusLabel(program);
                }
                else if (program.Status != ProgramStatus.Pending && !isRunning)
                {
                    program.Status = ProgramStatus.Stopped;
                    UpdateStatusLabel(program);
                }

                // Track BBS running state
                if (program.IsBBS)
                {
                    _bbsWasRunning = (program.Status == ProgramStatus.Running);
                }
            }
        }

        /// <summary>
        /// Called when BBS crashes.
        /// </summary>
        private void OnBBSCrashed()
        {
            // Fire event to restore main launcher window
            BBSCrashed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event fired when BBS crashes.
        /// </summary>
        public event EventHandler? BBSCrashed;

        /// <summary>
        /// Shows the App Manager and brings to front.
        /// </summary>
        public new void Show()
        {
            base.Show();
            this.BringToFront();
            InitializePrograms();
            UpdateDisplay();
        }

        /// <summary>
        /// Enables bottom-edge resize on this borderless form.
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_NCHITTEST)
            {
                var cursor = this.PointToClient(Cursor.Position);
                if (cursor.Y >= this.ClientSize.Height - 6)
                {
                    m.Result = (IntPtr)HTBOTTOM;
                }
            }
        }

        /// <summary>
        /// Form closing - save settings.
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Don't actually close, just hide
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }

            SaveSettings();
            base.OnFormClosing(e);
        }

        /// <summary>
        /// Make title bar draggable.
        /// </summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left && e.Y < 30) // Title bar area
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
    }
}
