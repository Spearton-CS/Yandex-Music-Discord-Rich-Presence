namespace YandexMusicRPC
{
    public static class URLGenerator
    {
        private static string GeneratePlayerUrl(string type, string id) => $"https://music.yandex.ru/{type}/{id}";
        public static string Artist(string id) => GeneratePlayerUrl("artist", id);
        public static string Album(string id) => GeneratePlayerUrl("album", id);
        public static string Track(string id) => GeneratePlayerUrl("track", id);
        public static string Image(string dbURL, string x1, string x2) => $"https://{dbURL[..^2]}{x1}x{x2}";
    }
}