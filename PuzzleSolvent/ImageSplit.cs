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
        public int ID;

        public ImageSplit(System.Drawing.Image split, int id)
        {
            BorderThickness = new Thickness(0, 0, 0, 0);
            Background = new SolidColorBrush(Color.FromRgb(0, 0, 0)) { Opacity = 0 };
            Margin = new Thickness(0);
            ID = id;
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            MemoryStream ms = new MemoryStream();
            split.Save(ms, ImageFormat.Bmp);
            ms.Seek(0, SeekOrigin.Begin);
            bitmap.StreamSource = ms;
            bitmap.EndInit();

            AddChild(new System.Windows.Controls.Image() { Name="ButtonImage", Source = bitmap, Stretch=Stretch.Uniform, Margin = new Thickness(0) });
        }

        internal void swapChildren(ImageSplit secondBox)
        {
            var temp = Content;
            var tempID = ID;
            Content = secondBox.Content;
            ID = secondBox.ID;
            secondBox.Content = temp;
            secondBox.ID = tempID;
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
            int comparison;
            ImageSplit secondObj;
            try
            {
                if (obj == null) throw new NullReferenceException();
                secondObj = obj as ImageSplit;
            }
            catch
            {
                return -1;
            }

            //comparison = getBitmap().Equals(secondObj.getBitmap()) ? 1 : 0;
            comparison = ID == secondObj.ID ? 1 : 0;

            return comparison;
        }
    }
}
