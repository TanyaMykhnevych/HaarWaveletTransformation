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
            ApplyWaveletForward();
            ApplyWaveletInverse();
            s.Stop();

            Console.WriteLine($"Done {TimeSpan.FromMilliseconds(s.ElapsedMilliseconds).TotalMinutes}");
            Console.ReadLine();
        }

        static void ApplyWaveletForward()
        {
            string[] images = Directory.GetFiles(@"..\..\..\TestHaarCSharp\Resources\", "*", SearchOption.AllDirectories);
            Parallel.ForEach(images, (i) =>
            {
                Bitmap bmp = new Bitmap(Image.FromFile(i));

                var channels = ColorChannels.CreateColorChannels(bmp.Width, bmp.Height);
                var transform = WaveletTransform.CreateTransform(true, 1);

                var imageProcessor = new ImageProcessor(channels, transform);
                imageProcessor.ApplyTransform(bmp);

                Bitmap result = bmp;
                string newPath = Path.Combine(@"..\..\..\TestHaarCSharp\Forward", Path.GetFileName(i));
                string dirName = Path.GetDirectoryName(newPath);
                if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);

                result.Save(newPath, bmp.RawFormat);
            });
        }

        static void ApplyWaveletInverse()
        {
            string[] images = Directory.GetFiles(@"..\..\..\TestHaarCSharp\Forward\", "*", SearchOption.AllDirectories);
            Parallel.ForEach(images, (i) =>
            {
                Bitmap bmp = new Bitmap(Image.FromFile(i));

                var channels = ColorChannels.CreateColorChannels(bmp.Width, bmp.Height);
                var transform = WaveletTransform.CreateTransform(false, 1);

                var imageProcessor = new ImageProcessor(channels, transform);
                imageProcessor.ApplyTransform(bmp);

                Bitmap result = bmp;
                string newPath = Path.Combine(@"..\..\..\TestHaarCSharp\Inverse", Path.GetFileName(i));
                string dirName = Path.GetDirectoryName(newPath);
                if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);

                result.Save(newPath, bmp.RawFormat);
            });
        }
    }
}
