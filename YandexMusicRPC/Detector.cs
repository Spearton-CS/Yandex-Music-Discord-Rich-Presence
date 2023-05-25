using System.Data.SQLite;
using Windows.Media.Control;

namespace YandexMusicRPC
{
    public class Detector : IDisposable
    {
        public static void DetectInstalledYM()
        {
            try
            {
                foreach (string dir in Directory.GetDirectories(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages")))
                    if (dir.Contains("Yandex.Music"))
                        foreach (string file in Directory.GetFiles(Path.Combine(dir, "LocalState")))
                            if (Path.GetExtension(file)?.ToLower() == ".sqlite")
                            {
                                Program.DBPath = Path.Combine(dir, "LocalState", Path.GetFileName(file));
                                return;
                            }
                MessageBox.Show("Can't detect installed from Microsoft Store Yandex Music!");
                ToastNotificationManagerCompat.Uninstall();
                Environment.Exit(0);
            }
            catch
            {
                MessageBox.Show("Can't detect installed from Microsoft Store Yandex Music!");
                ToastNotificationManagerCompat.Uninstall();
                Environment.Exit(0);
            }
        }
        private GlobalSystemMediaTransportControlsSessionManager GSMTCSM;
        private GlobalSystemMediaTransportControlsSessionMediaProperties? MediaProperties;
        private Action<TrackInfo> updateContent;
        private bool Disposed = false;
        public bool Enabled { get; set; } = true;
        public TrackInfo TrackInfo { get; private set; } = new();
        public Thread Thread { get; private set; }
#pragma warning disable CS8618
        public Detector(Action<TrackInfo> updateContent)
#pragma warning restore CS8618
        {
            this.updateContent = updateContent;
            Thread = new(StartMusic);
            Thread.Start();
        }
        private async void StartMusic()
        {
            GetGSMT();
            while (!Disposed)
            {
                if (Enabled)
                    try
                    {
                        await UpdateContent();
                    }
                    catch
                    {

                    }
                Thread.Sleep(1000);
            }
        }
        private async void GetGSMT() => GSMTCSM = await GetSMTCSM();
        private async Task UpdateContent()
        {
            await Task.Run(async () => {
                using (null)
                {
                    try
                    {
                        var props = GetMediaProperties(GSMTCSM.GetCurrentSession())?.Result;
                        if (props is null)
                            return;
                        MediaProperties = props;
                    }
                    catch
                    {
                        GC.Collect();
                        GSMTCSM = await GetSMTCSM();
                        return;
                    }
                    if (GSMTCSM is null)
                        return;
                    var CurrSession = GSMTCSM.GetCurrentSession();
                    if ($"{MediaProperties.Title}, {MediaProperties.Artist}" == TrackInfo.ToString())
                        return;
                    TrackInfo = new(MediaProperties.Title, MediaProperties.Artist);
                    updateContent(TrackInfo);
                    GetImage();
                    MediaProperties = null;
                    GC.Collect();
                }
            });
        }
        private async void GetImage()
        {
            try
            {
                var ssss = await MediaProperties?.Thumbnail.OpenReadAsync();
                using (StreamReader sr = new(ssss.AsStreamForRead()))
                {
                    byte[]? bytes = default;
                    using (MemoryStream memstream = new())
                    {
                        byte[] buffer = new byte[512];
                        int bytesRead = default;
                        while ((bytesRead = sr.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                            memstream.Write(buffer, 0, bytesRead);
                        bytes = memstream.ToArray();
                        if (TrackInfo.Image == bytes)
                            return;
                        TrackInfo.Image = bytes;
                        bytes = null;
                    }
                }
            }
            catch
            {

            }
            GC.Collect();
        }
        private async Task<GlobalSystemMediaTransportControlsSessionManager> GetSMTCSM() => await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
        private async Task<GlobalSystemMediaTransportControlsSessionMediaProperties>? GetMediaProperties(GlobalSystemMediaTransportControlsSession session) => await session.TryGetMediaPropertiesAsync();
        public void Dispose()
        {
            Disposed = true;
            GC.SuppressFinalize(this);
        }
    }
    public class TrackInfo
    {
        public string Track { get; set; }
        public string ShortedTrack
        {
            get
            {
                if (Track.Length > 32)
                    return $"{Track.Substring(0, 30)}..";
                else
                    return Track;
            }
        }
        public string TrackUrl => URLGenerator.Track(TrackID.ToString());
        public string Artist { get; set; }
        public string ShortedArtist
        {
            get
            {
                if (Artist.Length > 32)
                    return $"{Artist.Substring(0, 30)}..";
                else
                    return Artist;
            }
        }
        public byte[] Image { get; set; } = Array.Empty<byte>();
        public long Duration { get; private set; }
        public string ImageUrl { get; private set; }
        public ulong TrackID { get; private set; }
        public ulong ArtistID { get; private set; }
        public string ArtistUrl => URLGenerator.Artist(ArtistID.ToString());
        public string ArtistImageUrl { get; private set; }
        public ulong AlbumID { get; private set; }
        public string Album { get; private set; }
        public string ShortedAlbum
        {
            get
            {
                if (Album.Length > 32)
                    return $"{Album.Substring(0, 30)}..";
                else
                    return Album;
            }
        }
        public string AlbumUrl => URLGenerator.Album(AlbumID.ToString());
        public string AlbumImageUrl { get; private set; }
        public string Genre { get; private set; }
        private static void A(string a) => File.AppendAllText(".txt", $"{a}\n\r");
        private void GetInfoFromDB()
        {
            try
            {
                SQLiteConnection yandexDBConnection = new($"Data Source={Program.DBPath}");
                yandexDBConnection.Open();
                SQLiteCommand cmd = new($"SELECT * FROM T_Artist WHERE Name=\"{Artist}\"", yandexDBConnection);
                SQLiteCommand subCmd = new(yandexDBConnection);
                cmd.Prepare();
                subCmd.Prepare();
                SQLiteDataReader artistInfo = cmd.ExecuteReader();
                if (artistInfo.Read())
                {
                    ArtistID = ulong.Parse((string)artistInfo["Id"]);
                    ArtistImageUrl = URLGenerator.Image((string)artistInfo["CoverUri"], "200", "200");
                }
                _ = artistInfo.DisposeAsync();
                cmd.Reset();
                cmd.CommandText = $"SELECT * FROM T_TrackArtist WHERE ArtistId=\"{ArtistID}\"";
                SQLiteDataReader trackArtistInfo = cmd.ExecuteReader();
                bool exit = false;
                SQLiteDataReader? tempReader = null;
                while (trackArtistInfo.Read())
                {
                    if (exit)
                        break;
                    ulong id = ulong.Parse((string)trackArtistInfo["TrackId"]);
                    subCmd.CommandText = $"SELECT * FROM T_Track WHERE Id=\"{id}\"";
                    tempReader = subCmd.ExecuteReader();
                    if (tempReader.Read())
                        if ((string)tempReader["Title"] == Track)
                        {
                            exit = true;
                            TrackID = id;
                        }
                    _ = tempReader.DisposeAsync();
                }
                if (!exit)
                {
                    Message.Show($"Can't find '{Track}' ['{Artist}'] in Yandex DB", true);
                    return;
                }
                else
                {
                    _ = trackArtistInfo.DisposeAsync();
                    subCmd.Reset();
                }    
                cmd.Reset();
                cmd.CommandText = $"SELECT * FROM T_Track WHERE Id=\"{TrackID}\"";
                SQLiteDataReader info = cmd.ExecuteReader();
                if (info.Read())
                {
                    ImageUrl = URLGenerator.Image((string)info["CoverUri"], "200", "200");
                    Duration = (long)info["DurationMillis"];
                }
                _ = info.DisposeAsync();
                cmd.Reset();
                cmd.CommandText = $"SELECT * FROM T_TrackAlbum WHERE TrackId=\"{TrackID}\"";
                SQLiteDataReader albumInfo = cmd.ExecuteReader();
                if (albumInfo.Read())
                    AlbumID = ulong.Parse((string)albumInfo["AlbumId"]);
                _ = albumInfo.DisposeAsync();
                cmd.Reset();
                cmd.CommandText = $"SELECT * FROM T_Album WHERE Id=\"{AlbumID}\"";
                SQLiteDataReader album = cmd.ExecuteReader();
                if (album.Read())
                {
                    Album = (string)album["Title"];
                    Genre = (string)album["GenreId"];
                    AlbumImageUrl = URLGenerator.Image((string)album["CoverUri"], "200", "200");
                }
                _ = album.DisposeAsync();
                cmd.Dispose();
                subCmd.Dispose();
                yandexDBConnection.Close();
                yandexDBConnection.Dispose();
            }
            catch (Exception ex)
            {
                Message.Log($"Yandex DB Reader '{ex.GetType().Name}': {ex.Message}");
            }
        }
#pragma warning disable CS8618
        public TrackInfo(string track, string artist)
        {
            Track = track;
            Artist = artist;
            GetInfoFromDB();
        }
        public TrackInfo()
        {
            Track = "<unknown track>";
            Artist = "<unknown artist>";
            Album = "<unknown album>";
            Genre = "<unknown genre>";
        }
#pragma warning restore CS8618
        public override string ToString() => $"{Track}, {Artist}";
    }
}