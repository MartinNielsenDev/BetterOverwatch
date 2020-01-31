using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Image = System.Drawing.Image;

namespace BetterOverwatch
{
    public class BitmapFunctions
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static unsafe extern int memcpy(byte* dest, byte* src, long count);
        internal static string ProcessFrame(Bitmap frame, Rectangle rect, bool contrastFirst = false, short radius = 110, NetworkEnum network = NetworkEnum.Maps, bool invertColors = false, byte red = 255, byte green = 255, byte blue = 255, bool fillOutside = true, bool limeToWhite = false)
        {
            string output = string.Empty;
            try
            {
                using (Bitmap cropped = CropImage(frame, rect))
                {
                    using (Bitmap cloned = new Bitmap(rect.Width, rect.Height, PixelFormat.Format24bppRgb))
                    {
                        using (Graphics g = Graphics.FromImage(cloned))
                        {
                            g.DrawImage(cropped, new Rectangle(0, 0, rect.Width, rect.Height));
                        }
                        if (contrastFirst)
                        {
                            AdjustContrast(cloned, 255f, invertColors, limeToWhite);
                            AdjustColors(cloned, radius, red, green, blue, fillOutside);
                        }
                        else
                        {
                            AdjustColors(cloned, radius, red, green, blue, fillOutside);
                            AdjustContrast(cloned, 255f, invertColors, limeToWhite);
                        }

                        output = FetchTextFromBitmap(cloned, network);
                    }
                }
            }
            catch (Exception e) { Console.WriteLine($"BitmapToText error: {e}"); }
            return output;
        }
        internal static void AdjustColors(Bitmap b, short radius, byte red = 255, byte green = 255, byte blue = 255, bool fillOutside = true)
        {
            EuclideanColorFiltering filter = new EuclideanColorFiltering
            {
                CenterColor = new RGB(red, green, blue),
                Radius = radius,
                FillOutside = fillOutside
            };
            if (!fillOutside)
            {
                filter.FillColor = new RGB(255, 255, 255);
            }
            filter.ApplyInPlace(b);
        }
        internal static void AdjustContrast(Bitmap bitmap, float value, bool invertColors = false, bool limeToWhite = false)
        {
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            int imageBytes = Image.GetPixelFormatSize(bitmapData.PixelFormat) / 8;

            unsafe
            {
                byte* rgb = (byte*)bitmapData.Scan0;

                for (int x = 0; x < bitmap.Width; x++)
                {
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        int pos = (y * bitmapData.Stride) + (x * imageBytes);
                        byte b = rgb[pos];
                        byte g = rgb[pos + 1];
                        byte r = rgb[pos + 2];

                        float red = r / 255.0f;
                        float green = g / 255.0f;
                        float blue = b / 255.0f;
                        red = (((red - 0.5f) * value) + 0.5f) * 255.0f;
                        green = (((green - 0.5f) * value) + 0.5f) * 255.0f;
                        blue = (((blue - 0.5f) * value) + 0.5f) * 255.0f;

                        int iR = (int)red;
                        iR = iR > 255 ? 255 : iR;
                        iR = iR < 0 ? 0 : iR;
                        int iG = (int)green;
                        iG = iG > 255 ? 255 : iG;
                        iG = iG < 0 ? 0 : iG;
                        int iB = (int)blue;
                        iB = iB > 255 ? 255 : iB;
                        iB = iB < 0 ? 0 : iB;

                        if (invertColors)
                        {
                            if (iB == 255 && iG == 255 && iR == 255)
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
                        if (limeToWhite && iG == 255 && iR == 255)
                        {
                            iB = 255;
                        }
                        rgb[pos] = (byte)iB;
                        rgb[pos + 1] = (byte)iG;
                        rgb[pos + 2] = (byte)iR;
                    }
                }
            }

            bitmap.UnlockBits(bitmapData);
        }
        internal static byte[] GetPixelAtPosition(Bitmap bitmap, int pixelX, int pixelY)
        {
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            byte r, g, b;
            unsafe
            {
                byte* row = (byte*)data.Scan0 + (pixelY * data.Stride);
                b = row[pixelX * 4];
                g = row[(pixelX * 4) + 1];
                r = row[(pixelX * 4) + 2];
            }

            bitmap.UnlockBits(data);

            return new[] { r, g, b };
        }
        internal static bool BitmapIsCertainColor(Bitmap bitmap, int red, int green, int blue)
        {
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

            unsafe
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    byte* row = (byte*)data.Scan0 + (y * data.Stride);

                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        int b = row[x * 4];
                        int g = row[(x * 4) + 1];
                        int r = row[(x * 4) + 2];

                        if (blue - b > 12 || green - g > 12 || red - r > 12)
                            return false;
                    }
                }
            }
            bitmap.UnlockBits(data);

            return true;
        }
        internal static unsafe Bitmap CropImage(Bitmap bitmap, Rectangle rectangle)
        {
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            int bitmapBytes = bitmapData.Stride / bitmapData.Width;
            byte* srcPtr = (byte*)bitmapData.Scan0.ToPointer() + rectangle.Y * bitmapData.Stride + rectangle.X * bitmapBytes;
            int srcStride = bitmapData.Stride;

            Bitmap croppedBitmap = new Bitmap(rectangle.Width, rectangle.Height, bitmap.PixelFormat);
            BitmapData croppedBitmapData = croppedBitmap.LockBits(new Rectangle(0, 0, croppedBitmap.Width, croppedBitmap.Height), ImageLockMode.WriteOnly, croppedBitmap.PixelFormat);
            byte* dstPtr = (byte*)croppedBitmapData.Scan0.ToPointer();
            int dstStride = croppedBitmapData.Stride;

            for (int y = 0; y < rectangle.Height; y++)
            {
                memcpy(dstPtr, srcPtr, dstStride);
                srcPtr += srcStride;
                dstPtr += dstStride;
            }
            bitmap.UnlockBits(bitmapData);
            croppedBitmap.UnlockBits(croppedBitmapData);

            return croppedBitmap;
        }
        private static int[,] LabelImage(Bitmap image, out int labelCount)
        {
            BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);
            int nrow = image.Height;
            int ncol = image.Width;
            int[,] img = new int[nrow, ncol];
            int[,] label = new int[nrow, ncol];
            int imageBytes = Image.GetPixelFormatSize(bitmapData.PixelFormat) / 8;

            unsafe
            {
                byte* rgb = (byte*)bitmapData.Scan0;

                for (int x = 0; x < bitmapData.Width; x++)
                {
                    for (int y = 0; y < bitmapData.Height; y++)
                    {
                        int pos = (y * bitmapData.Stride) + (x * imageBytes);
                        if (rgb != null)
                        {
                            img[y, x] = (rgb[pos] + rgb[pos + 1] + rgb[pos + 2]) / 3;
                            label[y, x] = 0;
                        }
                    }
                }
            }
            image.UnlockBits(bitmapData);

            int lab = 1;
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

                        stack.Push(new[] { r, c });
                        label[r, c] = lab;

                        while (stack.Count != 0)
                        {
                            int[] pos = stack.Pop();
                            int y = pos[0];
                            int x = pos[1];

                            if (y > 0 && x > 0)
                            {
                                if (img[y - 1, x - 1] > 0 && label[y - 1, x - 1] == 0)
                                {
                                    stack.Push(new[] { y - 1, x - 1 });
                                    label[y - 1, x - 1] = lab;
                                }
                            }

                            if (y > 0)
                            {
                                if (img[y - 1, x] > 0 && label[y - 1, x] == 0)
                                {
                                    stack.Push(new[] { y - 1, x });
                                    label[y - 1, x] = lab;
                                }
                            }

                            if (y > 0 && x < ncol - 1)
                            {
                                if (img[y - 1, x + 1] > 0 && label[y - 1, x + 1] == 0)
                                {
                                    stack.Push(new[] { y - 1, x + 1 });
                                    label[y - 1, x + 1] = lab;
                                }
                            }

                            if (x > 0)
                            {
                                if (img[y, x - 1] > 0 && label[y, x - 1] == 0)
                                {
                                    stack.Push(new[] { y, x - 1 });
                                    label[y, x - 1] = lab;
                                }
                            }

                            if (x < ncol - 1)
                            {
                                if (img[y, x + 1] > 0 && label[y, x + 1] == 0)
                                {
                                    stack.Push(new[] { y, x + 1 });
                                    label[y, x + 1] = lab;
                                }
                            }

                            if (y < nrow - 1 && x > 0)
                            {
                                if (img[y + 1, x - 1] > 0 && label[y + 1, x - 1] == 0)
                                {
                                    stack.Push(new[] { y + 1, x - 1 });
                                    label[y + 1, x - 1] = lab;
                                }
                            }

                            if (y < nrow - 1)
                            {
                                if (img[y + 1, x] > 0 && label[y + 1, x] == 0)
                                {
                                    stack.Push(new[] { y + 1, x });
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
                                    stack.Push(new[] { y + 1, x + 1 });
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
        private static Bitmap[] FetchConnectedComponentLabels(Bitmap image)
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
                    int width = rects[i].Width - rects[i].X + 1;
                    int height = rects[i].Height - rects[i].Y + 1;

                    if (height / (double)image.Height > 0.6)
                    {
                        bitmaps.Add(new Bitmap(width, height, image.PixelFormat));

                        BitmapData bitmapData = bitmaps[bitmaps.Count - 1].LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, image.PixelFormat);
                        int imageBytes = Image.GetPixelFormatSize(bitmapData.PixelFormat) / 8;

                        unsafe
                        {
                            byte* rgb = (byte*)bitmapData.Scan0;

                            for (int x = 0; x < width; x++)
                            {
                                for (int y = 0; y < height; y++)
                                {
                                    int pos = (y * bitmapData.Stride) + (x * imageBytes);

                                    if (labels[y + rects[i].Y, x + rects[i].X] == i)
                                    {
                                        if (rgb != null)
                                        {
                                            rgb[pos] = 255;
                                            rgb[pos + 1] = 255;
                                            rgb[pos + 2] = 255;
                                        }
                                    }
                                }
                            }
                            bitmaps[bitmaps.Count - 1].UnlockBits(bitmapData);
                        }
                    }
                }
            }

            return bitmaps.ToArray();
        }
        internal static byte[] BitmapToBytes(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }
        internal static Bitmap ReduceBitmapSize(Bitmap bitmap, int percent)
        {
            float nPercent = (float)percent / 100;

            int sourceWidth = bitmap.Width;
            int sourceHeight = bitmap.Height;
            const int sourceX = 0;
            const int sourceY = 0;

            const int destX = 0;
            const int destY = 0;
            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(destWidth, destHeight, PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.InterpolationMode = InterpolationMode.High;
            grPhoto.DrawImage(bitmap, new Rectangle(destX, destY, destWidth, destHeight), new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight), GraphicsUnit.Pixel);
            grPhoto.Dispose();

            return bmPhoto;
        }
        internal static string FetchTextFromBitmap(Bitmap bitmap, NetworkEnum networkEnum)
        {
            string text = string.Empty;
            try
            {
                Bitmap[] bitmaps = FetchConnectedComponentLabels(bitmap);
                if (bitmaps.Length > 0)
                {
                    int[] predictions = AppData.networks[(int)networkEnum].GetPredictionFromBitmaps(bitmaps);
                    switch (networkEnum)
                    {
                        case NetworkEnum.Maps:
                        case NetworkEnum.Heroes:
                            text = string.Join("", predictions.Select(x => (char)((int)'A' + x)).ToArray());
                            break;
                        case NetworkEnum.Ratings:
                        case NetworkEnum.Stats:
                            text = string.Join("", predictions.Select(x => x.ToString()).ToArray());
                            break;
                        case NetworkEnum.Players:
                            foreach (int prediction in predictions)
                            {
                                if (((int)'A' + prediction - 9) < (int)'A')
                                {
                                    text += (prediction + 1).ToString();
                                }
                                else
                                {
                                    text += (char)((int)'A' + prediction - 9);
                                }
                            }
                            break;

                    }
                }
                foreach (Bitmap bmp in bitmaps) bmp.Dispose();
            }
            catch (Exception e)
            {
                Functions.DebugMessage("BitmapManipulation.FetchTextFromBitmap() error: " + e);
            }
            return text;
        }
        internal static void InvertColors(Bitmap bitmap)
        {
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            unsafe
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    byte* row = (byte*)data.Scan0 + (y * data.Stride);

                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        byte b = row[x * 4];
                        byte g = row[(x * 4) + 1];
                        byte r = row[(x * 4) + 2];
                        row[x * 4] = b == 255 ? (byte)0 : (byte)255;
                        row[(x * 4) + 1] = g == 255 ? (byte)0 : (byte)255;
                        row[(x * 4) + 2] = r == 255 ? (byte)0 : (byte)255;
                    }
                }
            }

            bitmap.UnlockBits(data);
        }
    }
}
