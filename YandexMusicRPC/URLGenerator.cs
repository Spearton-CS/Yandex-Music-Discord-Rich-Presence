namespace YandexMusicRPC
{
    public static class URLGenerator
    {
        public static string TrackSearch(string track, string artist)
        {
            string UrlBase = "https://music.yandex.ru/search?text=";
            string text = $"{track}, {artist}";
            text = text.Replace("%", "%25");
            text = text.Replace(" ", "%20");
            text = text.Replace(",", "%2C");
            text = text.Replace("+", "%2B");
            text = text.Replace("=", "%3D");
            text = text.Replace("?", "%3F");
            text = text.Replace("&", "%26");
            text = text.Replace(":", "%3A");
            text = text.Replace("^", "%5E");
            text = text.Replace("$", "%24");
            text = text.Replace(";", "%3B");
            text = text.Replace("#", "%23");
            text = text.Replace("\"", "%22");
            text = text.Replace("@", "%40");
            text = text.Replace(">", "%3E");
            text = text.Replace("<", "%3C");
            text = text.Replace("/", "%2F");
            text = text.Replace("\\", "%5C");
            text = text.Replace("|", "%7C");
            text = text.Replace("]", "%5D");
            text = text.Replace("[", "%5B");
            text = text.Replace("}", "%7D");
            text = text.Replace("{", "%7B");
            text = text.Replace("'", "%27");
            text = text.Replace("`", "%60");
            return UrlBase + text;
        }
        public static string TrackSearch(TrackInfo track) => TrackSearch(track.Track, track.Artist);
        public static string Image(string dbURL, string x1, string x2) => $"https://{dbURL[..^2]}{x1}x{x2}";
    }
}