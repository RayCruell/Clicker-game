using System.IO;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Game
{
    public class CIcon
    {
        public string Name { get; set; }
        public string ImagePath { get; set; }
        private int iconWidth;                                  
        private int iconHeight;                                  
        private Point position;                                     
        private Rectangle icon;

        public CIcon() { }
        public CIcon(int iconWidth, int iconHeight, string imagePath)
        {
            Debug.WriteLine($"=== СОЗДАНИЕ CICON ===");
            Debug.WriteLine($"Полученные параметры: Width={iconWidth}, Height={iconHeight}, Path={imagePath}");

            this.iconWidth = iconWidth;
            this.iconHeight = iconHeight;
            this.position = new Point(0, 0);

            this.ImagePath = imagePath;

            Debug.WriteLine($"Проверяем imagePath...");
            Debug.WriteLine($"IsNullOrEmpty: {string.IsNullOrEmpty(imagePath)}");

            if (!string.IsNullOrEmpty(imagePath))
            {
                Debug.WriteLine($"File.Exists: {File.Exists(imagePath)}");
                this.Name = System.IO.Path.GetFileNameWithoutExtension(imagePath);
                Debug.WriteLine($"Имя установлено: {this.Name}");
            }
            else
            {
                this.Name = "Unknown";
                Debug.WriteLine($"Имя установлено как 'Unknown'");
            }

            icon = new Rectangle();
            icon.Stroke = Brushes.Black;

            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                Debug.WriteLine($"Пытаемся создать ImageBrush...");
                try
                {
                    ImageBrush brush = new ImageBrush();
                    brush.AlignmentX = AlignmentX.Left;
                    brush.AlignmentY = AlignmentY.Top;

                    Debug.WriteLine($"Создаем BitmapImage...");
                    var uri = new Uri(imagePath, UriKind.Absolute);
                    Debug.WriteLine($"URI создан: {uri}");

                    brush.ImageSource = new BitmapImage(uri);
                    Debug.WriteLine($"BitmapImage создан успешно!");

                    icon.Fill = brush;
                    Debug.WriteLine($"ImageBrush установлен в Rectangle!");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ ОШИБКА при создании ImageBrush: {ex.GetType().Name} - {ex.Message}");
                    icon.Fill = Brushes.Red; // Красный цвет чтобы было видно ошибку
                }
            }
            else
            {
                Debug.WriteLine($"❌ Условие не выполнено - path пустой или файл не существует");
                icon.Fill = Brushes.Gray;
            }

            icon.RenderTransform = new TranslateTransform(position.X, position.Y);
            icon.HorizontalAlignment = HorizontalAlignment.Left;
            icon.VerticalAlignment = VerticalAlignment.Top;
            icon.Width = iconWidth;
            icon.Height = iconHeight;

            Debug.WriteLine($"=== CICON СОЗДАН ===");
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