using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace Game
{
    public class JsonEnemySaver : ISaveList<List<CEnemyTemplate>>
    {
        private readonly JsonSerializerOptions _options;

        public JsonEnemySaver()
        {
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                // Установка конвертера противников для реализации полиморфизма
                Converters = { new EnemyTemplateConverter() }
            };
        }

        // Реализация функции загрузки
        public List<CEnemyTemplate> Load(string path)
        {
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                // Десериализация с определением класса противника
                return JsonSerializer.Deserialize<List<CEnemyTemplate>>(json, _options) ?? new List<CEnemyTemplate>();
            }
            return new List<CEnemyTemplate>();
        }

        // Реализация функции сохранения
        public void Save(List<CEnemyTemplate> data, string path)
        {
            string json = JsonSerializer.Serialize(data, _options);
            File.WriteAllText(path, json);
        }
    }
}
