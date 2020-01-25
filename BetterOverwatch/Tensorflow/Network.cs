using NumSharp;
using System.Collections.Generic;
using Tensorflow;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace BetterOverwatch.Tensorflow
{
    class Network
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

        public Network(Graph graph, Session session)
        {
            //Graph graph = tf.Graph().as_default();
            //Session session = tf.Session(graph);
            //var saver = tf.train.import_meta_graph(Path.Combine(AppData.configPath, "_data\\network.meta"));
            //saver.restore(session, Path.Combine(AppData.configPath, "_data\\network"));
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
            this.graph = graph;
            this.session = session;

            this.session.graph.get_tensor_by_name("Train/Ratings/Loss/loss:0");
            this.graph.get_tensor_by_name("Train/Ratings/Accuracy/accuracy:0");
            x = this.session.graph.get_tensor_by_name("Input/Ratings/X:0");
            y = this.session.graph.get_tensor_by_name("Input/Ratings/Y:0");
            cls_prediction = this.session.graph.get_tensor_by_name("Train/Ratings/Prediction/predictions:0");
            prob = this.session.graph.get_tensor_by_name("Train/Ratings/Prediction/prob:0");
            this.session.graph.get_tensor_by_name("Train/Ratings/Optimizer/Adam-op:0");
        }
        public string Run(Bitmap[] bitmapArrToTest)
        {
            this.bitmapArrToTest = bitmapArrToTest;
            ndArrToTest = np.zeros(bitmapArrToTest.Length, img_h, img_w, n_channels);

            LoadImages(bitmapArrToTest, ndArrToTest);
            Test(session);

            return GetOutputString();
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
                g.InterpolationMode = InterpolationMode.Bilinear;
                g.DrawImage(src, new Rectangle(0, 0, width, height), new Rectangle(0, 0, src.Width, src.Height), GraphicsUnit.Pixel);
            }
            return result;
        }
        private NDArray TensorFromBitmap(Bitmap bitmap)
        {
            float[,] floatArray;
            Bitmap resizedImage = ResizeBitmap(bitmap, img_w, img_h);
            bitmap.Save($@"C:\test\t\delete\{System.Guid.NewGuid()}.png", ImageFormat.Bmp);
            //resizedImage.Save(@"C:\test\t\delete\" + System.Guid.NewGuid() + ".png", ImageFormat.Exif);
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
