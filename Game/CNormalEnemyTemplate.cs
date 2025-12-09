using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Text.Json.Serialization;

namespace Game
{
    public class CNormalEnemyTemplate : CEnemyTemplate
    {
        public CNormalEnemyTemplate() : base() { }

        public CNormalEnemyTemplate(string name, string iconName, int baseLife, int lifeModifier, 
                                   int baseGold, int goldModifier, int spawnChance)
            : base(name, iconName, baseLife, lifeModifier, baseGold, goldModifier, spawnChance)
        {
        }
    }
}