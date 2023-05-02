namespace YandexMusicRPC
{
    internal static class Program
    {
        private static string BatPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "ymRPC_autostart.bat");
        private static Detector Detector = new((string? track, string? artist, bool error) =>
        {
            if (error)
            {
                try
                {
                    RPC.client.ClearPresence();
                }
                catch
                {

                }
            }
            else
            {
#pragma warning disable CS8604
                if (!RPC.Set(new(track, artist)))
                    MessageBox.Show("Can't set Discord Rich Presence!", "YandexMusicRPC");
#pragma warning restore CS8604
            }
        });
        private static bool Exit = false;
        private static NotifyIcon TrayIcon = new();
        private static ContextMenuStrip TrayIconContextMenu = new();
        private static ToolStripMenuItem StopStartRPCTray = new();
        private static ToolStripMenuItem ExitTray = new();
        private static ToolStripMenuItem AutoStartTray = new();
        private static bool RPCEnable
        {
            get => Detector.Enabled;
            set
            {
                Detector.Enabled = value;
                if (!value)
                {
                    StopStartRPCTray.Text = "Start RPC";
                    try
                    {
                        RPC.client.ClearPresence();
                    }
                    catch
                    {

                    }
                }
                else
                    StopStartRPCTray.Text = "Stop RPC";
            }
        }
        private static bool AutoStart
        {
            get => File.Exists(BatPath);
            set
            {
                if (value)
                {
                    AutoStartTray.Text = "Auto start enabled";
                    File.WriteAllText(BatPath, $@"Start """" ""{Application.ExecutablePath}""");
                }
                else
                {
                    AutoStartTray.Text = "Auto start disabled";
                    File.Delete(BatPath);
                }
            }
        }
        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();
            RPC.client.Initialize();
            #region Initialize TrayIcon
            TrayIconContextMenu.SuspendLayout();
            Size ContextButtonSize = new(181, 92);
            TrayIconContextMenu.Items.AddRange(new ToolStripItem[] { StopStartRPCTray, ExitTray, AutoStartTray });
            TrayIconContextMenu.Name = "TrayIconContextMenu";
            TrayIconContextMenu.Size = ContextButtonSize;
            StopStartRPCTray.Name = "StopStartRPCTray";
            StopStartRPCTray.Text = "Stop RPC";
            StopStartRPCTray.Size = ContextButtonSize;
#pragma warning disable CS8622
            StopStartRPCTray.Click += StopStartRPC_Click;
            ExitTray.Name = "ExitTray";
            ExitTray.Text = "Exit";
            ExitTray.Click += Exit_Click;
            ExitTray.Size = ContextButtonSize;
            AutoStartTray.Name = "AutoStartTray";
            if (!AutoStart)
                AutoStartTray.Text = "Auto start disabled";
            else
                AutoStartTray.Text = "Auto start enabled";
            AutoStartTray.Click += AutoStart_Click;
            AutoStartTray.Size = ContextButtonSize;
            TrayIconContextMenu.ResumeLayout(false);
            TrayIcon.ContextMenuStrip = TrayIconContextMenu;
            TrayIcon.Icon = Properties.Resources.app;
            TrayIcon.Text = "Yandex music Discord RPC Provider";
#pragma warning restore CS8622
            #endregion
            if (File.Exists(BatPath))
                AutoStart = true;
            ShowTrayIcon();
            Application.Run();
            while (!Exit)
                Thread.Sleep(1000);
            HideTrayIcon();
            Detector.Dispose();
            TrayIcon.Dispose();
            TrayIconContextMenu.Dispose();
            AutoStartTray.Dispose();
            ExitTray.Dispose();
            StopStartRPCTray.Dispose();
            try
            {
                RPC.client.ClearPresence();
            }
            catch
            {

            }
        }
        private static void ShowTrayIcon() => TrayIcon.Visible = true;
        private static void HideTrayIcon() => TrayIcon.Visible = false;
        private static void StopStartRPC_Click(object sender, EventArgs e) => RPCEnable = !RPCEnable;
        private static void Exit_Click(object sender, EventArgs e)
        {
            HideTrayIcon();
            Detector.Dispose();
            TrayIcon.Dispose();
            TrayIconContextMenu.Dispose();
            AutoStartTray.Dispose();
            ExitTray.Dispose();
            StopStartRPCTray.Dispose();
            try
            {
                RPC.client.ClearPresence();
            }
            catch
            {

            }
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        private static void AutoStart_Click(object sender, EventArgs e) => AutoStart = !AutoStart;
    }
}