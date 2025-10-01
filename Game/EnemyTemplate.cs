using System.Text.Json.Serialization;

namespace ClickerGame.Models
{
    public class EnemyTemplate
    {
        [JsonInclude] private string name;
        [JsonInclude] private string iconName;

        [JsonInclude] private int baseLife;
        [JsonInclude] private double lifeModifier;

        [JsonInclude] private int baseGold;
        [JsonInclude] private double doubleGoldModifier;
        [JsonInclude] private double goldModifier
        {
            get => doubleGoldModifier;
            set => doubleGoldModifier = value;
        }

        [JsonInclude] private double spawnChance;

        public EnemyTemplate(string name, string iconName,
                             int baseLife, double lifeModifier,
                             int baseGold, double goldModifier,
                             double spawnChance)
        {
            this.name = name;
            this.iconName = iconName;
            this.baseLife = baseLife;
            this.lifeModifier = lifeModifier;
            this.baseGold = baseGold;
            this.doubleGoldModifier = goldModifier;
            this.spawnChance = spawnChance;
        }

        public string Name => name;
        public string IconName => iconName;
        public int BaseLife => baseLife;
        public double LifeModifier => lifeModifier;
        public int BaseGold => baseGold;
        public double GoldModifier => doubleGoldModifier;
        public double SpawnChance => spawnChance;

        public double GetEffectiveLife() => baseLife * lifeModifier;
        public double GetEffectiveGold() => baseGold * doubleGoldModifier;
    }
}
