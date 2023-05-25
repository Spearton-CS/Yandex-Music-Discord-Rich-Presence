using System;
using System.IO;

namespace UnInstallHelper
{
    internal class Program
    {
        private static void Main()
        {
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "ymRPC_autostart.bat");
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch
            {

            }
        }
    }
}