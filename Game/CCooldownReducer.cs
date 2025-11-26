using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Game
{
    public class CCooldownReducer : CCollectable
    {
        private double cooldownReduction;

        public CCooldownReducer(Point position, double size, double lifetime, double reduction)
            : base(position, size, lifetime)
        {
            this.cooldownReduction = reduction;
            sprite.Fill = Brushes.Blue;
        }

        public override bool onClick(CPlayer player, Point mousePosition)
        {
            if (!isMouseOnObject(mousePosition)) return false;
            return true;
        }
        public double GetCooldownReduction() => 0.95;

        public override BigNumber GetDamageValue(CPlayer player)
        {
            return player.Damage;
        }


    }
}
