using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Game
{
    public partial class MainWindow : Window
    {
        private EnemyTemplateList enemyList;
        private List<CIcon> icons;
        private CIcon selectedIcon;
        private string defaultIconsPath;

        public MainWindow()
        {
            InitializeComponent();
            enemyList = new EnemyTemplateList();
            icons = new List<CIcon>();

            defaultIconsPath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName),
                "Images", "Monsters"
            );

            if (Directory.Exists(defaultIconsPath))
                LoadIcons(defaultIconsPath);
            else
                MessageBox.Show("Папка с иконками не найдена:\n" + defaultIconsPath);

            // Выбираем первый тип врага по умолчанию
            EnemyTypeComboBox.SelectedIndex = 0;
            EnemyTypeComboBox_SelectionChanged(null, null); // Инициализируем панель

            EnemiesListBox.SelectionChanged += EnemiesListBox_SelectionChanged;
        }

        // Обработчик изменения типа врага
        private void EnemyTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EnemyTypeComboBox.SelectedItem == null) return;

            var selectedItem = (ComboBoxItem)EnemyTypeComboBox.SelectedItem;
            string enemyType = selectedItem.Tag.ToString();

            switch (enemyType)
            {
                case "Armored":
                    TypeSpecificPanel.Visibility = Visibility.Visible;
                    TypeParamLabel.Text = "Armor value:";
                    TypeParamBox.Text = "25";
                    TypeParamBox.ToolTip = "Значение брони (например, 25)";
                    break;

                case "Shrinking":
                    TypeSpecificPanel.Visibility = Visibility.Visible;
                    TypeParamLabel.Text = "Shrink factor:";
                    TypeParamBox.Text = "0.7";
                    TypeParamBox.ToolTip = "Коэффициент уменьшения (0.1-0.9)";
                    break;

                case "Healing":
                    TypeSpecificPanel.Visibility = Visibility.Visible;
                    TypeParamLabel.Text = "Heal chance %:";
                    TypeParamBox.Text = "25,30";
                    TypeParamBox.ToolTip = "Шанс лечения (1-100) и процент лечения через запятую (например: 25,30)";
                    break;

                default: // Normal
                    TypeSpecificPanel.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void LoadIcons(string path)
        {
            try
            {
                icons.Clear();

                foreach (string file in Directory.GetFiles(path, "*.png"))
                {
                    icons.Add(new CIcon
                    {
                        Name = System.IO.Path.GetFileNameWithoutExtension(file),
                        ImagePath = file
                    });
                }

                IconsPanel.ItemsSource = icons;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке иконок: " + ex.Message);
            }
        }

        private void IconButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is CIcon icon)
            {
                selectedIcon = icon;
                SelectedEnemyIcon.Source = new BitmapImage(new Uri(icon.ImagePath, UriKind.Absolute));
                IconNameBox.Text = icon.Name;
            }
        }

        private void AddEnemy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string enemyName = EnemyNameBox.Text.Trim();
                string iconName = IconNameBox.Text.Trim();
                string enemyType = ((ComboBoxItem)EnemyTypeComboBox.SelectedItem).Tag.ToString();

                if (string.IsNullOrWhiteSpace(enemyName))
                {
                    MessageBox.Show("Введите имя врага!");
                    return;
                }

                if (string.IsNullOrWhiteSpace(iconName) || selectedIcon == null)
                {
                    MessageBox.Show("Выберите иконку врага!");
                    return;
                }

                // Используем CultureInfo.InvariantCulture для парсинга с точкой
                // Или CultureInfo.CurrentCulture для запятой (русская локаль)
                var culture = System.Globalization.CultureInfo.CurrentCulture; // Для запятой

                // Проверяем и парсим основные параметры
                if (!int.TryParse(BaseLifeBox.Text, out int baseLife))
                {
                    MessageBox.Show("Некорректное значение здоровья!");
                    BaseLifeBox.Focus();
                    return;
                }

                // Для дробных чисел используем правильный парсинг
                if (!double.TryParse(LifeModifierBox.Text.Replace('.', ','), System.Globalization.NumberStyles.Any,
                                    System.Globalization.CultureInfo.InvariantCulture, out double lifeModifier))
                {
                    MessageBox.Show($"Некорректный модификатор здоровья! Введите число (например: 1,5 или 1.5)");
                    LifeModifierBox.Focus();
                    return;
                }

                if (!int.TryParse(BaseGoldBox.Text, out int baseGold))
                {
                    MessageBox.Show("Некорректное значение золота!");
                    BaseGoldBox.Focus();
                    return;
                }

                if (!double.TryParse(GoldModifierBox.Text.Replace('.', ','), System.Globalization.NumberStyles.Any,
                                    System.Globalization.CultureInfo.InvariantCulture, out double goldModifier))
                {
                    MessageBox.Show($"Некорректный модификатор золота! Введите число (например: 1,2 или 1.2)");
                    GoldModifierBox.Focus();
                    return;
                }

                if (!double.TryParse(SpawnChanceBox.Text.Replace('.', ','), System.Globalization.NumberStyles.Any,
                                    System.Globalization.CultureInfo.InvariantCulture, out double spawnChance))
                {
                    MessageBox.Show($"Некорректный шанс появления! Введите число (например: 10,5 или 10.5)");
                    SpawnChanceBox.Focus();
                    return;
                }

                // Проверяем диапазоны
                if (baseLife <= 0)
                {
                    MessageBox.Show("Здоровье должно быть больше 0!");
                    BaseLifeBox.Focus();
                    return;
                }

                if (spawnChance < 0 || spawnChance > 100)
                {
                    MessageBox.Show("Шанс появления должен быть от 0 до 100!");
                    SpawnChanceBox.Focus();
                    return;
                }

                // Создаем врага в зависимости от типа
                switch (enemyType)
                {
                    case "Normal":
                        enemyList.AddNormalEnemy(enemyName, iconName, baseLife, lifeModifier,
                                                baseGold, goldModifier, spawnChance);
                        break;

                    case "Armored":
                        if (!double.TryParse(TypeParamBox.Text.Replace('.', ','), System.Globalization.NumberStyles.Any,
                                            System.Globalization.CultureInfo.InvariantCulture, out double armor))
                        {
                            MessageBox.Show($"Некорректное значение брони! Введите число (например: 25,5 или 25.5)");
                            TypeParamBox.Focus();
                            return;
                        }
                        enemyList.AddArmoredEnemy(enemyName, iconName, baseLife, lifeModifier,
                                                 baseGold, goldModifier, spawnChance, armor);
                        break;

                    case "Shrinking":
                        if (!double.TryParse(TypeParamBox.Text.Replace('.', ','), System.Globalization.NumberStyles.Any,
                                            System.Globalization.CultureInfo.InvariantCulture, out double shrinkFactor))
                        {
                            MessageBox.Show($"Некорректный коэффициент уменьшения! Введите число (например: 0,7 или 0.7)");
                            TypeParamBox.Focus();
                            return;
                        }
                        if (shrinkFactor <= 0 || shrinkFactor >= 1)
                        {
                            MessageBox.Show("Коэффициент уменьшения должен быть между 0 и 1!");
                            TypeParamBox.Focus();
                            return;
                        }
                        enemyList.AddShrinkingEnemy(enemyName, iconName, baseLife, lifeModifier,
                                                   baseGold, goldModifier, spawnChance, shrinkFactor);
                        break;

                    case "Healing":
                        string[] healParams = TypeParamBox.Text.Split(',');

                        // Первый параметр - шанс лечения
                        if (!double.TryParse(healParams[0].Trim().Replace('.', ','), System.Globalization.NumberStyles.Any,
                                            System.Globalization.CultureInfo.InvariantCulture, out double healChance))
                        {
                            MessageBox.Show($"Некорректный шанс лечения! Введите число (например: 25 или 25,5)");
                            TypeParamBox.Focus();
                            return;
                        }

                        // Второй параметр - процент лечения (опциональный)
                        double healPercentage = 30;
                        if (healParams.Length > 1)
                        {
                            if (!double.TryParse(healParams[1].Trim().Replace('.', ','), System.Globalization.NumberStyles.Any,
                                                System.Globalization.CultureInfo.InvariantCulture, out healPercentage))
                            {
                                MessageBox.Show($"Некорректный процент лечения! Введите число (например: 30 или 30,5)");
                                TypeParamBox.Focus();
                                return;
                            }
                        }

                        if (healChance < 0 || healChance > 100)
                        {
                            MessageBox.Show("Шанс лечения должен быть от 0 до 100!");
                            TypeParamBox.Focus();
                            return;
                        }

                        enemyList.AddHealingEnemy(enemyName, iconName, baseLife, lifeModifier,
                                                 baseGold, goldModifier, spawnChance, healChance, healPercentage);
                        break;
                }

                UpdateEnemyListBox();
                ClearFields();
                MessageBox.Show($"Враг '{enemyName}' добавлен как тип '{enemyType}'");
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is FormatException)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неожиданная ошибка: {ex.Message}");
            }
        }
        // Вспомогательные методы для добавления новых типов врагов
        private void AddShrinkingEnemy(string name, string icon, int baseLife, double lifeModifier,
                                      int baseGold, double goldModifier, double spawnChance, double shrinkFactor)
        {
            if (enemyList.GetListOfEnemyNames().Any(n => n.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Враг с именем '{name}' уже существует!");
            }

            // Временное решение - используем рефлексию для доступа к приватному списку
            var enemy = new CShrinkingEnemyTemplate(name, icon, baseLife, lifeModifier,
                                                   baseGold, goldModifier, spawnChance, shrinkFactor);

            // Получаем доступ к приватному списку через рефлексию
            var enemiesField = typeof(EnemyTemplateList).GetField("enemies", BindingFlags.NonPublic | BindingFlags.Instance);
            if (enemiesField != null)
            {
                var enemiesList = (List<CEnemyTemplate>)enemiesField.GetValue(enemyList);
                enemiesList.Add(enemy);
            }
        }

        private void AddHealingEnemy(string name, string icon, int baseLife, double lifeModifier,
                                    int baseGold, double goldModifier, double spawnChance,
                                    double healChance, double healPercentage)
        {
            if (enemyList.GetListOfEnemyNames().Any(n => n.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Враг с именем '{name}' уже существует!");
            }

            var enemy = new CHealingEnemyTemplate(name, icon, baseLife, lifeModifier,
                                                 baseGold, goldModifier, spawnChance,
                                                 healChance, healPercentage);

            // Получаем доступ к приватному списку через рефлексию
            var enemiesField = typeof(EnemyTemplateList).GetField("enemies", BindingFlags.NonPublic | BindingFlags.Instance);
            if (enemiesField != null)
            {
                var enemiesList = (List<CEnemyTemplate>)enemiesField.GetValue(enemyList);
                enemiesList.Add(enemy);
            }
        }

        private void RemoveEnemy_Click(object sender, RoutedEventArgs e)
        {
            if (EnemiesListBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите врага для удаления.");
                return;
            }

            string name = EnemiesListBox.SelectedItem.ToString();
            enemyList.DeleteEnemyByName(name);
            UpdateEnemyListBox();
        }

        private void SaveEnemies_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                FileName = "Enemies",
                DefaultExt = ".json",
                Filter = "JSON files (*.json)|*.json"
            };

            if (dlg.ShowDialog() == true)
            {
                enemyList.SaveToJson(dlg.FileName);
                MessageBox.Show("Список врагов сохранён успешно.");
            }
        }

        private void LoadEnemies_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                DefaultExt = ".json",
                Filter = "JSON files (*.json)|*.json"
            };

            if (dlg.ShowDialog() == true)
            {
                enemyList.LoadFromJson(dlg.FileName);
                UpdateEnemyListBox();
                MessageBox.Show("Список врагов загружен успешно.");
            }
        }

        private void UpdateEnemyListBox()
        {
            EnemiesListBox.ItemsSource = null;
            EnemiesListBox.ItemsSource = enemyList.GetListOfEnemyNames();
        }

        private void ClearFields()
        {
            EnemyNameBox.Text = "";
            BaseLifeBox.Text = "";
            LifeModifierBox.Text = ""; // Запятая вместо точки!
            BaseGoldBox.Text = "";
            GoldModifierBox.Text = ""; // Запятая вместо точки!
            SpawnChanceBox.Text = ""; // Запятая вместо точки!
            TypeParamBox.Text = "";
            EnemyTypeComboBox.SelectedIndex = 0;
        }

        private void EnemiesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EnemiesListBox.SelectedItem == null) return;

            string selectedEnemyName = EnemiesListBox.SelectedItem.ToString();
            var enemy = enemyList.FindByName(selectedEnemyName);
            if (enemy == null) return;

            EnemyNameBox.Text = enemy.Name;
            IconNameBox.Text = enemy.IconName;
            BaseLifeBox.Text = enemy.BaseLife.ToString();
            LifeModifierBox.Text = enemy.LifeModifier.ToString();
            BaseGoldBox.Text = enemy.BaseGold.ToString();
            GoldModifierBox.Text = enemy.GoldModifier.ToString();
            SpawnChanceBox.Text = enemy.SpawnChance.ToString();

            // Определяем тип врага и показываем специальные параметры
            if (enemy is CArmoredEnemyTemplate armored)
            {
                EnemyTypeComboBox.SelectedIndex = 1; // Armored
                TypeParamBox.Text = armored.Armor.ToString();
                TypeSpecificPanel.Visibility = Visibility.Visible;
            }
            else if (enemy is CShrinkingEnemyTemplate shrinking)
            {
                EnemyTypeComboBox.SelectedIndex = 2; // Shrinking
                TypeParamBox.Text = shrinking.ShrinkFactor.ToString();
                TypeSpecificPanel.Visibility = Visibility.Visible;
            }
            else if (enemy is CHealingEnemyTemplate healing)
            {
                EnemyTypeComboBox.SelectedIndex = 3; // Healing
                TypeParamBox.Text = $"{healing.HealChance},{healing.HealPercentage}";
                TypeSpecificPanel.Visibility = Visibility.Visible;
            }
            else // Normal
            {
                EnemyTypeComboBox.SelectedIndex = 0;
                TypeSpecificPanel.Visibility = Visibility.Collapsed;
            }

            string iconPath = System.IO.Path.Combine(defaultIconsPath, enemy.IconName + ".png");
            if (File.Exists(iconPath))
                SelectedEnemyIcon.Source = new BitmapImage(new Uri(iconPath, UriKind.Absolute));
            else
                SelectedEnemyIcon.Source = null;
        }

        private void StartBattleButton_Click(object sender, RoutedEventArgs e)
        {
            if (enemyList.GetListOfEnemyNames().Count == 0)
            {
                MessageBox.Show("Сначала нужно добавить или загрузить врагов!");
                return;
            }

            CPlayer player = new CPlayer();
            IconList iconList = BuildIconList();

            BattleWindow battleWindow = new BattleWindow(player, iconList, enemyList);
            battleWindow.Owner = this;
            battleWindow.Show();
        }

        private IconList BuildIconList()
        {
            IconList iconList = new IconList(64, 64, 800, 600);
            foreach (var cicon in icons)
            {
                CIcon newIcon = new CIcon(64, 64, cicon.ImagePath);
                iconList.GetIcons().Add(newIcon);
                Debug.WriteLine($"Добавлена иконка в iconList: {newIcon.GetName()} -> {newIcon.ImagePath}");
            }
            return iconList;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}