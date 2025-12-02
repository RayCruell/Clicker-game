using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Game
{
    public abstract class CEnemyTemplate
    {
        private string name;
        private string iconName;
        private int baseLife;
        private double lifeModifier;
        private int baseGold;
        private double goldModifier;
        private double spawnChance;

        [JsonInclude]
        public string Name
        {
            get { return name; }
            set { if (!string.IsNullOrEmpty(value)) name = value; else name = "Unknown"; }
        }

        [JsonInclude]
        public string IconName
        {
            get { return iconName; }
            set { iconName = value ?? ""; }
        }

        [JsonInclude]
        public int BaseLife
        {
            get { return baseLife; }
            set { if (value > 0) baseLife = value; else baseLife = 100; }
        }

        [JsonInclude]
        public double LifeModifier
        {
            get { return lifeModifier; }
            set { if (value > 0) lifeModifier = value; else lifeModifier = 1.0; }
        }

        [JsonInclude]
        public int BaseGold
        {
            get { return baseGold; }
            set { if (value >= 0) baseGold = value; else baseGold = 10; }
        }

        [JsonInclude]
        public double GoldModifier
        {
            get { return goldModifier; }
            set { if (value > 0) goldModifier = value; else goldModifier = 1.0; }
        }

        [JsonInclude]
        public double SpawnChance
        {
            get { return spawnChance; }
            set { if (value >= 0 && value <= 100) spawnChance = value; else spawnChance = 10.0; }
        }

        protected CEnemyTemplate()
        {
            name = "Unknown";
            iconName = "";
            baseLife = 100;
            lifeModifier = 1.0;
            baseGold = 10;
            goldModifier = 1.0;
            spawnChance = 10.0;
        }

        protected CEnemyTemplate(string name, string iconName, int baseLife, double lifeModifier,
                               int baseGold, double goldModifier, double spawnChance)
        {
            Name = name;
            IconName = iconName;
            BaseLife = baseLife;
            LifeModifier = lifeModifier;
            BaseGold = baseGold;
            GoldModifier = goldModifier;
            SpawnChance = spawnChance;
        }
    }
}
