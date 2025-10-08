using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Game
{
    public class Icon
    {
        public string Name { get; private set; }
        public string ImagePath { get; set; }
        private int iconWidth;                                  
        private int iconHeight;                                  
        private Point position;                                     
        private Rectangle icon;                                      

        public Icon(int iconWidth,
                    int iconHeight,
                    string imagePath)
        {
            this.iconWidth = iconWidth;
            this.iconHeight = iconHeight;
            this.position = new Point(0, 0);
            this.Name = System.IO.Path.GetFileNameWithoutExtension(imagePath);

            icon = new Rectangle();
            icon.Stroke = Brushes.Black;

            ImageBrush brush = new ImageBrush();
            brush.AlignmentX = AlignmentX.Left;
            brush.AlignmentY = AlignmentY.Top;
            brush.ImageSource = new BitmapImage(new Uri(imagePath, UriKind.Absolute));

            icon.Fill = brush;
            icon.RenderTransform = new TranslateTransform(position.X, position.Y);
            icon.HorizontalAlignment = HorizontalAlignment.Left;
            icon.VerticalAlignment = VerticalAlignment.Top;
            icon.Width = iconWidth;
            icon.Height = iconHeight;
        }

        public string GetName() => Name;                            
        public double X() => position.X;                             
        public double Y() => position.Y;                           
        public int GetIconWidth() => iconWidth;                    
        public int GetIconHeight() => iconHeight;                  
        public Rectangle GetIcon() => icon;                    

        public void SetPosition(Point newPosition)
        {
            position = newPosition;
            icon.RenderTransform = new TranslateTransform(position.X, position.Y);
        }

        public bool IsMouseOver(Point mousePosition)
        {
            return mousePosition.X >= position.X &&
                   mousePosition.X <= position.X + iconWidth &&
                   mousePosition.Y >= position.Y &&
                   mousePosition.Y <= position.Y + iconHeight;
        }

        public Rectangle CloneIcon()
        {
            Rectangle clone = new Rectangle
            {
                Width = icon.Width,
                Height = icon.Height,
                Fill = icon.Fill,
                Stroke = icon.Stroke
            };
            clone.RenderTransform = new TranslateTransform(position.X, position.Y);
            return clone;
        }
    }
}