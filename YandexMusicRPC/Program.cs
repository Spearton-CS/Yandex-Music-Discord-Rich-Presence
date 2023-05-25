namespace YandexMusicRPC
{
    internal static class Program
    {
        public static string DBPath { get; set; } = string.Empty;
        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();
            ToastNotificationManagerCompat.OnActivated += toastArgs => { };
            Detector.DetectInstalledYM();
            Application.Run(new MainWindow());
            ToastNotificationManagerCompat.Uninstall();
        }
    }
}