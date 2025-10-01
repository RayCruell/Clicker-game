using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ClickerGame.Models
{
    public class Icon
    {
        [JsonInclude] public string Name { get; private set; }
        [JsonInclude] public int Width { get; private set; }
        [JsonInclude] public int Height { get; private set; }
        [JsonInclude] public string ImagePath { get; private set; }
        [JsonInclude] public double PosX { get; private set; } = 0;
        [JsonInclude] public double PosY { get; private set; } = 0;

        [JsonIgnore] public Rectangle Rectangle { get; private set; }

        public Icon(int width, int height, string imagePath)
        {
            Width = width;
            Height = height;
            ImagePath = imagePath ?? throw new ArgumentNullException(nameof(imagePath));
            Name = System.IO.Path.GetFileNameWithoutExtension(imagePath);
            CreateRectangle();
        }

        public void CreateRectangle()
        {
            var rect = new Rectangle
            {
                Width = Width,
                Height = Height,
                Stroke = Brushes.Black
            };

            try
            {
                var ib = new ImageBrush();
                ib.ImageSource = new BitmapImage(new Uri(ImagePath, UriKind.Absolute));
                ib.Stretch = Stretch.Fill;
                rect.Fill = ib;
            }
            catch
            {
                rect.Fill = Brushes.LightGray;
            }

            rect.RenderTransform = new TranslateTransform(PosX, PosY);
            Rectangle = rect;
        }

        public void SetPosition(double x, double y)
        {
            PosX = x;
            PosY = y;
            if (Rectangle != null)
                Rectangle.RenderTransform = new TranslateTransform(PosX, PosY);
        }

        public bool ContainsPoint(Point canvasPoint)
        {
            return canvasPoint.X >= PosX && canvasPoint.X <= PosX + Width
                && canvasPoint.Y >= PosY && canvasPoint.Y <= PosY + Height;
        }
    }
}