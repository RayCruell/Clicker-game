using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public abstract class CCollectable
    {
        private Point position;
        protected Size size;
        private double lifetime;
        protected Ellipse sprite;

        public abstract BigNumber GetDamageValue(CPlayer player);

        public CCollectable(Point position, double size, double lifetime)
        {
            this.position = position;
            this.size = new Size(size, size);
            this.lifetime = lifetime;

            sprite = new Ellipse();
            sprite.StrokeThickness = 2;
            sprite.Stroke = Brushes.Black;
            sprite.HorizontalAlignment = HorizontalAlignment.Center;
            sprite.VerticalAlignment = VerticalAlignment.Center;
            sprite.Width = this.size.Width;
            sprite.Height = this.size.Height;
            sprite.RenderTransform = new TranslateTransform(position.X, position.Y);
        }

        public bool isMouseOnObject(Point mousePosition)
        {
            return mousePosition.X >= position.X &&
                   mousePosition.X <= position.X + size.Width &&
                   mousePosition.Y >= position.Y &&
                   mousePosition.Y <= position.Y + size.Height;
        }

        public Ellipse getSprite()
        {
            return sprite;
        }

        public bool updateLifetime(double delta)
        {
            lifetime -= delta;
            return lifetime <= 0;
        }

        public abstract bool onClick(CPlayer player, Point mousePosition);
    }
}
