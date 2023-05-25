namespace YandexMusicRPC
{
    public class Settings
    {
        public Settings()
        {
            if (File.Exists("Settings.dat"))
                try
                {
                    using (BinaryReader reader = new(File.OpenRead("Settings.dat")))
                    {
                        FirstButton = reader.ReadInt32();
                        SecondButton = reader.ReadInt32();
                        DetailsFormat = reader.ReadString();
                        LargeImage = reader.ReadInt32();
                        SmallImage = reader.ReadInt32();
                        DarkTheme = reader.ReadBoolean();
                        CaptionColor = Color.FromArgb(reader.ReadInt32());
                    }
                }
                catch
                {
                    string errorText = "An error occurred when loading settings from the Settings.dat file!";
                    try
                    {
                        File.Delete("Settings.dat");
                        errorText += " The default values will be set.";
                        Message.Show(errorText);
                    }
                    catch
                    {
                        errorText += "\nAnd program can't delete invalid file [Settings.dat]. The default values will be set.";
                        Message.Show(errorText);
                    }
                    FirstButton = 0;
                    SecondButton = 1;
                    DetailsFormat = DefaultDetailsFormat;
                    LargeImage = 2;
                    SmallImage = 1;
                    DarkTheme = false;
                }
            else
            {
                FirstButton = 0;
                SecondButton = 1;
                DetailsFormat = DefaultDetailsFormat;
                LargeImage = 2;
                SmallImage = 1;
                DarkTheme = false;
            }
        }
        public bool Save()
        {
            try
            {
                using (BinaryWriter writer = new(File.Create("Settings.dat")))
                {
                    writer.Write(FirstButton);
                    writer.Write(SecondButton);
                    writer.Write(DetailsFormat);
                    writer.Write(LargeImage);
                    writer.Write(SmallImage);
                    writer.Write(DarkTheme);
                    writer.Write(CaptionColor.ToArgb());
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// -1 - Yandex Music;
        /// 0 - Track;
        /// 1 - Artist;
        /// 2 - Album
        /// </summary>
        public int FirstButton { get; set; }
        /// <summary>
        /// -1 - Yandex Music;
        /// 0 - Track;
        /// 1 - Artist;
        /// 2 - Album
        /// </summary>
        public int SecondButton { get; set; }
        /// <summary>
        /// {Track}
        /// {Artist}
        /// {Album}
        /// {Genre}
        /// </summary>
        public string DetailsFormat { get; set; }
        /// <summary>
        /// 0 - Track;
        /// 1 - Artist;
        /// 2 - Album
        /// </summary>
        public int LargeImage { get; set; }
        /// <summary>
        /// 0 - Track;
        /// 1 - Artist;
        /// 2 - Album
        /// </summary>
        public int SmallImage { get; set; }
        /// <summary>
        /// {Track}
        /// {Artist}
        /// {Album}
        /// {Genre}
        /// </summary>
        public const string DefaultDetailsFormat = "'{Track}' - '{Artist}'";
        public bool DarkTheme { get; set; }
        private Color? captionColor;
        public Color CaptionColor
        {
            get
            {
                if (captionColor is not null)
                    return (Color)captionColor;
                else if (DarkTheme)
                    return Color.FromArgb(24, 24, 24);
                else
                    return Color.FromKnownColor(KnownColor.Control);
            }
            set => captionColor = value;
        }
    }
}