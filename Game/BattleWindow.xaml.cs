using System;
using System.Collections.Generic;
using System.IO;
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

        public BattleWindow(CPlayer player, IconList iconList, EnemyTemplateList enemyTemplates)
        {
            InitializeComponent();
            this.player = player ?? throw new ArgumentNullException(nameof(player));
            this.iconList = iconList ?? throw new ArgumentNullException(nameof(iconList));
            this.templates = enemyTemplates ?? throw new ArgumentNullException(nameof(enemyTemplates));

            NextEnemy(); // загружаем первого врага
        }

        private void UpdateUI()
        {
            // Враг
            if (currentEnemy != null)
            {
                EnemyNameText.Text = currentEnemy.Name;
                EnemyHPText.Text = $"{currentEnemy.CurrentHitPoints} / {currentEnemy.MaxHitPoints}";
                EnemyGoldText.Text = currentEnemy.GoldReward.ToString();

                if (currentEnemy.Icon != null && File.Exists(currentEnemy.Icon.ImagePath))
                {
                    try
                    {
                        EnemyIcon.Source = new BitmapImage(new Uri(currentEnemy.Icon.ImagePath, UriKind.Absolute));
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

            // Игрок
            PlayerLevelText.Text = player.Lvl.ToString();
            PlayerGoldText.Text = player.Gold.ToString();
            PlayerDamageText.Text = player.Damage.ToString();
        }

        // Клик по иконке врага
        private void EnemyIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (currentEnemy == null) return;

            var dmg = player.DealDamage();
            if (currentEnemy.TakeDamage(dmg, out BigNumber reward))
            {
                // Враг убит
                player.AddGold(reward);
                MessageBox.Show($"Enemy defeated! You got {reward} gold!", "Victory", MessageBoxButton.OK, MessageBoxImage.Information);
                NextEnemy();
            }

            UpdateUI();
        }

        private void UpgradeButton_Click(object sender, RoutedEventArgs e)
        {
            if (player.TryUpgrade())
            {
                MessageBox.Show("Upgrade successful!", "Upgrade", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Not enough gold!", "Upgrade", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            UpdateUI();
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentEnemy == null) return;

            // Клонируем текущего врага
            currentEnemy = currentEnemy.Clone();
            UpdateUI();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            NextEnemy();
        }

        private void NextEnemy()
        {
            if (templates == null || templates.GetListOfEnemyNames().Count == 0) return;

            CEnemy enemy = null;
            int attempts = 0;

            while (enemy == null && attempts < 100)
            {
                attempts++;
                var namesList = templates.GetListOfEnemyNames();
                var randomName = namesList[rng.Next(namesList.Count)];
                var template = templates.GetEnemyByName(randomName);
                if (template == null) continue;

                double roll = rng.NextDouble() * 100;
                if (roll <= template.SpawnChance)
                {
                    Icon enemyIcon = null;

                    if (iconList != null)
                    {
                        var ic = iconList.FindByName(template.IconName);
                        if (ic != null)
                            enemyIcon = new Icon(ic.GetIconWidth(), ic.GetIconHeight(), ic.ImagePath);
                    }

                    enemy = new CEnemy(
                        template.Name,
                        new BigNumber(template.BaseLife.ToString()),
                        new BigNumber(template.BaseGold.ToString()),
                        enemyIcon
                    );
                }
            }

            if (enemy != null)
            {
                currentEnemy = enemy;
                UpdateUI();
            }
            else
            {
                MessageBox.Show("Не удалось загрузить врага по шансам спавна.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
