using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private List<string> availableEnemies = new List<string>();
        private List<string> defeatedEnemies = new List<string>();

        public BattleWindow(CPlayer player, IconList iconList, EnemyTemplateList enemyTemplates)
        {
            InitializeComponent();
            this.player = player ?? throw new ArgumentNullException(nameof(player));
            this.iconList = iconList ?? throw new ArgumentNullException(nameof(iconList));
            this.templates = enemyTemplates ?? throw new ArgumentNullException(nameof(enemyTemplates));

            availableEnemies = new List<string>(templates.GetListOfEnemyNames());
            defeatedEnemies = new List<string>();

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

                Debug.WriteLine($"Иконка врага: {currentEnemy.Icon?.ImagePath}");

                if (currentEnemy.Icon != null && File.Exists((string)currentEnemy.Icon.ImagePath))
                {
                    try
                    {
                        EnemyIcon.Source = new BitmapImage(new Uri((string)currentEnemy.Icon.ImagePath, UriKind.Absolute));
                        Debug.WriteLine("Иконка загружена успешно");
                    }
                    catch
                    {
                        Debug.WriteLine($"Ошибка загрузки иконки");
                        EnemyIcon.Source = null;
                    }
                }
                else
                {
                    Debug.WriteLine("Иконка не установлена или путь пустой");
                    EnemyIcon.Source = null;
                }
            }

            // Игрок
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

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            // Начинаем последовательность заново
            availableEnemies = new List<string>(templates.GetListOfEnemyNames());
            defeatedEnemies.Clear();
            NextEnemy(); // Это заполнит интерфейс новым врагом
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            NextEnemy();
        }

        private void NextEnemy()
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

                // СОЗДАЕМ врага НЕ удаляя из availableEnemies!
                currentEnemy = new CEnemy(
                    template.Name,
                    new BigNumber(template.BaseLife.ToString()),
                    new BigNumber(template.BaseGold.ToString()),
                    enemyIcon
                );

                UpdateUI();

                // НЕ удаляем из availableEnemies здесь!
                // Враг удалится только когда будет побежден в EnemyIcon_MouseDown
            }
        }

        private void EnemyIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (currentEnemy == null) return;

            var dmg = player.DealDamage();
            if (currentEnemy.TakeDamage(dmg, out BigNumber reward))
            {
                // Враг убит - ТОЛЬКО ТЕПЕРЬ удаляем из availableEnemies!
                var defeatedEnemyName = currentEnemy.Name;
                availableEnemies.Remove(defeatedEnemyName);
                defeatedEnemies.Add(defeatedEnemyName);

                player.AddGold(reward);
                MessageBox.Show($"Враг убит! Вот тебе {reward} золота!", "Победа!");

                // Проверяем не последний ли это был враг
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

        private void ClearBattleInterface()
        {
            currentEnemy = null;

            // Очищаем все поля
            EnemyNameText.Text = "";
            EnemyHPText.Text = "";
            EnemyGoldText.Text = "";
            EnemyIcon.Source = null;

            EnemyNameText.Text = "Все враги побеждены!";
        }
    }
}
