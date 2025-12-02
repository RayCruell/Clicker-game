using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Game
{
    public class CArmoredEnemyTemplate : CEnemyTemplate
    {
        private double armor;

        [JsonInclude]
        public double Armor
        {
            get { return armor; }
            set { if (value > 0) armor = value; else armor = 25; }
        }

        public CArmoredEnemyTemplate() : base()
        {
            armor = 25;
        }

        public CArmoredEnemyTemplate(string name, string iconName, int baseLife, double lifeModifier,
                                    int baseGold, double goldModifier, double spawnChance, double armor)
            : base(name, iconName, baseLife, lifeModifier, baseGold, goldModifier, spawnChance)
        {
            Armor = armor;
        }
    }
}
