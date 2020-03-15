using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using TestHaarCSharp;

namespace ProcessImages
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            ApplyWavelet();
            s.Stop();

            Console.WriteLine($"Done {TimeSpan.FromMilliseconds(s.ElapsedMilliseconds).TotalMinutes}");
            Console.ReadLine();
        }

        static void ApplyWavelet()
        {
            int times = 7;
            string[] images = Directory.GetFiles(@"..\..\..\TestHaarCSharp\Resources_Noisy\", "*", SearchOption.AllDirectories);
            Parallel.ForEach(images, (i) =>
            {
                Bitmap initial = new Bitmap(Image.FromFile(i));

                //var res = AddNoise(initial, 50);
                //res.Save($@"..\..\..\TestHaarCSharp\Resources_noisy\{Path.GetFileName(i)}", res.RawFormat);

                Bitmap forward = new Bitmap(initial);

                var channels = ColorChannels.CreateColorChannels(initial.Width, initial.Height);
                var transform = WaveletTransform.CreateTransform(true, times);
                var imageProcessor = new ImageProcessor(channels, transform);
                imageProcessor.ApplyTransform(forward);

                string newPath = Path.Combine(@"..\..\..\TestHaarCSharp\Forward_Noisy", Path.GetFileName(i));
                string dirName = Path.GetDirectoryName(newPath);
                if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
                forward.Save(newPath, initial.RawFormat);


                Bitmap inverse = new Bitmap(forward);
                transform = WaveletTransform.CreateTransform(false, times);
                imageProcessor = new ImageProcessor(channels, transform);
                imageProcessor.ApplyTransform(inverse);

                var rmse = GetRMSE(initial, inverse);

                newPath = Path.Combine(@"..\..\..\TestHaarCSharp\Inverse_Noisy",
                    $"{Path.GetFileNameWithoutExtension(i)}_{rmse.ToString("#.##")}{Path.GetExtension(i)}");
                dirName = Path.GetDirectoryName(newPath);
                if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
                inverse.Save(newPath, inverse.RawFormat);
            });
        }

        public static Bitmap AddNoise(Bitmap original, int amount)
        {
            Bitmap result = new Bitmap(original);
            Random rnd = new Random();
            for (int x = 0; x < result.Width; ++x)
            {
                for (int y = 0; y < result.Height; ++y)
                {
                    Color currentPixel = original.GetPixel(x, y);
                    int R = currentPixel.R + rnd.Next(-amount, amount + 1);
                    int G = currentPixel.G + rnd.Next(-amount, amount + 1);
                    int B = currentPixel.B + rnd.Next(-amount, amount + 1);
                    R = R > 255 ? 255 : R;
                    R = R < 0 ? 0 : R;
                    G = G > 255 ? 255 : G;
                    G = G < 0 ? 0 : G;
                    B = B > 255 ? 255 : B;
                    B = B < 0 ? 0 : B;
                    Color temp = Color.FromArgb(R, G, B);
                    result.SetPixel(x, y, temp);
                }
            }
            return result;
        }


        public static double GetRMSE(Bitmap originalImage, Bitmap markedImage)
        {
            int width = originalImage.Width, height = originalImage.Height;
            double mse = 0;
            double mseR = 0;
            double mseG = 0;
            double mseB = 0;
            double rmse = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    mseR = mseR + ((markedImage.GetPixel(x, y).R - originalImage.GetPixel(x, y).R) * (markedImage.GetPixel(x, y).R - originalImage.GetPixel(x, y).R));
                    mseG = mseG + ((markedImage.GetPixel(x, y).G - originalImage.GetPixel(x, y).G) * (markedImage.GetPixel(x, y).G - originalImage.GetPixel(x, y).G));
                    mseB = mseB + ((markedImage.GetPixel(x, y).B - originalImage.GetPixel(x, y).B) * (markedImage.GetPixel(x, y).B - originalImage.GetPixel(x, y).B));
                }
            }

            mse = (1.0 / (3 * width * height)) * (mseR + mseG + mseB);
            rmse = Math.Sqrt(mse);

            return rmse;
        }
    }
}
