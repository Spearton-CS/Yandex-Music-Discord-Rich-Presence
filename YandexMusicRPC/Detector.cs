using Windows.Media.Control;

namespace YandexMusicRPC
{
    public delegate void UpdateContent(string? track, string? artist, bool error);
#pragma warning disable CA1416
    public class Detector : IDisposable
    {
        private GlobalSystemMediaTransportControlsSessionManager GSMTCSM;
        private GlobalSystemMediaTransportControlsSessionMediaProperties? MediaProperties;
        private UpdateContent updateContent;
        private bool Disposed = false;
        public bool Enabled { get; set; } = true;
        public TrackInfo TrackInfo { get; private set; } = new();
        public Thread Thread { get; private set; }
#pragma warning disable CS8618
        public Detector(UpdateContent updateContent)
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
        private async void GetGSMT() => GSMTCSM = await GetSystemMediaTransportControlsSessionManager();
        private async Task UpdateContent()
        {
            await Task.Run(async () => {
                using (null)
                {
                    try
                    {
                        MediaProperties = await GetMediaProperties(GSMTCSM.GetCurrentSession());
                    }
                    catch
                    {
                        updateContent(null, null, true);
                        GC.Collect();
                        GSMTCSM = await GetSystemMediaTransportControlsSessionManager();
                        return;
                    }
                    if (GSMTCSM is null)
                        return;
                    var CurrSession = GSMTCSM.GetCurrentSession();
                    TrackInfo trackInfo = new(MediaProperties.Title, MediaProperties.Artist);
                    if (trackInfo.ToString() == TrackInfo.ToString())
                        return;
                    TrackInfo = new(MediaProperties.Title, MediaProperties.Artist);
                    updateContent(MediaProperties.Title, MediaProperties.Artist, false);
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
                    var bytes = default(byte[]);
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
        private async Task<GlobalSystemMediaTransportControlsSessionManager> GetSystemMediaTransportControlsSessionManager() => await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
        private async Task<GlobalSystemMediaTransportControlsSessionMediaProperties> GetMediaProperties(GlobalSystemMediaTransportControlsSession session) => await session.TryGetMediaPropertiesAsync();
        public void Dispose()
        {
            Disposed = true;
            GC.SuppressFinalize(this);
        }
    }
#pragma warning restore CA1416
    public class TrackInfo
    {
        public string Track { get; set; }
        public string Artist { get; set; }
        public byte[] Image { get; set; } = Array.Empty<byte>();
        public TrackInfo(string track, string artist)
        {
            Track = track;
            Artist = artist;
        }
        public TrackInfo()
        {
            Track = "<track>";
            Artist = "<artist>";
        }
        public override string ToString() => $"{Track}, {Artist}";
    }
}