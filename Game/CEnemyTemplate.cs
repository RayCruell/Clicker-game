using System.Text.Json.Serialization;

namespace Game
{
    public abstract class CEnemyTemplate
    {
        private string name;
        private string iconName;
        private int baseLife;
        private int lifeModifier;
        private int baseGold;
        private int goldModifier;
        private int spawnChance;

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
        public int LifeModifier
        {
            get { return lifeModifier; }
            set { if (value > 0) lifeModifier = value; else lifeModifier = 1; } // изменено на int
        }

        [JsonInclude]
        public int BaseGold
        {
            get { return baseGold; }
            set { if (value >= 0) baseGold = value; else baseGold = 10; }
        }

        [JsonInclude]
        public int GoldModifier
        {
            get { return goldModifier; }
            set { if (value > 0) goldModifier = value; else goldModifier = 1; } // изменено на int
        }

        [JsonInclude]
        public int SpawnChance
        {
            get { return spawnChance; }
            set { if (value >= 0 && value <= 100) spawnChance = value; else spawnChance = 10; } // изменено на int
        }

        protected CEnemyTemplate()
        {
            name = "Unknown";
            iconName = "";
            baseLife = 100;
            lifeModifier = 1; // int значение
            baseGold = 10;
            goldModifier = 1; // int значение
            spawnChance = 10; // int значение
        }

        protected CEnemyTemplate(string name, string iconName, int baseLife, int lifeModifier, // int параметры
                               int baseGold, int goldModifier, int spawnChance) // int параметры
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