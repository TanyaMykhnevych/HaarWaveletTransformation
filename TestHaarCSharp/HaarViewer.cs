using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows.Forms;


namespace TestHaarCSharp
{
    public partial class HaarViewer : Form
    {
        public HaarViewer()
        {
            this.InitializeComponent();
            this.OriginalImage = (Bitmap)this.pictureBox1.Image;
        }

        private Bitmap OriginalImage { get; set; }

        private Bitmap TransformedImage { get; set; }

        private void ApplyHaarTransform(bool forward, int iterations, Bitmap bmp)
        {
            var maxScale = WaveletTransform.GetMaxScale(bmp.Width, bmp.Height);
            if (iterations < 1 || iterations > maxScale)
            {
                MessageBox.Show(string.Format("Iteration must be Integer from 1 to {0}", maxScale));
                return;
            }

            var time = Environment.TickCount;

            var channels = ColorChannels.CreateColorChannels(bmp.Width, bmp.Height);

            var transform = WaveletTransform.CreateTransform(forward, iterations);

            var imageProcessor = new ImageProcessor(channels, transform);
            imageProcessor.ApplyTransform(bmp);

            if (forward)
            {
                this.TransformedImage = new Bitmap(bmp);
            }

            this.pictureBox1.Image = bmp;
            this.lblDirection.Text = forward ? "Forward" : "Inverse";
            this.lblTransformTime.Text = string.Format("{0} milis.", Environment.TickCount - time);
        }

        private void ApplyHaarTransform(bool forward, string iterations)
        {
            int i;
            int.TryParse(iterations, out i);
            ApplyHaarTransform(forward, i);
        }

        private void ApplyHaarTransform(bool forward, int iterations)
        {
            if (this.OriginalImage == null) return;

            var bmp = forward ? new Bitmap(this.OriginalImage) : new Bitmap(this.TransformedImage);
            this.ApplyHaarTransform(forward, iterations, bmp);
        }

        private void BtnBrowseClick(object sender, EventArgs e)
        {
            var open = new OpenFileDialog
            {
                Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp; *.png; *.tif)|*.jpg; *.jpeg; *.gif; *.bmp; *.png; *.tif"
            };
            if (open.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var tempbitmap = new Bitmap(open.FileName);

            this.OriginalImage = tempbitmap;
            this.pictureBox1.Image = this.OriginalImage;
        }

        private void BtnForwardSafeClick(object sender, EventArgs e)
        {
            this.ApplyHaarTransform(true, (int)this.numericIterations.Value);
        }

        private void BtnInverseSafeClick(object sender, EventArgs e)
        {
            this.ApplyHaarTransform(false, (int)this.numericIterations.Value);
        }

        private void BtnSaveClick(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog { Filter = "Images|*.jpg; *.jpeg; *.gif; *.bmp; *.png; *.tif" };
            var format = ImageFormat.Png;
            if (sfd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var ext = Path.GetExtension(sfd.FileName);
            switch (ext)
            {
                case ".jpg":
                case ".jpeg":
                    format = ImageFormat.Jpeg;
                    break;
                case ".bmp":
                    format = ImageFormat.Bmp;
                    break;
                case ".gif":
                    format = ImageFormat.Gif;
                    break;
                case ".png":
                    format = ImageFormat.Png;
                    break;
                case ".tif":
                    format = ImageFormat.Tiff;
                    break;
            }

            this.pictureBox1.Image.Save(sfd.FileName, format);
        }
    }
}