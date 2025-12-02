using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Game
{
    public class CShrinkingEnemyTemplate : CEnemyTemplate
    {
        private double shrinkFactor;

        [JsonInclude]
        public double ShrinkFactor
        {
            get { return shrinkFactor; }
            set { if (value > 0 && value < 1) shrinkFactor = value; else shrinkFactor = 0.7; }
        }

        public CShrinkingEnemyTemplate() : base()
        {
            shrinkFactor = 0.7;
        }

        public CShrinkingEnemyTemplate(string name, string iconName, int baseLife, double lifeModifier,
                                      int baseGold, double goldModifier, double spawnChance, double shrinkFactor)
            : base(name, iconName, baseLife, lifeModifier, baseGold, goldModifier, spawnChance)
        {
            ShrinkFactor = shrinkFactor;
        }
    }
}