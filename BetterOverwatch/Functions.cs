using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;


namespace BetterOverwatch
{
    class Functions
    {
        [DllImport("winmm.dll")]
        internal static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        [DllImport("user32.dll")]
        internal static extern uint SendMessage(IntPtr hWnd, uint msg, uint wParam, uint lParam);
        [DllImport("user32.dll")]
        internal static extern short GetAsyncKeyState(int vKey);
        internal static string ActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, buff, nChars) > 0)
            {
                return buff.ToString();
            }
            return string.Empty;
        }
        internal static double CompareTwoBitmaps(Bitmap bitmap, Bitmap bitmap2)
        {
            int correctPixels = 0;
            if (bitmap.Width + bitmap.Height != bitmap2.Width + bitmap2.Height) return 0.00;

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            BitmapData data2 = bitmap2.LockBits(new Rectangle(0, 0, bitmap2.Width, bitmap2.Height), ImageLockMode.ReadOnly, bitmap2.PixelFormat);

            unsafe
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    byte* row = (byte*)data.Scan0 + (y * data.Stride);
                    byte* row2 = (byte*)data2.Scan0 + (y * data2.Stride);

                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        byte b = row[x * 4];
                        byte g = row[(x * 4) + 1];
                        byte r = row[(x * 4) + 2];
                        byte b2 = row2[x * 4];
                        byte g2 = row2[(x * 4) + 1];
                        byte r2 = row2[(x * 4) + 2];

                        if (b == b2 && g == g2 && r == r2) correctPixels++;
                    }
                }
            }

            bitmap.UnlockBits(data);
            bitmap2.UnlockBits(data);

            return correctPixels / (double)(bitmap.Width * bitmap.Height);

        }
        internal static double CompareStrings(string string1, string string2)
        {
            try
            {
                string1 = string1.ToLower();
                string2 = string2.ToLower();
                int[,] d = new int[string1.Length + 1, string2.Length + 1];

                if (string1.Length == 0) return string2.Length;
                if (string2.Length == 0) return string1.Length;
                for (int i = 0; i <= string1.Length; d[i, 0] = i++) { }
                for (int j = 0; j <= string2.Length; d[0, j] = j++) { }

                for (int i = 1; i <= string1.Length; i++)
                {
                    for (int j = 1; j <= string2.Length; j++)
                    {
                        int cost = (string2[j - 1] == string1[i - 1]) ? 0 : 1;

                        d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                    }
                }
                int big = Math.Max(string1.Length, string2.Length);

                return Math.Floor(Convert.ToDouble(big - d[string1.Length, string2.Length]) / Convert.ToDouble(big) * 100);
            }
            catch (Exception e) { Console.WriteLine($"CompareStrings error: {e}"); }
            return 0.00;
        }
        internal static bool IsMapBlacklisted(string map)
        {
            bool blacklisted = false;

            foreach (string blacklistedMap in Constants.MAP_LIST_BLACKLIST)
            {
                if (map.Equals(blacklistedMap))
                {
                    blacklisted = true;
                    break;
                }
            }

            return blacklisted;
        }
        internal static string CheckMaps(string map)
        {
            for (int i = 0; i < Constants.MAP_LIST.Length; i++)
            {
                string mapName = Constants.MAP_LIST[i].Replace(" ", string.Empty).ToLower();

                if (map.ToLower().Equals(mapName) || (CompareStrings(map, mapName) >= 60 && !IsMapBlacklisted(map)))
                {
                    return Constants.MAP_LIST[i];
                }
            }
            return string.Empty;
        }
        internal static bool IsProcessOpen(string name)
        {
            if (Process.GetProcessesByName(name).Length > 0) return true;
            return false;
        }
        internal static void SetVolume(int vol)
        {
            int newVolume = ushort.MaxValue / 100 * vol;
            uint newVolumeAllChannels = ((uint)newVolume & 0x0000ffff) | ((uint)newVolume << 16);
            waveOutSetVolume(IntPtr.Zero, newVolumeAllChannels);
        }
        internal static void DebugMessage(string msg)
        {
            try
            {
                if (Directory.Exists(AppData.configPath))
                {
                    string date = DateTime.Now.ToString("dd/MM/yy HH:mm:ss");
                    File.AppendAllText(Path.Combine(AppData.configPath, "debug.log"), $"[{date}] {msg + "\r\n"}");
                }
            }
            catch { }
            Debug.WriteLine(msg);
        }
        /*
 * UNUSED METHODS
         private static StringBuilder GetHashFromImage(Bitmap bitmap) // DEBUG
        {
            byte[] bytes;
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png); // gif for example
                bytes = ms.ToArray();
            }
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hash = md5.ComputeHash(bytes);

            // make a hex string of the hash for display or whatever
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hash)
            {
                sb.Append(b.ToString("x2").ToLower());
            }

            return sb;
        }
private static Bitmap Downscale(Image original)
{
    double widthPercent = (double)original.Width / 1920 * 1366;
    double heightPercent = (double)original.Height / 1080 * 768;
    int width = (int)widthPercent;
    int height = (int)heightPercent;
    Bitmap downScaled = new Bitmap(width, height);

    using (Graphics graphics = Graphics.FromImage(downScaled))
    {
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.DrawImage(original, 0, 0, downScaled.Width, downScaled.Height);
    }

    return downScaled;
}

private static Bitmap Upscale(Image original)
{
    double widthPercent = (double)original.Width / 1366 * 1920;
    double heightPercent = (double)original.Height / 768 * 1080;
    int width = (int)widthPercent + 1;
    int height = (int)heightPercent + 1;
    Bitmap upScaled = new Bitmap(width, height);

    using (Graphics graphics = Graphics.FromImage(upScaled))
    {
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.DrawImage(original, 0, 0, upScaled.Width, upScaled.Height);
    }

    return upScaled;
}
*/
    }
}