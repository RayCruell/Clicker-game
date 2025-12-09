using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Game
{
    public class CDodgingEnemyTemplate : CEnemyTemplate
    {
        private double dodgeChance; // Шанс уворота в %

        [JsonInclude]
        public double DodgeChance
        {
            get { return dodgeChance; }
            set { if (value >= 0 && value <= 100) dodgeChance = value; else dodgeChance = 25; }
        }

        public CDodgingEnemyTemplate() : base()
        {
            dodgeChance = 25;
        }

        public CDodgingEnemyTemplate(string name, string iconName, int baseLife, int lifeModifier,
                                    int baseGold, int goldModifier, int spawnChance, double dodgeChance) 
            : base(name, iconName, baseLife, lifeModifier, baseGold, goldModifier, spawnChance)
        {
            DodgeChance = dodgeChance;
        }
    }
}