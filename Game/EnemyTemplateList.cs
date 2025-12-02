using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Game
{
    public class EnemyTemplateList
    {
        [JsonInclude]
        private List<CEnemyTemplate> enemies;
        private readonly ISaveList<List<CEnemyTemplate>> _serializer;

        public EnemyTemplateList()
        {
            enemies = new List<CEnemyTemplate>();
            _serializer = new JsonEnemySaver();
        }

        // Добавление обычного врага
        public void AddNormalEnemy(string name, string icon, int baseLife, double lifeModifier,
                                  int baseGold, double goldModifier, double spawnChance)
        {
            if (enemies.Any(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Враг с именем '{name}' уже существует!");

            enemies.Add(new CNormalEnemyTemplate(name, icon, baseLife, lifeModifier,
                                                baseGold, goldModifier, spawnChance));
        }

        // Добавление бронированного врага
        public void AddArmoredEnemy(string name, string icon, int baseLife, double lifeModifier,
                                   int baseGold, double goldModifier, double spawnChance, double armor)
        {
            if (enemies.Any(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Враг с именем '{name}' уже существует!");

            enemies.Add(new CArmoredEnemyTemplate(name, icon, baseLife, lifeModifier,
                                                 baseGold, goldModifier, spawnChance, armor));
        }

        // Добавление укорачивающегося врага
        public void AddShrinkingEnemy(string name, string icon, int baseLife, double lifeModifier,
                                     int baseGold, double goldModifier, double spawnChance, double shrinkFactor)
        {
            if (enemies.Any(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Враг с именем '{name}' уже существует!");

            enemies.Add(new CShrinkingEnemyTemplate(name, icon, baseLife, lifeModifier,
                                                   baseGold, goldModifier, spawnChance, shrinkFactor));
        }

        // Добавление исцеляющегося врага
        public void AddHealingEnemy(string name, string icon, int baseLife, double lifeModifier,
                                   int baseGold, double goldModifier, double spawnChance,
                                   double healChance, double healPercentage)
        {
            if (enemies.Any(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Враг с именем '{name}' уже существует!");

            enemies.Add(new CHealingEnemyTemplate(name, icon, baseLife, lifeModifier,
                                                 baseGold, goldModifier, spawnChance,
                                                 healChance, healPercentage));
        }

        // Общий метод добавления любого врага
        public void AddEnemy(CEnemyTemplate enemy)
        {
            if (enemies.Any(e => e.Name.Equals(enemy.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Враг с именем '{enemy.Name}' уже существует!");

            enemies.Add(enemy);
        }

        public CEnemyTemplate GetEnemyByName(string name)
        {
            return enemies.FirstOrDefault(e => e.Name == name);
        }

        public CEnemyTemplate GetEnemyByIndex(int id)
        {
            if (id >= 0 && id < enemies.Count)
                return enemies[id];
            return null;
        }

        public void DeleteEnemyByName(string name)
        {
            enemies.RemoveAll(e => e.Name == name);
        }

        public List<string> GetListOfEnemyNames()
        {
            return enemies.Select(e => e.Name).ToList();
        }

        public void SaveToJson(string path)
        {
            _serializer.Save(enemies, path);
        }

        public void LoadFromJson(string path)
        {
            enemies = _serializer.Load(path);
        }

        public CEnemyTemplate FindByName(string name)
        {
            return enemies.FirstOrDefault(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public List<CEnemyTemplate> GetAllEnemies()
        {
            return new List<CEnemyTemplate>(enemies);
        }
    }
}