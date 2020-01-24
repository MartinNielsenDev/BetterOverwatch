using NumSharp;
using System.Collections.Generic;
using System.IO;
using Tensorflow;
using static Tensorflow.Binding;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace BetterOverwatch.TensorFlow
{
    class TensorFlowNetwork
    {
        Bitmap[] bitmapArrToTest;
        NDArray ndArrToTest;
        Dictionary<int, string> keyToValue;
        int img_h = 32;
        int img_w = 32;
        int img_std = 255;
        int n_channels = 1;

        Tensor x, y;
        Tensor cls_prediction, prob;

        NDArray CNNResult, Test_Data;
        Graph graph;
        Session session;

        public TensorFlowNetwork()
        {
            keyToValue = new Dictionary<int, string>()
            {
                { 0, "0" },
                { 1, "1" },
                { 2, "2" },
                { 3, "3" },
                { 4, "4" },
                { 5, "5" },
                { 6, "6" },
                { 7, "7" },
                { 8, "8" },
                { 9, "9" }
            };
            graph = tf.Graph().as_default();
            session = tf.Session(graph);
            LoadModel(session);
        }
        public string Run(Bitmap[] bitmapArrToTest)
        {
            this.bitmapArrToTest = bitmapArrToTest;
            ndArrToTest = np.zeros(bitmapArrToTest.Length, img_h, img_w, n_channels);

            LoadImages(bitmapArrToTest, ndArrToTest);
            Test(session);

            return GetOutputString();
        }
        private void LoadModel(Session sess)
        {
            var saver = tf.train.import_meta_graph(Path.Combine(AppData.configPath, @"_data/network.meta"));
            saver.restore(sess, Path.Combine(AppData.configPath, @"_data/network"));

            sess.graph.get_tensor_by_name("Train/Loss/loss:0");
            sess.graph.get_tensor_by_name("Train/Accuracy/accuracy:0");
            x = sess.graph.get_tensor_by_name("Input/X:0");
            y = sess.graph.get_tensor_by_name("Input/Y:0");
            cls_prediction = sess.graph.get_tensor_by_name("Train/Prediction/predictions:0");
            prob = sess.graph.get_tensor_by_name("Train/Prediction/prob:0");
            sess.graph.get_tensor_by_name("Train/Optimizer/Adam-op:0");
        }
        private void LoadImages(Bitmap[] bitmapArr, NDArray ndArr)
        {
            for (int i = 0; i < bitmapArr.Length; i++)
            {
                ndArr[i] = TensorFromBitmap(bitmapArr[i]);
            }
        }
        public float[,] ToGrayscale(Bitmap colorBitmap)
        {
            int Width = colorBitmap.Width;
            int Height = colorBitmap.Height;
            float[,] floatArray = new float[img_w, img_h];

            unsafe
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        Color clr = colorBitmap.GetPixel(x, y);
                        byte byPixel = (byte)((30 * clr.R + 59 * clr.G + 11 * clr.B) / 100);

                        floatArray[y, x] = (float)byPixel / (float)img_std;
                    }
                }
            }
            return floatArray;
        }
        private Bitmap ResizeBitmap(Bitmap src, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                //g.InterpolationMode = InterpolationMode.Bilinear;
                g.DrawImage(src, new Rectangle(0, 0, width, height), new Rectangle(0, 0, src.Width, src.Height), GraphicsUnit.Pixel);
            }
            return result;
        }
        private NDArray TensorFromBitmap(Bitmap bitmap)
        {
            float[,] floatArray;
            Bitmap resizedImage = ResizeBitmap(bitmap, img_w, img_h);
            //bitmap.Save(@"C:\test\t\delete\" + System.Guid.NewGuid() + ".png", ImageFormat.Bmp);
            //resizedImage.Save(@"C:\test\t\delete\" + System.Guid.NewGuid() + ".png", ImageFormat.Bmp);
            floatArray = ToGrayscale(resizedImage);
            NDArray nd = new NDArray(floatArray, new Shape(1, img_w, img_h, n_channels));

            return nd;
        }

        public void Test(Session sess)
        {
            (CNNResult, Test_Data) = sess.run((cls_prediction, prob), (x, ndArrToTest));
        }
        private string GetOutputString()
        {
            string output = "";

            for (int i = 0; i < bitmapArrToTest.Length; i++)
            {
                bitmapArrToTest[i].Dispose();
                output += keyToValue[(int)(CNNResult[i])];
            }
            return output;
        }
    }
}
