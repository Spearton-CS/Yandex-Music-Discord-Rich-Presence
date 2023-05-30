namespace YandexMusicRPC
{
    public static class Message
    {
        public static void Show(string message, bool log = false)
        {
            new ToastContentBuilder().AddText(message, AdaptiveTextStyle.Body).Show();
            if (log)
                Log(message);
        }
        public static void Log(string message) => File.AppendAllText("Messages.log", $"{DateTime.Now.ToLongDateString()}: {DateTime.Now.ToLongTimeString()}\n\r{message}\n\r");
    }
}