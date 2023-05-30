namespace YandexMusicRPC
{
    internal static class Program
    {
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