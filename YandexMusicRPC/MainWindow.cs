using System.Diagnostics;

namespace YandexMusicRPC
{
    public partial class MainWindow : Form
    {
        private Settings Settings;
        private RPC RPC;
        private static string ToBgr(Color c) => $"{c.B:X2}{c.G:X2}{c.R:X2}";
        [System.Runtime.InteropServices.DllImport("DwmApi")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);
        private const int DWWMA_CAPTION_COLOR = 35;
        private const int DWWMA_BORDER_COLOR = 34;
        private const int DWMWA_TEXT_COLOR = 36;
        private Color ThemeBackColor
        {
            get
            {
                if (Settings.DarkTheme)
                    return Color.FromArgb(24, 24, 24);
                else
                    return Color.FromKnownColor(KnownColor.Control);
            }
        }
        private Color ThemeForeColor
        {
            get
            {
                if (Settings.DarkTheme)
                    return Color.White;
                else
                    return Color.FromKnownColor(KnownColor.ActiveCaptionText);
            }
        }
        private static int[] CreateDWMParams(Color c) => new int[] { int.Parse(ToBgr(c), System.Globalization.NumberStyles.HexNumber) };
        private void CustomWindow()
        {
            IntPtr hWnd = Handle;
            DwmSetWindowAttribute(hWnd, DWWMA_CAPTION_COLOR, CreateDWMParams(Settings.CaptionColor), 4);
            DwmSetWindowAttribute(hWnd, DWMWA_TEXT_COLOR, CreateDWMParams(ThemeForeColor), 4);
            DwmSetWindowAttribute(hWnd, DWWMA_BORDER_COLOR, CreateDWMParams(ThemeBackColor), 4);
            BackColor = ThemeBackColor;
            ForeColor = ThemeForeColor;
            ChangeSettingsButton.BackColor = ThemeBackColor;
            ChangeSettingsButton.ForeColor = ThemeForeColor;
            DetailsFormatLinkLabel.BackColor = ThemeBackColor;
            DetailsFormatLinkLabel.ForeColor = ThemeForeColor;
            FirstButtonLabel.BackColor = ThemeBackColor;
            FirstButtonLabel.ForeColor = ThemeForeColor;
            SecondButtonLabel.BackColor = ThemeBackColor;
            SecondButtonLabel.ForeColor = ThemeForeColor;
            SmallImageLabel.BackColor = ThemeBackColor;
            SmallImageLabel.ForeColor = ThemeForeColor;
            LargeImageLabel.BackColor = ThemeBackColor;
            LargeImageLabel.ForeColor = ThemeForeColor;
            FirstButtonDomainUpDown.BackColor = ThemeBackColor;
            FirstButtonDomainUpDown.ForeColor = ThemeForeColor;
            SecondButtonDomainUpDown.BackColor = ThemeBackColor;
            SecondButtonDomainUpDown.ForeColor = ThemeForeColor;
            SmallImageDomainUpDown.BackColor = ThemeBackColor;
            SmallImageDomainUpDown.ForeColor = ThemeForeColor;
            LargeImageDomainUpDown.BackColor = ThemeBackColor;
            LargeImageDomainUpDown.ForeColor = ThemeForeColor;
            AutoStartCheckBox.BackColor = ThemeBackColor;
            AutoStartCheckBox.ForeColor = ThemeForeColor;
            CaptionColorButton.BackColor = ThemeBackColor;
            CaptionColorButton.ForeColor = ThemeForeColor;
            DarkThemeCheckBox.BackColor = ThemeBackColor;
            DarkThemeCheckBox.ForeColor = ThemeForeColor;
            DetailsFormatTextBox.BackColor = ThemeBackColor;
            DetailsFormatTextBox.ForeColor = ThemeForeColor;
            RPCCheckBox.BackColor = ThemeBackColor;
            RPCCheckBox.ForeColor = ThemeForeColor;
            SettingsGroup.BackColor = ThemeBackColor;
            SettingsGroup.ForeColor = ThemeForeColor;
            SourceCodeLinkLabel.BackColor = ThemeBackColor;
            SourceCodeLinkLabel.ForeColor = ThemeForeColor;
            HideToTrayButton.BackColor = ThemeBackColor;
            HideToTrayButton.ForeColor = ThemeForeColor;
            openToolStripMenuItem.BackColor = ThemeBackColor;
            openToolStripMenuItem.ForeColor = ThemeForeColor;
            exitToolStripMenuItem.BackColor = ThemeBackColor;
            exitToolStripMenuItem.ForeColor = ThemeForeColor;
            TrayContextMenu.BackColor = ThemeBackColor;
            TrayContextMenu.ForeColor = ThemeForeColor;
        }
        private static readonly string AutoStartPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "ymRPC_AutoStart.cmd");
        private static readonly string AutoStartContent = @$"@echo off\r\nStart """" ""{Application.ExecutablePath}""";
        private static bool RPCEnable = true;
        private Detector Detector;
        private bool AutoStart
        {
            get
            {
                try
                {
                    return File.Exists(AutoStartPath) && File.ReadAllText(AutoStartPath) == AutoStartContent;
                }
                catch (Exception ex)
                {
                    Message.Show($"An error occurred while getting the value of 'AutoStart' ['{ex.Message}']");
                    return false;
                }
            }
            set
            {
                try
                {
                    if (value)
                    {
                        if (File.Exists(AutoStartPath))
                            File.Delete(AutoStartPath);
                        File.WriteAllText(AutoStartPath, AutoStartContent);
                    }
                    else if (File.Exists(AutoStartPath))
                        File.Delete(AutoStartPath);
                }
                catch (Exception ex)
                {
                    Message.Show($"An error occurred when setting the value of 'AutoStart' ['{ex.Message}']");
                    AutoStartCheckBox.Checked = AutoStart;
                }
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            Settings = new();
            RPC = new();
            Detector = new((info) =>
                    {
                        if (RPCEnable)
                            if (info is null)
                                RPC.Clear();
                            else
                                RPC.SetRPC(info, Settings.SmallImage, Settings.LargeImage, Settings.FirstButton, Settings.SecondButton, Settings.DetailsFormat);
                    });
            if (!RPC.Client.Initialize())
            {
                MessageBox.Show("Can't initialize Discord RPC connection!");
                ToastNotificationManagerCompat.Uninstall();
                Environment.Exit(0);
            }
            CaptionColorDialog.Color = Settings.CaptionColor;
            AutoStartCheckBox.Checked = AutoStart;
            if (Settings.DetailsFormat != Settings.DefaultDetailsFormat)
                DetailsFormatTextBox.Text = Settings.DetailsFormat;
            FirstButtonDomainUpDown.SelectedIndex = Settings.FirstButton + 1;
            SecondButtonDomainUpDown.SelectedIndex = Settings.SecondButton + 1;
            SmallImageDomainUpDown.SelectedIndex = Settings.SmallImage;
            LargeImageDomainUpDown.SelectedIndex = Settings.LargeImage;
            DarkThemeCheckBox.Checked = Settings.DarkTheme;
            CustomWindow();
        }
        private void DarkThemeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.DarkTheme = DarkThemeCheckBox.Checked;
            CustomWindow();
        }
        private void CaptionColorButton_Click(object sender, EventArgs e)
        {
            if (CaptionColorDialog.ShowDialog() == DialogResult.OK)
            {
                if (CaptionColorDialog.Color == Color.Transparent)
                {
                    Message.Show("It is impossible to repaint the window in a transparent color!");
                    return;
                }
                Settings.CaptionColor = CaptionColorDialog.Color;
                CustomWindow();
            }
        }
        private void AutoStartCheckBox_CheckedChanged(object sender, EventArgs e) => AutoStart = AutoStartCheckBox.Checked;
        private void DetailsFormatLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => MessageBox.Show("This property is used to customize the display of your activity in Yandex Music. Keywords that are automatically replaced by the program:\r\n{Track} - track being played;\r\n{Artist} - artist(s) of the track;\r\n{Genre} - genre of the track (in Yandex Music album);\r\n{Album} - the album that contains the track");
        private void ChangeSettingsButton_Click(object sender, EventArgs e)
        {
            if (DetailsFormatTextBox.Text != string.Empty)
                Settings.DetailsFormat = DetailsFormatTextBox.Text;
            Settings.FirstButton = FirstButtonDomainUpDown.SelectedIndex - 1;
            Settings.SecondButton = SecondButtonDomainUpDown.SelectedIndex - 1;
            Settings.SmallImage = SmallImageDomainUpDown.SelectedIndex;
            Settings.LargeImage = LargeImageDomainUpDown.SelectedIndex;
            Settings.Save();
        }
        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            Detector.Dispose();
            RPC.Clear();
            RPC.Client.Dispose();
        }
        private void RPCCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            RPCEnable = RPCCheckBox.Checked;
            if (!RPCEnable)
                RPC.Client.ClearPresence();
        }
        private void SourceCodeLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => Process.Start(new ProcessStartInfo() { FileName = "https://github.com/Spearton-CS/Yandex-Music-Discord-Rich-Presence", UseShellExecute = true });
        private void HideToTrayButton_Click(object sender, EventArgs e)
        {
            TrayIcon.Visible = true;
            Hide();
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TrayIcon.Visible = false;
            Show();
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TrayIcon.Visible = false;
            Close();
        }
    }
}