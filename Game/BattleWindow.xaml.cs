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

        private List<string> availableEnemies = new List<string>();
        private List<string> defeatedEnemies = new List<string>();

        public List<CEnemy> enemies = new List<CEnemy>(); //ДОБАВЛЕНО

        public BattleWindow(CPlayer player, IconList iconList, EnemyTemplateList enemyTemplates)
        {
            InitializeComponent();
            this.player = player ?? throw new ArgumentNullException(nameof(player));
            this.iconList = iconList ?? throw new ArgumentNullException(nameof(iconList));
            this.templates = enemyTemplates ?? throw new ArgumentNullException(nameof(enemyTemplates));

            availableEnemies = new List<string>(templates.GetListOfEnemyNames());
            defeatedEnemies = new List<string>();

            //CEnemy randEnemy = templates.FindByName("amogus");

            NextEnemy();
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
            availableEnemies = new List<string>(templates.GetListOfEnemyNames());
            defeatedEnemies.Clear();
            NextEnemy();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e) //Кнопка следующего врага
        {
            NextEnemy();
        }

        private void NextEnemy() // Обработка следующего врага
        {
            if (templates == null || templates.GetListOfEnemyNames().Count == 0) return;

            if (availableEnemies.Count == 0)
            {
                ClearBattleInterface();
                return;
            }

            // Берем случайного врага из доступных
            var randomName = availableEnemies[rng.Next(availableEnemies.Count)];
            var template = templates.GetEnemyByName(randomName);

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

                currentEnemy = new CEnemy(
                    template.Name,
                    new BigNumber(template.BaseLife.ToString()),
                    new BigNumber(template.BaseGold.ToString()),
                    enemyIcon
                );
                UpdateUI();
            }
        }

        private void EnemyIcon_MouseDown(object sender, MouseButtonEventArgs e) //ТУТ ПОМЕНЯТЬ
        {
            if (currentEnemy == null) return;

            var dmg = player.DealDamage();
            if (currentEnemy.TakeDamage(dmg, out BigNumber reward))
            {
                var defeatedEnemyName = currentEnemy.Name;
                availableEnemies.Remove(defeatedEnemyName);
                defeatedEnemies.Add(defeatedEnemyName);

                //enemies.

                player.AddGold(reward);
                MessageBox.Show($"Враг убит! Вот тебе {reward} золота!", "Победа!");

                if (availableEnemies.Count == 0)
                {
                    ClearBattleInterface();
                    MessageBox.Show("Все враги убиты! Нажми Repeat для очередного гринда...", "Победа!");
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
            EnemyNameText.Text = "Все враги побеждены!";
        }
    }
}
