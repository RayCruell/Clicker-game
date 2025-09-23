using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ClickerGame.Models
{
    public class EnemyTemplateList
    {
        private readonly List<EnemyTemplate> enemies = new List<EnemyTemplate>();

        public IReadOnlyList<EnemyTemplate> Enemies => enemies.AsReadOnly();

        public void Add(EnemyTemplate e) => enemies.Add(e);
        public bool Remove(EnemyTemplate e) => enemies.Remove(e);
        public void Clear() => enemies.Clear();

        public void SaveToFile(string filePath)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(enemies, options);
            File.WriteAllText(filePath, json);
        }

        public void LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);
            enemies.Clear();
            string json = File.ReadAllText(filePath);
            var docs = JsonDocument.Parse(json);
            foreach (var el in docs.RootElement.EnumerateArray())
            {
                try
                {
                    string name = el.GetProperty("name").GetString();
                    string iconName = el.GetProperty("iconName").GetString();
                    int baseLife = el.GetProperty("baseLife").GetInt32();
                    double lifeModifier = el.GetProperty("lifeModifier").GetDouble();
                    int baseGold = el.GetProperty("baseGold").GetInt32();
                    double goldModifier = el.GetProperty("goldModifier").GetDouble();
                    double spawnChance = el.GetProperty("spawnChance").GetDouble();

                    var enemy = new EnemyTemplate(name, iconName, baseLife, lifeModifier, baseGold, goldModifier, spawnChance);
                    enemies.Add(enemy);
                }
                catch { continue; }
            }
        }
    }
}
