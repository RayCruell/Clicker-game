using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Game
{
    public class EnemyTemplateList
    {
        [JsonInclude] private List<EnemyTemplate> enemies; 

        public EnemyTemplateList()
        {
            enemies = new List<EnemyTemplate>(); 
        }

        public void AddEnemy(string name, string icon, int baseLife, double lifeModifier,
                     int baseGold, double goldModifier, double spawnChance)
        {
            if (enemies.Any(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Враг с именем '{name}' уже существует!");
            }
            enemies.Add(new EnemyTemplate(name, icon, baseLife, lifeModifier, baseGold, goldModifier, spawnChance));
        }

        public EnemyTemplate GetEnemyByName(string name)
        {
            foreach (var enemy in enemies)
                if (enemy.Name == name) 
                    return enemy;
            return null;
        }

        public EnemyTemplate GetEnemyByIndex(int id)
        {
            if (id >= 0 && id < enemies.Count)
                return enemies[id];
            return null;
        }

        public void DeleteEnemyByName(string name)
        {
            enemies.RemoveAll(e => e.Name == name); 
        }

        public void DeleteEnemyByIndex(int id)
        {
            if (id >= 0 && id < enemies.Count)
                enemies.RemoveAt(id); 
        }

        public List<string> GetListOfEnemyNames()
        {
            List<string> names = new List<string>();
            foreach (var enemy in enemies)
                names.Add(enemy.Name);
            return names;
        }

        public void SaveToJson(string path)
        {
            string jsonString = JsonSerializer.Serialize(enemies, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, jsonString);
        }

        public void LoadFromJson(string path)
        {
            if (!File.Exists(path))
                return;

            string jsonFromFile = File.ReadAllText(path);
            List<EnemyTemplate> loadedEnemies = new List<EnemyTemplate>();

            JsonDocument doc = JsonDocument.Parse(jsonFromFile);
            foreach (JsonElement element in doc.RootElement.EnumerateArray())
            {
                string name = element.GetProperty("Name").GetString();
                string iconName = element.GetProperty("IconName").GetString();
                int baseLife = element.GetProperty("BaseLife").GetInt32();
                double lifeModifier = element.GetProperty("LifeModifier").GetDouble();
                int baseGold = element.GetProperty("BaseGold").GetInt32();
                double goldModifier = element.GetProperty("GoldModifier").GetDouble();
                double spawnChance = element.GetProperty("SpawnChance").GetDouble();

                EnemyTemplate enemy = new EnemyTemplate(name, iconName, baseLife, lifeModifier, baseGold, goldModifier, spawnChance);
                loadedEnemies.Add(enemy);
            }

            enemies = loadedEnemies; 
        }
        public EnemyTemplate FindByName(string name)
        {
            return enemies.FirstOrDefault(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

    }
}