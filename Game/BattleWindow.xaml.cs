using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Policy;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Game
{
    public partial class BattleWindow : Window
    {
        private CPlayer player;
        private CEnemy currentEnemy;
        private EnemyTemplateList templates;
        private IconList iconList;
        private Random rng = new Random();
        private CCountdownTimer clickCooldownTimer; //Таймер кулдауна
        private DispatcherTimer gameTimer; //Таймер обновления

        private List<CCollectable> bonusObjects = new List<CCollectable>();
        private double bonusSpawnTimer = 0;

        public List<CEnemy> enemies = new List<CEnemy>(); //Вместо двух списков у нас теперь только один, больше не реализуем два списка по именам, где были побеждённые и доступные враги

        public BattleWindow(CPlayer player, IconList iconList, EnemyTemplateList enemyTemplates)
        {
            InitializeComponent();
            this.player = player ?? throw new ArgumentNullException(nameof(player));
            this.iconList = iconList ?? throw new ArgumentNullException(nameof(iconList));
            this.templates = enemyTemplates ?? throw new ArgumentNullException(nameof(enemyTemplates));

            clickCooldownTimer = new CCountdownTimer(0.5); //Инициализация таймера
            InitializeGameTimer(); 

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

            CooldownText.Text = clickCooldownTimer.IsFinished() ? "Готово!" : $"{clickCooldownTimer.getTime():F1} сек";
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
            if (!clickCooldownTimer.IsFinished())
            {
                AddActionLog($"Перезарядка... ({clickCooldownTimer.getTime():F1} сек)");
                return;
            }
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

            AddActionLog($"Убит {currentEnemy.Name}! +{reward} золота");

            clickCooldownTimer.Start(0.5);
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

        private void InitializeGameTimer()
        {
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(100);
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();
        }

        private void GameTimer_Tick(object sender, EventArgs e) //Обновление перезарядки
        {
            clickCooldownTimer.update(0.1);
            CooldownText.Text = clickCooldownTimer.IsFinished() ? "Готово!" : $"{clickCooldownTimer.getTime():F1} сек";
            UpdateBonusObjects(0.1);
        }

        private void BonusCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point mousePosition = e.GetPosition(BonusCanvas);

            var objectsToCheck = new List<CCollectable>(bonusObjects);

            foreach (var bonus in objectsToCheck)
            {
                if (bonus.isMouseOnObject(mousePosition))
                {
                    if (currentEnemy != null)
                    {
                        BigNumber bonusDamage = bonus.GetDamageValue(player);

                        if (currentEnemy.TakeDamage(bonusDamage, out BigNumber reward))
                        {
                            RemoveDefeatedEnemy(currentEnemy.Name);
                            player.AddGold(reward);
                            AddActionLog($"Бонус убил врага! +{reward} золота");

                            if (enemies.Count == 0)
                            {
                                ClearBattleInterface();
                                AddActionLog("Все враги побеждены!");
                            }
                            else
                            {
                                NextEnemy();
                            }
                        }
                        else
                        {
                            AddActionLog($"Бонус нанес {bonusDamage} урона");
                        }
                    }

                    // ДАЁМ БОНУС ИГРОКУ
                    if (bonus is CDamageBooster damageBooster)
                    {
                        AddActionLog("Урон увеличен на 50%");
                    }
                    else if (bonus is CCooldownReducer cooldownReducer)
                    {
                        double newCooldown = clickCooldownTimer.GetDuration() * cooldownReducer.GetCooldownReduction();
                        clickCooldownTimer.SetDuration(newCooldown);
                        AddActionLog($"Перезарядка -15% ({newCooldown:F1} сек)");
                    }
                    else if (bonus is CGoldGiver goldGiver)
                    {
                        player.AddGold(goldGiver.GetGoldAmount());
                        AddActionLog($"+{goldGiver.GetGoldAmount()} золота");
                    }

                    bonusObjects.Remove(bonus);
                    RenderBonusObjects();
                    UpdateUI();
                    break;
                }
            }
        }

        private void UpdateBonusObjects(double delta)
        {
            bonusSpawnTimer += delta;

            // СПАВН БОНУСОВ КАЖДЫЕ 10 СЕКУНД
            if (bonusSpawnTimer >= 2.0)
            {
                SpawnBonusObject();
                bonusSpawnTimer = 0;
            }

            // ОБНОВЛЯЕМ ВРЕМЯ ЖИЗНИ БОНУСОВ
            for (int i = bonusObjects.Count - 1; i >= 0; i--)
            {
                if (bonusObjects[i].updateLifetime(delta))
                {
                    bonusObjects.RemoveAt(i);
                }
            }

            RenderBonusObjects();
        }

        private void SpawnBonusObject()
        {
            // Теперь бонусы появляются ТОЛЬКО на области картинки врага (150x150)
            double x = rng.NextDouble() * (150 - 25); // 150 - размер иконки, 25 - размер бонуса
            double y = rng.NextDouble() * (150 - 25);
            Point position = new Point(x, y);

            int type = rng.Next(0, 3);
            CCollectable bonus;

            switch (type)
            {
                case 0:
                    bonus = new CDamageBooster(position, 25, 5.0, 1.5);
                    break;
                case 1:
                    bonus = new CCooldownReducer(position, 25, 8.0, 0.7);
                    break;
                case 2:
                    bonus = new CGoldGiver(position, 25, 5.0, new BigNumber("50"));
                    break;
                default:
                    return;
            }

            bonusObjects.Add(bonus);
        }

        private void RenderBonusObjects()
        {
            BonusCanvas.Children.Clear();
            foreach (var bonus in bonusObjects)
            {
                BonusCanvas.Children.Add(bonus.getSprite());
            }
        }

        private void UpgradeCooldownButton_Click(object sender, RoutedEventArgs e)
        {
            BigNumber cost = new BigNumber("100");
            if (player.TrySpendGold(cost))
            {
                double newCooldown = clickCooldownTimer.GetDuration() * 0.97;
                clickCooldownTimer.SetDuration(newCooldown);
                AddActionLog($"Ускорение атаки -3% ({newCooldown:F1} сек)");
                UpdateUI();
            }
            else
            {
                AddActionLog("Недостаточно золота для ускорения");
            }
        }

        private void AddActionLog(string message)
        {
            // Добавляем новое сообщение в начало с временем
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            ActionLogText.Text = $"[{timestamp}] {message}\n" + ActionLogText.Text;

            // Ограничиваем лог 10 сообщениями
            var lines = ActionLogText.Text.Split('\n');
            if (lines.Length > 10)
            {
                ActionLogText.Text = string.Join("\n", lines.Take(10));
            }
        }
    }
}
