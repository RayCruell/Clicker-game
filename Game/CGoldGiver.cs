using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Game
{
    public class CGoldGiver : CCollectable
    {
        private BigNumber goldAmount;

        public CGoldGiver(Point position, double size, double lifetime, BigNumber gold)
            : base(position, size, lifetime)
        {
            this.goldAmount = gold;
            sprite.Fill = Brushes.Gold;
        }

        public override bool onClick(CPlayer player, Point mousePosition)
        {
            if (!isMouseOnObject(mousePosition)) return false;
            return true;
        }

        public BigNumber GetGoldAmount() => goldAmount;

        public override BigNumber GetDamageValue(CPlayer player)
        {
            return player.Damage;
        }
    }
}
