using System.Data.SQLite;

namespace YandexMusicRPC
{
    public static class RPC
    {
        public static DiscordRPC.DiscordRpcClient client { get; } = new("1096773597000912916");
        public static bool Set(TrackInfo info)
        {
            try
            {
                string TrackImageUrl = "";
                string? dbPath = null;
                long TrackDuration = 0;
                foreach (string dir in Directory.GetDirectories(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages")))
                    if (dir.Contains("Yandex.Music"))
                        foreach (string file in Directory.GetFiles(Path.Combine(dir, "LocalState")))
                            if (Path.GetExtension(file)?.ToLower() == ".sqlite")
                            {
                                dbPath = Path.Combine(dir, "LocalState", Path.GetFileName(file));
                                break;
                            }
                if (dbPath is null)
                    return false;
                SQLiteConnection yandexDBConnection = new($"Data Source={dbPath}");
                yandexDBConnection.Open();
                SQLiteCommand cmd = new($"SELECT * FROM T_Track WHERE Title=\"{info.Track}\"", yandexDBConnection);
                SQLiteDataReader track = cmd.ExecuteReader();
                if (track.Read())
                {
                    TrackImageUrl = "https://" + ((string)track["CoverUri"])[..^2] + "200x200";
                    TrackDuration = (long)track["DurationMillis"];
                }
                yandexDBConnection.Close();
                yandexDBConnection.Dispose();
                client.SetPresence(new()
                {
                    Assets = new()
                    {
                        LargeImageKey = TrackImageUrl,
                        LargeImageText = info.ToString(),
                        SmallImageKey = "https://yastatic.net/s3/doc-binary/freeze/m0Q5UlZ5G_e_LHMDh2HJXl15mvM.png",
                        SmallImageText = "Yandex Music"
                    },
                    Buttons = new DiscordRPC.Button[]
                    {
                        new()
                        {
                            Label = "Yandex Music",
                            Url = "https://music.yandex.ru"
                        }
                    },
                    Timestamps = new DiscordRPC.Timestamps()
                    {
                        End = DateTime.UtcNow.AddMilliseconds((ulong)TrackDuration)
                    },
                    State = "Listening",
                    Details = info.ToString()
                });
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}