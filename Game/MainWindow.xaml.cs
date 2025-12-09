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
        public delegate void EditorEventHandler(object sender, EditorEventArgs e);

        // СОБЫТИЯ редактора
        public event EditorEventHandler IconSelected;      // Выбрана иконка
        public event EditorEventHandler EnemyAdded;        // Добавлен враг
        public event EditorEventHandler EnemyRemoved;      // Удалён враг
        public event EditorEventHandler EnemySelected;     // Выбран враг из списка
        public event EditorEventHandler ListSaved;         // Список сохранён
        public event EditorEventHandler ListLoaded;        // Список загружен

        private EnemyTemplateList enemyList;
        private List<CIcon> icons;
        private CIcon selectedIcon;
        private string defaultIconsPath;

        private void RaiseEditorEvent(EditorEventHandler handler, string message, object data = null)
        {
            handler?.Invoke(this, new EditorEventArgs(message, data));
        }

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

            EnemyTypeComboBox.SelectedIndex = 0;
            UpdateTypeSpecificPanel("Normal");

            EnemyTypeComboBox.SelectionChanged += EnemyTypeComboBox_SelectionChanged;
            EnemiesListBox.SelectionChanged += EnemiesListBox_SelectionChanged;

            SetDefaultValues();

            // ПОДПИСКА НА СОБЫТИЯ РЕДАКТОРА
            IconSelected += OnEditorEvent;
            EnemyAdded += OnEditorEvent;
            EnemyRemoved += OnEditorEvent;
            EnemySelected += OnEditorEvent;
            ListSaved += OnEditorEvent;
            ListLoaded += OnEditorEvent;

            // Добавляем стартовое сообщение в лог
            AddEventLog("Редактор врагов запущен");
        }

        // Вспомогательный метод для добавления сообщений в лог
        private void AddEventLog(string message)
        {
            string logEntry = $"[{DateTime.Now:HH:mm:ss}] {message}";
            EventLogListBox.Items.Insert(0, logEntry);

            if (EventLogListBox.Items.Count > 15)
                EventLogListBox.Items.RemoveAt(15);
        }

        // ОБРАБОТЧИК СОБЫТИЙ РЕДАКТОРА
        private void OnEditorEvent(object sender, EditorEventArgs e)
        {
            // Формируем запись лога
            string logEntry = $"[{e.Timestamp:HH:mm:ss}] {e.Message}";

            // Добавляем в ListBox
            Dispatcher.Invoke(() =>
            {
                EventLogListBox.Items.Insert(0, logEntry);

                // Ограничиваем 15 сообщениями
                if (EventLogListBox.Items.Count > 15)
                    EventLogListBox.Items.RemoveAt(15);
            });

            // Также выводим в консоль для отладки
            Console.WriteLine(logEntry);
        }

        private void SetDefaultValues()
        {
            BaseLifeBox.Text = "100";
            LifeModifierBox.Text = "1";
            BaseGoldBox.Text = "10";
            GoldModifierBox.Text = "1";
            SpawnChanceBox.Text = "10";
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

                // ГЕНЕРИРУЕМ СОБЫТИЕ
                RaiseEditorEvent(IconSelected, $"Выбрана иконка: {icon.Name}", icon);
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

                // ГЕНЕРИРУЕМ СОБЫТИЕ ПОСЛЕ УСПЕШНОГО ДОБАВЛЕНИЯ
                var addedEnemy = enemyList.FindByName(enemyName);
                RaiseEditorEvent(EnemyAdded, $"Добавлен враг: {enemyName} (тип: {enemyType})", addedEnemy);
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

            // ГЕНЕРИРУЕМ СОБЫТИЕ
            RaiseEditorEvent(EnemyRemoved, $"Удалён враг: {name}", name);
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

                // ГЕНЕРИРУЕМ СОБЫТИЕ
                RaiseEditorEvent(ListSaved, $"Список сохранён в: {dlg.FileName}", dlg.FileName);
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

                // ГЕНЕРИРУЕМ СОБЫТИЕ
                RaiseEditorEvent(ListLoaded, $"Список загружен из: {dlg.FileName}", dlg.FileName);
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
            else if (enemy is CDodgingEnemyTemplate dodging)
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

            // ГЕНЕРИРУЕМ СОБЫТИЕ ВЫБОРА ВРАГА
            RaiseEditorEvent(EnemySelected, $"Выбран враг: {enemy.Name}", enemy);
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
            // Обработчик изменения текста в поле имени врага
        }
        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            EventLogListBox.Items.Clear();
            AddEventLog("Лог событий очищен");
        }
    }
}