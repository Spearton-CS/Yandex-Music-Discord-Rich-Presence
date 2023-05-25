using DiscordRPC;
using Button = DiscordRPC.Button;

namespace YandexMusicRPC
{
    public class RPC
    {
        private int SmallImage = 1;
        private int LargeImage = 2;
        private int FirstButton = 0;
        private int SecondButton = 1;
        private string DetailsFormat = Settings.DefaultDetailsFormat;
        public DiscordRpcClient Client { get; } = new("1096773597000912916");
        private Assets CreateImages(TrackInfo info)
        {
            string i1text;
            string i1url;
            string i2text;
            string i2url;
            switch (SmallImage)
            {
                case 1:
                    if (info.Artist.Length > 128)
                        i1text = info.ShortedArtist;
                    else
                        i1text = info.Artist;
                    i1url = info.ArtistImageUrl;
                    break;
                case 2:
                    if (info.Album.Length > 128)
                        i1text = info.ShortedAlbum;
                    else
                        i1text = info.Album;
                    i1url = info.AlbumImageUrl;
                    break;
                default:
                    if (info.Track.Length > 128)
                        i1text = info.ShortedTrack;
                    else
                        i1text = info.Track;
                    i1url = info.ImageUrl;
                    break;
            }
            switch (LargeImage)
            {
                case 1:
                    if (info.Artist.Length > 128)
                        i2text = info.ShortedArtist;
                    else
                        i2text = info.Artist;
                    i2url = info.ArtistImageUrl;
                    break;
                case 2:
                    if (info.Album.Length > 128)
                        i2text = info.ShortedAlbum;
                    else
                        i2text = info.Album;
                    i2url = info.AlbumImageUrl;
                    break;
                default:
                    if (info.Track.Length > 128)
                        i2text = info.ShortedTrack;
                    else
                        i2text = info.Track;
                    i2url = info.ImageUrl;
                    break;
            }
            return new()
            {
                SmallImageKey = i1url,
                SmallImageText = i1text,
                LargeImageKey = i2url,
                LargeImageText = i2text
            };
        }
        private string CreateDetails(TrackInfo info)
        {
            string full = DetailsFormat.Replace("{Track}", info.Track).Replace("{Artist}", info.Artist).Replace("{Album}", info.Album).Replace("{Genre}", info.Genre).Replace("{Duration}", info.Duration.ToString());
            string shorted = DetailsFormat.Replace("{Track}", info.ShortedTrack).Replace("{Artist}", info.ShortedArtist).Replace("{Album}", info.ShortedAlbum).Replace("{Genre}", info.Genre).Replace("{Duration}", info.Duration.ToString());
            string defaultFull = Settings.DefaultDetailsFormat.Replace("{Track}", info.Track).Replace("{Artist}", info.Artist);
            string defaultShorted = Settings.DefaultDetailsFormat.Replace("{Track}", info.ShortedTrack).Replace("{Artist}", info.ShortedArtist);
            if (full.Length > 128)
                if (shorted.Length > 128)
                    if (defaultFull.Length > 128)
                        if (defaultShorted.Length > 128)
                            return "";
                        else
                            return defaultShorted;
                    else
                        return defaultFull;
                else
                    return shorted;
            else
                return full;
        }
        private Button[] CreateButtons(TrackInfo info)
        {
            Button b1 = new();
            switch (FirstButton)
            {
                case 0:
                    if (info.ShortedTrack.Length <= 32)
                        b1.Label = info.ShortedTrack;
                    else
                        b1.Label = "Track";
                    b1.Url = info.TrackUrl;
                    break;
                case 1:
                    if (info.ShortedArtist.Length <= 32)
                        b1.Label = info.ShortedArtist;
                    else
                        b1.Label = "Artist";
                    b1.Url = info.ArtistUrl;
                    break;
                case 2:
                    if (info.ShortedAlbum.Length <= 32)
                        b1.Label = info.ShortedAlbum;
                    else
                        b1.Label = "Album";
                    b1.Url = info.AlbumUrl;
                    break;
                default:
                    b1.Label = "Yandex Music";
                    b1.Url = "https://music.yandex.ru";
                    break;
            }
            Button b2 = new();
            switch (SecondButton)
            {
                case 0:
                    if (info.ShortedTrack.Length <= 32)
                        b2.Label = info.ShortedTrack;
                    else
                        b2.Label = "Track";
                    b2.Url = info.TrackUrl;
                    break;
                case 1:
                    if (info.ShortedArtist.Length <= 32)
                        b2.Label = info.ShortedArtist;
                    else
                        b2.Label = "Artist";
                    b2.Url = info.ArtistUrl;
                    break;
                case 2:
                    if (info.ShortedAlbum.Length <= 32)
                        b2.Label = info.ShortedAlbum;
                    else
                        b2.Label = "Album";
                    b2.Url = info.AlbumUrl;
                    break;
                default:
                    b2.Label = "Yandex Music";
                    b2.Url = "https://music.yandex.ru";
                    break;
            }
            return new Button[] { b1, b2 };
        }
        public void SetRPC(TrackInfo info, int sImage, int lImage, int fButton, int sButton, string detailsFormat)
        {
            SmallImage = sImage;
            LargeImage = lImage;
            FirstButton = fButton;
            SecondButton = sButton;
            DetailsFormat = detailsFormat;
            try
            {
                Client.SetPresence(new()
                {
                    Assets = CreateImages(info),
                    Buttons = CreateButtons(info),
                    Timestamps = new()
                    {
                        End = DateTime.UtcNow.AddMilliseconds(info.Duration)
                    },
                    Details = CreateDetails(info)
                });
            }
            catch (Exception ex)
            {
                Message.Show($"Can't set RPC for '{info.Track}' ['{info.Artist}']. Error message: {ex.Message}\n\rWhere: {ex.StackTrace}", true);
            }
        }
    }
}