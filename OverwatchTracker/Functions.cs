using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using AForge.Imaging.Filters;

namespace BetterOverwatch
{
    class Functions
    {
        [DllImport("winmm.dll")]
        public static extern int waveOutGetVolume(IntPtr hwo, out uint dwVolume);
        [DllImport("winmm.dll")]
        public static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        [DllImport("user32.dll")]
        public static extern uint SendMessage(IntPtr hWnd, uint msg, uint wParam, uint lParam);
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);
        public static string ActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return String.Empty;
        }
        public static Bitmap CaptureRegion(Bitmap frame, int x, int y, int width, int height)
        {
            return frame.Clone(new Rectangle(x, y, width, height), PixelFormat.Format32bppArgb);
        }
        public static Bitmap AdjustColors(Bitmap b, short radius, byte red = 255, byte green = 255, byte blue = 255, bool fillOutside = true)
        {
            EuclideanColorFiltering filter = new EuclideanColorFiltering();
            filter.CenterColor = new AForge.Imaging.RGB(red, green, blue);
            filter.Radius = radius;
            filter.FillOutside = fillOutside;
            if (!fillOutside)
            {
                filter.FillColor = new AForge.Imaging.RGB(255, 255, 255);
            }
            filter.ApplyInPlace(b);

            return b;
        }
        public static void AdjustContrast(Bitmap image, float Value, bool invertColors = false)
        {
            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, image.PixelFormat);
            int Width = image.Width;
            int Height = image.Height;

            unsafe
            {
                for (int y = 0; y < Height; ++y)
                {
                    byte* row = (byte*)data.Scan0 + (y * data.Stride);
                    int columnOffset = 0;

                    for (int x = 0; x < Width; ++x)
                    {
                        byte B = row[columnOffset];
                        byte G = row[columnOffset + 1];
                        byte R = row[columnOffset + 2];

                        float Red = R / 255.0f;
                        float Green = G / 255.0f;
                        float Blue = B / 255.0f;
                        Red = (((Red - 0.5f) * Value) + 0.5f) * 255.0f;
                        Green = (((Green - 0.5f) * Value) + 0.5f) * 255.0f;
                        Blue = (((Blue - 0.5f) * Value) + 0.5f) * 255.0f;

                        int iR = (int)Red;
                        iR = iR > 255 ? 255 : iR;
                        iR = iR < 0 ? 0 : iR;
                        int iG = (int)Green;
                        iG = iG > 255 ? 255 : iG;
                        iG = iG < 0 ? 0 : iG;
                        int iB = (int)Blue;
                        iB = iB > 255 ? 255 : iB;
                        iB = iB < 0 ? 0 : iB;

                        if (invertColors)
                        {
                            if (iB == 255)
                            {
                                iB = 0;
                                iG = 0;
                                iR = 0;
                            }
                            else
                            {
                                iB = 255;
                                iG = 255;
                                iR = 255;
                            }
                        }

                        row[columnOffset] = (byte)iB;
                        row[columnOffset + 1] = (byte)iG;
                        row[columnOffset + 2] = (byte)iR;

                        columnOffset += 4;
                    }
                }
            }

            image.UnlockBits(data);
        }
        public static double CompareStrings(string s, string t)
        {
            s = s.ToLower();
            t = t.ToLower();
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0) return m;
            if (m == 0) return n;
            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }
            int big = Math.Max(s.Length, t.Length);

            return Math.Floor(Convert.ToDouble(big - d[n, m]) / Convert.ToDouble(big) * 100);
        }
        public static string CheckMaps(string input)
        {
            for (int i = 0; i < Vars.maps.Length; i++)
            {
                string map = Vars.maps[i].Replace(" ", String.Empty).ToLower();

                if (input.ToLower().Contains(map))
                {
                    return Vars.maps[i];
                }
                else
                {
                    double percent = CompareStrings(input, map);

                    if (percent >= 60)
                    {
                        return Vars.maps[i];
                    }
                }
            }
            return String.Empty;
        }
        public static bool CheckStats(string elimsText, string damageText, string objKillsText, string healingText, string deathsText, double time)
        {
            int seconds = Convert.ToInt32(Math.Floor(time / 1000));

            if (int.TryParse(elimsText, out int elims) && (elims / seconds) * 60 < 7 &&
                int.TryParse(damageText, out int damage) && (damage / seconds) * 60 < 2000 &&
                int.TryParse(objKillsText, out int objKills) && (objKills / seconds) * 60 < 7 &&
                int.TryParse(healingText, out int healing) && (healing / seconds) * 60 < 2000 &&
                int.TryParse(deathsText, out int deaths) && (deaths / seconds) * 60 < 7)
            {
                return true;
            }
            return false;
        }
        public static string BitmapToText(Bitmap frame, int x, int y, int width, int height, bool contrastFirst = false, short radius = 110, Network network = 0, bool invertColors = false, byte red = 255, byte green = 255, byte blue = 255, bool fillOutside = true)
        {
            string output = String.Empty;
            try
            {
                Bitmap result = new Bitmap(frame.Clone(new Rectangle(x, y, width, height), PixelFormat.Format32bppArgb));

                if (contrastFirst)
                {
                    AdjustContrast(result, 255f);
                    result = AdjustColors(result, radius, red, green, blue, fillOutside);
                }
                else
                {
                    result = AdjustColors(result, radius, red, green, blue, fillOutside);
                    AdjustContrast(result, 255f, invertColors);
                }
                output = FetchTextFromImage(result, network);
                result.Dispose();
            }
            catch { }
            return output;
        }
        private static Bitmap Downscale(Bitmap original)
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

        private static Bitmap Upscale(Bitmap original)
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
        public static int[,] LabelImage(Bitmap image, out int labelCount)
        {
            BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);
            int nrow = image.Height;
            int ncol = image.Width;
            int[,] img = new int[nrow, ncol];
            int[,] label = new int[nrow, ncol];
            int imageBytes = Image.GetPixelFormatSize(bitmapData.PixelFormat) / 8;

            unsafe
            {
                byte* ptr = (byte*)bitmapData.Scan0;

                for (int i = 0; i < image.Height; i++)
                {
                    for (int j = 0; j < image.Width; j++)
                    {
                        img[i, j] = (ptr[0] + ptr[1] + ptr[2]) / 3;
                        label[i, j] = 0;
                        ptr += imageBytes;
                    }
                }
            }
            image.UnlockBits(bitmapData);

            int lab = 1;
            int[] pos;
            Stack<int[]> stack = new Stack<int[]>();

            try
            {
                for (int c = 0; c != ncol; c++)
                {
                    for (int r = 0; r != nrow; r++)
                    {
                        if (img[r, c] == 0 || label[r, c] != 0)
                        {
                            continue;
                        }

                        stack.Push(new int[] { r, c });
                        label[r, c] = lab;

                        while (stack.Count != 0)
                        {
                            pos = stack.Pop();
                            int y = pos[0];
                            int x = pos[1];

                            if (y > 0 && x > 0)
                            {
                                if (img[y - 1, x - 1] > 0 && label[y - 1, x - 1] == 0)
                                {
                                    stack.Push(new int[] { y - 1, x - 1 });
                                    label[y - 1, x - 1] = lab;
                                }
                            }

                            if (y > 0)
                            {
                                if (img[y - 1, x] > 0 && label[y - 1, x] == 0)
                                {
                                    stack.Push(new int[] { y - 1, x });
                                    label[y - 1, x] = lab;
                                }
                            }

                            if (y > 0 && x < ncol - 1)
                            {
                                if (img[y - 1, x + 1] > 0 && label[y - 1, x + 1] == 0)
                                {
                                    stack.Push(new int[] { y - 1, x + 1 });
                                    label[y - 1, x + 1] = lab;
                                }
                            }

                            if (x > 0)
                            {
                                if (img[y, x - 1] > 0 && label[y, x - 1] == 0)
                                {
                                    stack.Push(new int[] { y, x - 1 });
                                    label[y, x - 1] = lab;
                                }
                            }

                            if (x < ncol - 1)
                            {
                                if (img[y, x + 1] > 0 && label[y, x + 1] == 0)
                                {
                                    stack.Push(new int[] { y, x + 1 });
                                    label[y, x + 1] = lab;
                                }
                            }

                            if (y < nrow - 1 && x > 0)
                            {
                                if (img[y + 1, x - 1] > 0 && label[y + 1, x - 1] == 0)
                                {
                                    stack.Push(new int[] { y + 1, x - 1 });
                                    label[y + 1, x - 1] = lab;
                                }
                            }

                            if (y < nrow - 1)
                            {
                                if (img[y + 1, x] > 0 && label[y + 1, x] == 0)
                                {
                                    stack.Push(new int[] { y + 1, x });
                                    label[y + 1, x] = lab;
                                }
                            }

                            if (y < nrow - 1 && x < ncol - 1)
                            {
                                if (x + 1 == 21 && y + 1 == 15)
                                {
                                }
                                if (img[y + 1, x + 1] > 0 && label[y + 1, x + 1] == 0)
                                {
                                    stack.Push(new int[] { y + 1, x + 1 });
                                    label[y + 1, x + 1] = lab;
                                }
                            }
                        }
                        lab++;
                    }
                }
            }
            catch { }
            labelCount = lab;

            return label;
        }
        public static List<Bitmap> GetConnectedComponentLabels(Bitmap image)
        {
            int[,] labels = LabelImage(image, out int labelCount);
            List<Bitmap> bitmaps = new List<Bitmap>();

            if (labelCount > 0)
            {
                Rectangle[] rects = new Rectangle[labelCount];

                for (int x = 0; x < image.Width; x++)
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        for (int i = 1; i < labelCount; i++)
                        {
                            if (labels[y, x] == i)
                            {
                                if (x < rects[i].X || rects[i].X == 0)
                                {
                                    rects[i].X = x;
                                }
                                if (y < rects[i].Y || rects[i].Y == 0)
                                {
                                    rects[i].Y = y;
                                }
                                if (x > rects[i].Width)
                                {
                                    rects[i].Width = x;
                                }
                                if (y > rects[i].Height)
                                {
                                    rects[i].Height = y;
                                }
                            }
                        }
                    }
                }

                for (int i = 1; i < labelCount; i++)
                {
                    int Width = (rects[i].Width - rects[i].X) + 1;
                    int Height = (rects[i].Height - rects[i].Y) + 1;

                    if ((double)Height / (double)image.Height > 0.6)
                    {
                        bitmaps.Add(new Bitmap(Width, Height, image.PixelFormat));
                        BitmapData bitmapData = bitmaps[bitmaps.Count - 1].LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, image.PixelFormat);
                        int imageBytes = Image.GetPixelFormatSize(bitmapData.PixelFormat) / 8;

                        unsafe
                        {
                            byte* rgb = (byte*)bitmapData.Scan0;

                            for (int x = 0; x < Width; x++)
                            {
                                for (int y = 0; y < Height; y++)
                                {
                                    int pos = (y * bitmapData.Stride) + (x * imageBytes);

                                    if (labels[(y + rects[i].Y), (x + rects[i].X)] == i)
                                    {
                                        rgb[pos] = 255;
                                        rgb[pos + 1] = 255;
                                        rgb[pos + 2] = 255;
                                    }
                                }
                            }
                            bitmaps[bitmaps.Count - 1].UnlockBits(bitmapData);
                        }
                    }
                }
            }

            return bitmaps;
        }
        public static bool IsProcessOpen(string name)
        {
            foreach (Process clsProcess in Process.GetProcesses())
            {
                if (clsProcess.ProcessName.Equals(name))
                {
                    return true;
                }
            }
            return false;
        }
        public static byte[] ImageToBytes(Image img)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }
        public static Bitmap ReduceImageSize(Bitmap imgPhoto, int Percent)
        {
            float nPercent = ((float)Percent / 100);

            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;

            int destX = 0;
            int destY = 0;
            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(destWidth, destHeight, PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.InterpolationMode = InterpolationMode.High;
            grPhoto.DrawImage(imgPhoto, new Rectangle(destX, destY, destWidth, destHeight), new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight), GraphicsUnit.Pixel);
            grPhoto.Dispose();

            return bmPhoto;
        }
        public static string FetchBattleTag()
        {
            if (Vars.isAdmin)
            {
                Process[] processes = Process.GetProcessesByName("Battle.net");
                foreach (Process process in processes)
                {
                    if(Vars.blizzardAppOffset == 0)
                    {
                        if (!Server.FetchBlizzardAppOffset(process.MainModule.FileVersionInfo.FileVersion)) break;
                    }
                    IntPtr processBaseAddress = process.MainModule.BaseAddress;

                    foreach (ProcessModule processModule in process.Modules)
                    {
                        if (processModule.ModuleName == "battle.net.dll")
                        {
                            processBaseAddress = processModule.BaseAddress;
                            byte[] battleTagBytes = Memory.ReadBytes(process.Handle, processBaseAddress + Vars.blizzardAppOffset, new int[] { 0x28, 0x10, 0x8, 0x84, 0x0 });

                            if (battleTagBytes.Length > 0)
                            {
                                string[] battleTagSplit = Encoding.UTF8.GetString(battleTagBytes).Split('#');

                                if (battleTagSplit.Length == 2)
                                {
                                    DebugMessage("Found BattleTag");
                                    return $"{battleTagSplit[0]}-{battleTagSplit[1].Substring(0, battleTagSplit[1].Length > 4 ? 5 : 4)}";
                                }
                            }
                        }
                    }
                }
                DebugMessage("Failed to find BattleTag");
            }
            return "PLAYER-0000";
        }
        public static string FetchLetterFromImage(BackPropNetwork network, Bitmap image, Network networkId)
        {
            double[] input = BackPropNetwork.CharToDoubleArray(image);

            for (int i = 0; i < network.InputNodesCount; i++)
            {
                network.InputNode(i).Value = input[i];
            }
            network.Run();

            if (networkId == Network.Maps || networkId == Network.HeroNames)
            {
                return Convert.ToChar('A' + network.BestNodeIndex).ToString();
            }
            else if (networkId == Network.TeamSkillRating || networkId == Network.Numbers)
            {
                return network.BestNodeIndex.ToString();
            }
            return String.Empty;
        }
        public static string FetchTextFromImage(Bitmap image, Network network)
        {
            string text = String.Empty;
            try
            {
                List<Bitmap> bitmaps = GetConnectedComponentLabels(image);

                for (int i = 0; i < bitmaps.Count; i++)
                {
                    if (network == Network.Maps)
                    {
                        text += FetchLetterFromImage(BetterOverwatchNetworks.mapsNN, bitmaps[i], network);
                    }
                    else if (network == Network.TeamSkillRating)
                    {
                        text += FetchLetterFromImage(BetterOverwatchNetworks.teamSkillRatingNN, bitmaps[i], network);
                    }
                    else if (network == Network.Numbers)
                    {
                        text += FetchLetterFromImage(BetterOverwatchNetworks.numbersNN, bitmaps[i], network);
                    }
                    else if (network == Network.HeroNames)
                    {
                        text += FetchLetterFromImage(BetterOverwatchNetworks.heroNamesNN, bitmaps[i], network);
                    }
                    bitmaps[i].Dispose();
                }
            }
            catch (Exception e)
            {
                Functions.DebugMessage("getTextFromImage() error: " + e.ToString());
            }
            return text;
        }
        public static int GetTimeDeduction(bool getNextDeduction = false)
        {
            int offset = 10; // amount of extra seconds to offset the slowdown time and showing "Round complete" etc..
            int accumulatedResult = 0;

            if (!getNextDeduction || Vars.roundsCompleted == 0)
            {
                if (Vars.gameData.mapInfo.isKoth)
                    accumulatedResult += 70;
                else
                    accumulatedResult += 100;
            }

            for (int i = 1; i < Vars.roundsCompleted + 1; i++)
            {
                if (getNextDeduction)
                    i = Vars.roundsCompleted;

                if (Vars.gameData.mapInfo.isKoth)
                    accumulatedResult += 40 + offset;
                else if (i == 1)
                    accumulatedResult += 100;
                else
                    accumulatedResult += 70 + offset;

                if (getNextDeduction)
                    break;
            }
            return accumulatedResult * 1000;
        }
        public static void PlaySound()
        {
            Vars.audio.Play();
        }
        public static void SetVolume(int vol)
        {
            int NewVolume = ((ushort.MaxValue / 100) * vol);
            uint NewVolumeAllChannels = (((uint)NewVolume & 0x0000ffff) | ((uint)NewVolume << 16));
            waveOutSetVolume(IntPtr.Zero, NewVolumeAllChannels);
        }
        public static string SecondsToMinutes(double secs)
        {
            double mins = Math.Floor(secs / 60);
            secs -= (mins * 60);
            string minutes, seconds;
            if (mins < 10) { minutes = "0" + mins; }
            else { minutes = mins.ToString(); }

            if (secs < 10) { seconds = "0" + secs; }
            else { seconds = secs.ToString(); }

            return String.Format("{0}:{1}", minutes, seconds);
        }
        public static void DebugMessage(string msg)
        {
            try
            {
                if (Directory.Exists(Vars.configPath))
                {
                    string date = DateTime.Now.ToString("dd/MM/yy HH:mm:ss");
                    File.AppendAllText(Path.Combine(Vars.configPath, "debug.log"), String.Format("[{0}] {1}", date, msg + "\r\n"));
                }
            }
            catch { }
            Debug.WriteLine(msg);
        }
    }
}