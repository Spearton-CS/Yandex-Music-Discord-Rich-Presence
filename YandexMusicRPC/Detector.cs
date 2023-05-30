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
                        return;
            }
            catch
            {

            }
            MessageBox.Show("Can't detect installed from Microsoft Store Yandex Music!");
            ToastNotificationManagerCompat.Uninstall();
            Environment.Exit(0);
        }
        private GlobalSystemMediaTransportControlsSessionManager GSMTCSM;
        private GlobalSystemMediaTransportControlsSessionMediaProperties? MediaProperties;
        private Action<TrackInfo?> updateContent;
        private bool Disposed = false;
        public bool Enabled { get; set; } = true;
        public TrackInfo TrackInfo { get; private set; }
        public Thread Thread { get; private set; }
#pragma warning disable CS8618
        public Detector(Action<TrackInfo?> updateContent)
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
                        updateContent(null);
                    }
                Thread.Sleep(1000);
            }
        }
        private async void GetGSMT() => GSMTCSM = await GetSMTCSM();
        private async Task UpdateContent()
        {
            await Task.Run(async () =>
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
                if ($"{MediaProperties.Title}, {MediaProperties.Artist}" == TrackInfo?.ToString())
                    return;
                TrackInfo = new(MediaProperties.Title, MediaProperties.Artist);
                updateContent(TrackInfo);
                GetImage();
                MediaProperties = null;
                GC.Collect();
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
        public string Track { get; }
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
        public string TrackUrl { get; }
        public string Artist { get; }
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
        public long Duration { get; }
        public string ImageUrl { get; }
        public string ArtistUrl { get; }
        public string ArtistImageUrl { get; }
        public string Album { get; }
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
        public string AlbumUrl { get; }
        public string AlbumImageUrl { get; }
        public string Genre { get; }
        public TrackInfo(string track, string artist)
        {
            Track = track;
            Artist = artist;
            Yandex.Music.Client.YandexMusicClient client = new();
            Yandex.Music.Api.Models.Search.Track.YSearchTrackModel yTrack = client.Search($"{track} {artist}", Yandex.Music.Api.Models.Common.YSearchType.Track).Tracks.Results.ToArray()[0];
            Duration = yTrack.DurationMs;
            ImageUrl = URLGenerator.Image(yTrack.CoverUri, "100", "100");
            TrackUrl = URLGenerator.Track(yTrack.Id);
            Yandex.Music.Api.Models.Album.YAlbum yAlbum = yTrack.Albums[0];
            Album = yAlbum.Title;
            AlbumUrl = URLGenerator.Album(yAlbum.Id);
            AlbumImageUrl = URLGenerator.Image(yAlbum.CoverUri, "100", "100");
            Genre = yAlbum.Genre;
            Yandex.Music.Api.Models.Artist.YArtist yArtist = yTrack.Artists[0];
            ArtistUrl = URLGenerator.Artist(yArtist.Id);
            ArtistImageUrl = URLGenerator.Image(((Yandex.Music.Api.Models.Common.Cover.YCoverImage)yArtist.Cover).Uri, "100", "100");
        }
        public override string ToString() => $"{Track}, {Artist}";
    }
}