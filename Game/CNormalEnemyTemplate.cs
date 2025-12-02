using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Game
{
    public class CNormalEnemyTemplate : CEnemyTemplate
{
    public CNormalEnemyTemplate() : base() { }

    public CNormalEnemyTemplate(string name, string iconName, int baseLife, double lifeModifier,
                               int baseGold, double goldModifier, double spawnChance)
        : base(name, iconName, baseLife, lifeModifier, baseGold, goldModifier, spawnChance)
    {
    }
}
}
