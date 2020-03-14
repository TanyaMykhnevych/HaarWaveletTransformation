using ImageMagick;
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
            int times = 1;
            string[] images = Directory.GetFiles(@"..\..\..\TestHaarCSharp\Resources\", "*", SearchOption.AllDirectories);
            Parallel.ForEach(images, (i) =>
            {
                Bitmap initial = new Bitmap(Image.FromFile(i));
                Bitmap forward = new Bitmap(initial);

                var channels = ColorChannels.CreateColorChannels(initial.Width, initial.Height);
                var transform = WaveletTransform.CreateTransform(true, times);
                var imageProcessor = new ImageProcessor(channels, transform);
                imageProcessor.ApplyTransform(forward);

                string newPath = Path.Combine(@"..\..\..\TestHaarCSharp\Forward", Path.GetFileName(i));
                string dirName = Path.GetDirectoryName(newPath);
                if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
                forward.Save(newPath, initial.RawFormat);


                Bitmap inverse = new Bitmap(forward);
                transform = WaveletTransform.CreateTransform(false, times);
                imageProcessor = new ImageProcessor(channels, transform);
                imageProcessor.ApplyTransform(inverse);

                var rmse = GetRMSE(initial, inverse);

                newPath = Path.Combine(@"..\..\..\TestHaarCSharp\Inverse",
                    $"{Path.GetFileNameWithoutExtension(i)}_{rmse.ToString("#.##")}{Path.GetExtension(i)}");
                dirName = Path.GetDirectoryName(newPath);
                if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
                inverse.Save(newPath, inverse.RawFormat);
            });
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
