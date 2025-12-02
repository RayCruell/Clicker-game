using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Game
{
    public class CDamageBooster : CCollectable
    {
        private double damageMultiplier;

        public CDamageBooster(Point position, double size, double lifetime, double multiplier)
        : base(position, size, lifetime)
        {
            this.damageMultiplier = multiplier;
            sprite.Fill = Brushes.Red;
        }

        public override bool onClick(CPlayer player, Point mousePosition)
        {
            if (!isMouseOnObject(mousePosition)) return false;
            return true;
        }

        public double GetDamageMultiplier() => damageMultiplier;

        public override BigNumber GetDamageValue(CPlayer player)
        {
            return player.Damage * damageMultiplier;
        }
        public void ApplyBonus(CPlayer player)
        {
            player.Damage = player.Damage * damageMultiplier;
        }
    }
}
