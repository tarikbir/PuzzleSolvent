using System;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PuzzleSolvent
{
    public partial class ImageSplit : Button
    {
        public BitmapImage bitmap = new BitmapImage();
        public ImageSplit(System.Drawing.Image split)
        {
            BorderThickness = new Thickness(0, 0, 0, 0);
            Background = new SolidColorBrush(Color.FromRgb(0, 0, 0)) { Opacity = 0 };
            Margin = new Thickness(0);

            
            bitmap.BeginInit();
            MemoryStream ms = new MemoryStream();
            split.Save(ms, ImageFormat.Bmp);
            ms.Seek(0, SeekOrigin.Begin);
            bitmap.StreamSource = ms;
            bitmap.EndInit();

            AddChild(new System.Windows.Controls.Image() { Name = "ButtonImage", Source = bitmap, Stretch = Stretch.Uniform, Margin = new Thickness(0) });

        }

        internal void swapChildren(ImageSplit secondBox)
        {
            var temp = Content;
            Content = secondBox.Content;
            secondBox.Content = temp;
        }
    }
}
