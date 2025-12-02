using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;


namespace Game
{
    public class CHealingEnemyTemplate : CEnemyTemplate
    {
        private double healChance;
        private double healPercentage;

        [JsonInclude]
        public double HealChance
        {
            get { return healChance; }
            set { if (value >= 0 && value <= 100) healChance = value; else healChance = 25; }
        }

        [JsonInclude]
        public double HealPercentage
        {
            get { return healPercentage; }
            set { if (value > 0 && value <= 100) healPercentage = value; else healPercentage = 30; }
        }

        public CHealingEnemyTemplate() : base()
        {
            healChance = 25;
            healPercentage = 30;
        }

        public CHealingEnemyTemplate(string name, string iconName, int baseLife, double lifeModifier,
                                    int baseGold, double goldModifier, double spawnChance,
                                    double healChance, double healPercentage)
            : base(name, iconName, baseLife, lifeModifier, baseGold, goldModifier, spawnChance)
        {
            HealChance = healChance;
            HealPercentage = healPercentage;
        }
    }
}