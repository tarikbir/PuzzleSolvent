using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PuzzleSolvent
{
    public partial class ImageSplit : Button, IComparable
    {
        public int buttonID;
        private Emgu.CV.Image<Bgr, Byte> cvImage;

        public ImageSplit(System.Drawing.Image split, int id)
        {
            cvImage = new Emgu.CV.Image<Bgr, Byte>((System.Drawing.Bitmap)split);
            BorderThickness = new Thickness(0, 0, 0, 0);
            Background = new SolidColorBrush(Color.FromRgb(0, 0, 0)) { Opacity = 0 };
            Margin = new Thickness(0);
            buttonID = id;
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            MemoryStream ms = new MemoryStream();
            cvImage.Bitmap.Save(ms, ImageFormat.Bmp);
            ms.Seek(0, SeekOrigin.Begin);
            bitmap.StreamSource = ms;
            bitmap.EndInit();

            AddChild(new System.Windows.Controls.Image() { Name="ButtonImage", Source = bitmap, Stretch=Stretch.Uniform, Margin = new Thickness(0) });
        }

        internal void swapChildren(ImageSplit secondBox)
        {
            var temp = Content;
            var tempCvImage = cvImage;
            Content = secondBox.Content;
            cvImage = secondBox.cvImage;
            secondBox.Content = temp;
            secondBox.cvImage = tempCvImage;
        }

        internal System.Drawing.Bitmap getBitmap()
        {
            System.Drawing.Bitmap bmpOut = null;

            using (MemoryStream ms = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)(this.Content as System.Windows.Controls.Image).Source));
                encoder.Save(ms);

                using (var bmp = new System.Drawing.Bitmap(ms))
                {
                    bmpOut = new System.Drawing.Bitmap(bmp);
                }
            }

            return bmpOut;
        }

        public int CompareTo(object obj)
        {
            int comparison = 0;
            int threshold = 10;
            System.Drawing.Image secondObj;
            try
            {
                if (obj == null) throw new NullReferenceException();
                secondObj = obj as System.Drawing.Image;
            }
            catch
            {
                return -1;
            }

            var secondObjBitmap = new Emgu.CV.Image<Bgr, Byte>(secondObj as System.Drawing.Bitmap);

            var bmp = cvImage.AbsDiff(secondObjBitmap).ToBitmap();
            bmp.Save("absdiff.bmp");
            cvImage.Save("cvimage.bmp");
            secondObjBitmap.Save("secondobjbitmap.bmp");
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    if (bmp.GetPixel(j,i).R + bmp.GetPixel(j, i).G + bmp.GetPixel(j, i).B < 3 * threshold)
                    {
                        comparison++;
                    }
                }
            }

            comparison = comparison / (cvImage.Height * cvImage.Width);

            return comparison;
        }
    }
}
