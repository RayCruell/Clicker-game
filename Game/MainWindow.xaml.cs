using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            UpdateTypeSpecificPanel("Normal"); // Инициализируем панель

            // Подписываемся на событие изменения типа
            EnemyTypeComboBox.SelectionChanged += EnemyTypeComboBox_SelectionChanged;
            EnemiesListBox.SelectionChanged += EnemiesListBox_SelectionChanged;

            // Устанавливаем значения по умолчанию
            SetDefaultValues();
        }

        private void SetDefaultValues()
        {
            BaseLifeBox.Text = "";
            LifeModifierBox.Text = "";
            BaseGoldBox.Text = "";
            GoldModifierBox.Text = "";
            SpawnChanceBox.Text = "";
        }

        // Обработчик изменения типа врага
        private void EnemyTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EnemyTypeComboBox.SelectedItem == null) return;

            var selectedItem = (ComboBoxItem)EnemyTypeComboBox.SelectedItem;
            string enemyType = selectedItem.Tag.ToString();
            UpdateTypeSpecificPanel(enemyType);
        }

        private void UpdateTypeSpecificPanel(string enemyType)
        {
            switch (enemyType)
            {
                case "Armored":
                    TypeSpecificPanel.Visibility = Visibility.Visible;
                    TypeParamLabel.Text = "Броня:";
                    TypeParamBox.Text = "25";
                    TypeParamBox.ToolTip = "Значение брони (например: 25)";
                    break;

                case "Dodging":
                    TypeSpecificPanel.Visibility = Visibility.Visible;
                    TypeParamLabel.Text = "Шанс уворота %:";
                    TypeParamBox.Text = "25";
                    TypeParamBox.ToolTip = "Шанс уворота (1-100)";
                    break;

                case "Healing":
                    TypeSpecificPanel.Visibility = Visibility.Visible;
                    TypeParamLabel.Text = "Лечение (шанс,процент):";
                    TypeParamBox.Text = "25,30";
                    TypeParamBox.ToolTip = "Шанс лечения и процент через запятую (например: 25,50)";
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

                if (EnemyTypeComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите тип врага!");
                    return;
                }

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

                // Парсим основные параметры
                if (!int.TryParse(BaseLifeBox.Text, out int baseLife))
                {
                    MessageBox.Show("Некорректное значение здоровья!");
                    BaseLifeBox.Focus();
                    return;
                }

                if (!int.TryParse(LifeModifierBox.Text, out int lifeModifier))
                {
                    MessageBox.Show("Модификатор здоровья должен быть целым числом!");
                    LifeModifierBox.Focus();
                    return;
                }

                if (!int.TryParse(BaseGoldBox.Text, out int baseGold))
                {
                    MessageBox.Show("Некорректное значение золота!");
                    BaseGoldBox.Focus();
                    return;
                }

                if (!int.TryParse(GoldModifierBox.Text, out int goldModifier))
                {
                    MessageBox.Show("Модификатор золота должен быть целым числом!");
                    GoldModifierBox.Focus();
                    return;
                }

                if (!int.TryParse(SpawnChanceBox.Text, out int spawnChance))
                {
                    MessageBox.Show("Шанс появления должен быть целым числом!");
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
                            MessageBox.Show("Некорректное значение брони!");
                            TypeParamBox.Focus();
                            return;
                        }
                        enemyList.AddArmoredEnemy(enemyName, iconName, baseLife, lifeModifier,
                                                 baseGold, goldModifier, spawnChance, armor);
                        break;

                    case "Dodging":
                        if (!double.TryParse(TypeParamBox.Text.Replace('.', ','), System.Globalization.NumberStyles.Any,
                                            System.Globalization.CultureInfo.InvariantCulture, out double dodgeChance))
                        {
                            MessageBox.Show("Некорректный шанс уворота!");
                            TypeParamBox.Focus();
                            return;
                        }
                        if (dodgeChance < 0 || dodgeChance > 100)
                        {
                            MessageBox.Show("Шанс уворота должен быть от 0 до 100!");
                            TypeParamBox.Focus();
                            return;
                        }
                        enemyList.AddDodgingEnemy(enemyName, iconName, baseLife, lifeModifier,
                                                 baseGold, goldModifier, spawnChance, dodgeChance);
                        break;

                    case "Healing":
                        string[] healParams = TypeParamBox.Text.Split(',');

                        // Парсим шанс лечения (первое число)
                        if (!int.TryParse(healParams[0].Trim(), out int healChance))
                        {
                            MessageBox.Show("Некорректный шанс лечения! Введите число (например: 25)");
                            TypeParamBox.Focus();
                            return;
                        }

                        // Парсим процент лечения (второе число, по умолчанию 30)
                        int healPercentage = 30;
                        if (healParams.Length > 1)
                        {
                            if (!int.TryParse(healParams[1].Trim(), out healPercentage))
                            {
                                MessageBox.Show("Некорректный процент лечения! Введите число (например: 30)");
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

                        if (healPercentage < 0 || healPercentage > 100)
                        {
                            MessageBox.Show("Процент лечения должен быть от 0 до 100!");
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
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неожиданная ошибка: {ex.Message}");
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
            IconNameBox.Text = "";
            SelectedEnemyIcon.Source = null;
            selectedIcon = null;
            SetDefaultValues();
            EnemyTypeComboBox.SelectedIndex = 0;
            TypeParamBox.Text = "";
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
            LifeModifierBox.Text = enemy.LifeModifier.ToString(); // Теперь int, нет дробной части
            BaseGoldBox.Text = enemy.BaseGold.ToString();
            GoldModifierBox.Text = enemy.GoldModifier.ToString(); // Теперь int
            SpawnChanceBox.Text = enemy.SpawnChance.ToString();   // Теперь int

            // Определяем тип врага и показываем специальные параметры
            if (enemy is CArmoredEnemyTemplate armored)
            {
                EnemyTypeComboBox.SelectedIndex = 1; // Armored
                TypeParamBox.Text = armored.Armor.ToString();
                TypeSpecificPanel.Visibility = Visibility.Visible;
            }
            else if (enemy is CDodgingEnemyTemplate dodging) // Было: CShrinkingEnemyTemplate
            {
                EnemyTypeComboBox.SelectedIndex = 2; // Уворачивающийся
                TypeParamBox.Text = dodging.DodgeChance.ToString();
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

            // Загружаем иконку
            string iconPath = System.IO.Path.Combine(defaultIconsPath, enemy.IconName + ".png");
            if (File.Exists(iconPath))
            {
                SelectedEnemyIcon.Source = new BitmapImage(new Uri(iconPath, UriKind.Absolute));
                // Находим соответствующую иконку в списке
                selectedIcon = icons.FirstOrDefault(i => i.Name == enemy.IconName);
            }
            else
            {
                SelectedEnemyIcon.Source = null;
                selectedIcon = null;
            }
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
            }
            return iconList;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Обработка клика мыши в окне
        }

        private void EnemyNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}