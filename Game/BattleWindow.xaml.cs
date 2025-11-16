using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Policy;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Game
{
    public partial class BattleWindow : Window
    {
        private CPlayer player;
        private CEnemy currentEnemy;
        private EnemyTemplateList templates;
        private IconList iconList;
        private Random rng = new Random();

        public List<CEnemy> enemies = new List<CEnemy>(); // Вместо двух списков у нас теперь только один, больше не реализуем два списка по именам, где были побеждённые и доступные враги

        public BattleWindow(CPlayer player, IconList iconList, EnemyTemplateList enemyTemplates)
        {
            InitializeComponent();
            this.player = player ?? throw new ArgumentNullException(nameof(player));
            this.iconList = iconList ?? throw new ArgumentNullException(nameof(iconList));
            this.templates = enemyTemplates ?? throw new ArgumentNullException(nameof(enemyTemplates));

            enemies = CreateAllEnemies();

            NextEnemy();
        }

        private List<CEnemy> CreateAllEnemies()
        {
            var allEnemies = new List<CEnemy>();
            var enemyNames = templates.GetListOfEnemyNames();

            foreach (var enemyName in enemyNames)
            {
                var template = templates.GetEnemyByName(enemyName);
                if (template != null)
                {
                    CIcon enemyIcon = null;
                    if (iconList != null)
                    {
                        var ic = iconList.FindByName(template.IconName);
                        if (ic != null && !string.IsNullOrEmpty(ic.ImagePath))
                        {
                            enemyIcon = new CIcon(ic.GetIconWidth(), ic.GetIconHeight(), ic.ImagePath);
                        }
                    }

                    var enemy = new CEnemy(
                        template.Name,
                        new BigNumber(template.BaseLife.ToString()),
                        new BigNumber(template.BaseGold.ToString()),
                        enemyIcon
                    );
                    allEnemies.Add(enemy);
                }
            }
            return allEnemies;
        }

        private CEnemy GetRandomEnemyWithChances()
        {
            if (enemies.Count == 0) return null;
            double totalChance = 0;
            foreach (var enemy in enemies)
            {
                var template = templates.GetEnemyByName(enemy.Name);
                if (template != null)
                {
                    totalChance += template.SpawnChance;
                }
            }

            if (totalChance == 0) return enemies[0];

            double randomValue = rng.NextDouble() * totalChance;

            double currentSum = 0;
            foreach (var enemy in enemies)
            {
                var template = templates.GetEnemyByName(enemy.Name);
                if (template != null)
                {
                    currentSum += template.SpawnChance;
                    if (randomValue <= currentSum)
                    {
                        return enemy;
                    }
                }
            }

            return enemies[0];
        }

        private void UpdateUI()
        {
            if (currentEnemy != null)
            {
                EnemyNameText.Text = currentEnemy.Name;
                EnemyHPText.Text = $"{currentEnemy.CurrentHitPoints} / {currentEnemy.MaxHitPoints}";
                EnemyGoldText.Text = currentEnemy.GoldReward.ToString();

                if (currentEnemy.Icon != null && File.Exists((string)currentEnemy.Icon.ImagePath))
                {
                    try
                    {
                        EnemyIcon.Source = new BitmapImage(new Uri((string)currentEnemy.Icon.ImagePath, UriKind.Absolute));
                    }
                    catch
                    {
                        EnemyIcon.Source = null;
                    }
                }
                else
                {
                    EnemyIcon.Source = null;
                }
            }

            PlayerLevelText.Text = player.Lvl.ToString();
            PlayerGoldText.Text = player.Gold.ToString();
            PlayerDamageText.Text = player.Damage.ToString();
        }

        private void UpgradeButton_Click(object sender, RoutedEventArgs e)
        {
            if (player.TryUpgrade())
            {
                MessageBox.Show("Успешный апдейт!", "Апдейт", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Нужно больше золота!", "Апдейт", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            UpdateUI();
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e) //Кнопка повторения списка
        {
            enemies = CreateAllEnemies();
            NextEnemy();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e) //Кнопка следующего врага
        {
            NextEnemy();
        }

        private void RemoveDefeatedEnemy(string enemyName)
        {
            var defeatedEnemy = enemies.Find(e => e.Name == enemyName);
            if (defeatedEnemy != null)
            {
                enemies.Remove(defeatedEnemy);
            }
        }

        private void NextEnemy() // Обработка следующего врага
        {
            if (templates == null || templates.GetListOfEnemyNames().Count == 0)
            {
                ClearBattleInterface();
                return;
            }

            currentEnemy = GetRandomEnemyWithChances();

            if (currentEnemy != null)
            {
                UpdateUI();
            }
            else
            {
                ClearBattleInterface();
            }
        }

        private void EnemyIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (currentEnemy == null) return;

            var dmg = player.DealDamage();
            if (currentEnemy.TakeDamage(dmg, out BigNumber reward))
            {
                RemoveDefeatedEnemy(currentEnemy.Name);

                player.AddGold(reward);
                MessageBox.Show($"Враг убит! Держи {reward} золота!", "Успех!");

                if (enemies.Count == 0)
                {
                    ClearBattleInterface();
                    MessageBox.Show("Все враги побеждены! Нажмите 'Repeat' для новой битвы.", "Победа!");
                }
                else
                {
                    NextEnemy();
                }
            }

            UpdateUI();
        }

        private void ClearBattleInterface() //Убираем после пройденного списка всех врагов
        {
            currentEnemy = null;
            EnemyNameText.Text = "";
            EnemyHPText.Text = "";
            EnemyGoldText.Text = "";
            EnemyIcon.Source = null;
        }
    }
}
