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
using static System.Formats.Asn1.AsnWriter;

namespace Game
{
    public partial class MainWindow : Window
    {
        private EnemyTemplateList enemyList;      
        private List<CIcon> icons;                 
        private CIcon selectedIcon;               
        private string defaultIconsPath;           

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point mousePosition = Mouse.GetPosition(scene);
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

            SetupButtons();
            EnemiesListBox.SelectionChanged += EnemiesListBox_SelectionChanged;
        }

        private void SetupButtons()
        {
            foreach (var btn in FindChildren<Button>(this))
            {
                switch (btn.Content?.ToString())
                {
                    case "Add":
                        btn.Click += AddEnemy_Click;
                        break;
                    case "Remove":
                        btn.Click += RemoveEnemy_Click;
                        break;
                    case "Save":
                        btn.Click += SaveEnemies_Click;
                        break;
                    case "Load":
                        btn.Click += LoadEnemies_Click;
                        break;
                }
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

                if (string.IsNullOrWhiteSpace(enemyName) || selectedIcon == null)
                {
                    MessageBox.Show("Выберите иконку и введите имя врага!");
                    return;
                }

                int baseLife = int.Parse(BaseLifeBox.Text);
                double lifeModifier = double.Parse(LifeModifierBox.Text);
                int baseGold = int.Parse(BaseGoldBox.Text);
                double goldModifier = double.Parse(GoldModifierBox.Text);
                double spawnChance = double.Parse(SpawnChanceBox.Text);

                enemyList.AddEnemy(enemyName, iconName, baseLife, lifeModifier, baseGold, goldModifier, spawnChance);
                UpdateEnemyListBox();
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении врага: " + ex.Message);
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
            LifeModifierBox.Text = "";
            BaseGoldBox.Text = "";
            GoldModifierBox.Text = "";
            SpawnChanceBox.Text = "";
        }

        public static IEnumerable<T> FindChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null)
                yield break;

            foreach (var child in LogicalTreeHelper.GetChildren(depObj))
            {
                if (child is DependencyObject dep)
                {
                    if (child is T t)
                        yield return t;

                    foreach (var childOfChild in FindChildren<T>(dep))
                        yield return childOfChild;
                }
            }
        }

        private void EnemiesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EnemiesListBox.SelectedItem == null)
                return;

            string selectedEnemyName = EnemiesListBox.SelectedItem.ToString();
            var enemy = enemyList.FindByName(selectedEnemyName);
            if (enemy == null)
                return;

            EnemyNameBox.Text = enemy.Name;
            IconNameBox.Text = enemy.IconName;
            BaseLifeBox.Text = enemy.BaseLife.ToString();
            LifeModifierBox.Text = enemy.LifeModifier.ToString();
            BaseGoldBox.Text = enemy.BaseGold.ToString();
            GoldModifierBox.Text = enemy.GoldModifier.ToString();
            SpawnChanceBox.Text = enemy.SpawnChance.ToString();

            string iconPath = System.IO.Path.Combine(defaultIconsPath, enemy.IconName + ".png");
            if (File.Exists(iconPath))
            {
                SelectedEnemyIcon.Source = new BitmapImage(new Uri(iconPath, UriKind.Absolute));
            }
            else
            {
                SelectedEnemyIcon.Source = null; 
            }
        }
    }

    public class CIcon
    {
        public string Name { get; set; }
        public string ImagePath { get; set; }
    }
}
