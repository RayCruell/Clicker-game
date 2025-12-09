using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class CEnemy
    {
        // Приватные поля
        private string name;
        private BigNumber maxHitPoints;
        private BigNumber currentHitPoints;
        private BigNumber goldReward;
        private bool isDead;
        private CIcon icon;

        // НОВЫЕ ПОЛЯ для механик
        private string enemyType; // "Normal", "Armored", "Dodging", "Healing"
        private double armor;     // для бронированных
        private double dodgeChance; // для уворачивающихся
        private double healChance;  // для исцеляющихся
        private double healPercentage; // для исцеляющихся
        private Random rng = new Random();

        // Публичные свойства
        public string Name
        {
            get { return name; }
            private set { name = value; }
        }

        public BigNumber MaxHitPoints
        {
            get { return maxHitPoints; }
            private set { maxHitPoints = value; }
        }

        public BigNumber CurrentHitPoints
        {
            get { return currentHitPoints; }
            private set { currentHitPoints = value; }
        }

        public BigNumber GoldReward
        {
            get { return goldReward; }
            private set { goldReward = value; }
        }

        public bool IsDead
        {
            get { return isDead; }
            private set { isDead = value; }
        }

        public CIcon Icon
        {
            get { return icon; }
            private set { icon = value; }
        }

        // Конструктор по умолчанию
        public CEnemy()
        {
            name = "Unknown";
            maxHitPoints = new BigNumber("100");
            currentHitPoints = new BigNumber("100");
            goldReward = new BigNumber("10");
            isDead = false;
            icon = null;
            enemyType = "Normal";
        }

        // Основной конструктор
        public CEnemy(string name, BigNumber maxHp, BigNumber goldReward, CIcon icon = null)
        {
            this.name = name ?? "Unknown";
            this.maxHitPoints = maxHp?.Clone() ?? new BigNumber("100");
            this.currentHitPoints = this.maxHitPoints.Clone();
            this.goldReward = goldReward?.Clone() ?? new BigNumber("10");
            this.isDead = false;
            this.icon = icon;
            this.enemyType = "Normal";
        }

        // НОВЫЙ конструктор с типом врага и параметрами
        public CEnemy(string name, BigNumber maxHp, BigNumber goldReward, string enemyType,
                     CIcon icon = null, double specialParam1 = 0, double specialParam2 = 0)
        {
            this.name = name ?? "Unknown";
            this.maxHitPoints = maxHp?.Clone() ?? new BigNumber("100");
            this.currentHitPoints = this.maxHitPoints.Clone();
            this.goldReward = goldReward?.Clone() ?? new BigNumber("10");
            this.isDead = false;
            this.icon = icon;
            this.enemyType = enemyType;

            // Устанавливаем параметры в зависимости от типа
            switch (enemyType)
            {
                case "Armored":
                    armor = specialParam1;
                    break;
                case "Dodging":
                    dodgeChance = specialParam1;
                    break;
                case "Healing":
                    healChance = specialParam1;
                    healPercentage = specialParam2;
                    break;
            }
        }

        // Метод клонирования
        public CEnemy Clone()
        {
            var clone = new CEnemy(this.name, this.maxHitPoints.Clone(), this.goldReward.Clone(),
                                 this.enemyType, this.icon);

            // Копируем специальные параметры
            clone.armor = this.armor;
            clone.dodgeChance = this.dodgeChance;
            clone.healChance = this.healChance;
            clone.healPercentage = this.healPercentage;

            return clone;
        }

        // ОСНОВНОЙ МЕТОД с механиками и ВОЗВРАТОМ СТРОКИ ДЛЯ ЛОГА
        public bool TakeDamage(BigNumber dmg, out BigNumber GoldReward, out string logMessage)
        {
            logMessage = "";

            // Проверка: если враг уже мёртв
            if (isDead)
            {
                GoldReward = new BigNumber("0");
                logMessage = "Враг уже мёртв!";
                return false;
            }

            BigNumber originalDamage = dmg.Clone();
            BigNumber actualDamage = dmg.Clone();
            bool enemyDodged = false;
            bool enemyHealed = false;

            // 1. ПРОВЕРКА УВОРОТА (для Dodging врагов)
            if (enemyType == "Dodging" && dodgeChance > 0)
            {
                double roll = rng.NextDouble() * 100;
                if (roll <= dodgeChance)
                {
                    enemyDodged = true;
                    logMessage = $"{name} УВЕРНУЛСЯ! (шанс: {dodgeChance}%)";
                    GoldReward = new BigNumber("0");
                    return false;
                }
            }

            // 2. ПРИМЕНЕНИЕ БРОНИ (для Armored врагов)
            if (enemyType == "Armored" && armor > 0)
            {
                BigNumber armorValue = new BigNumber(((int)armor).ToString());
                if (armorValue > actualDamage)
                {
                    actualDamage = new BigNumber("1"); // Минимальный урон 1
                }
                else
                {
                    actualDamage = actualDamage - armorValue;
                }
                logMessage = $"{name}: Броня поглотила {armorValue} урона. ";
            }

            // 3. ПРИМЕНЕНИЕ УРОНА
            if (actualDamage > currentHitPoints)
            {
                currentHitPoints = new BigNumber("0");
            }
            else
            {
                currentHitPoints = currentHitPoints - actualDamage;
            }

            // 4. ПРОВЕРКА СМЕРТИ
            if (currentHitPoints <= new BigNumber("0"))
            {
                Die();
                GoldReward = goldReward.Clone();
                if (!string.IsNullOrEmpty(logMessage))
                    logMessage += $"УБИТ! +{GoldReward} золота";
                else
                    logMessage = $"{name} УБИТ! +{GoldReward} золота";
                return true;
            }

            // 5. ПРОВЕРКА ИСЦЕЛЕНИЯ (для Healing врагов)
            if (enemyType == "Healing" && healChance > 0)
            {
                double roll = rng.NextDouble() * 100;
                if (roll <= healChance)
                {
                    enemyHealed = true;
                    BigNumber healAmount = maxHitPoints * (healPercentage / 100.0);
                    BigNumber healthBefore = currentHitPoints.Clone();
                    currentHitPoints = currentHitPoints + healAmount;

                    if (currentHitPoints > maxHitPoints)
                        currentHitPoints = maxHitPoints.Clone();

                    logMessage += $"{name} ИСЦЕЛИЛСЯ на {healAmount} HP! ";
                }
            }

            // 6. ФОРМИРОВАНИЕ ФИНАЛЬНОГО СООБЩЕНИЯ
            if (string.IsNullOrEmpty(logMessage))
            {
                logMessage = $"{name}: получил {actualDamage} урона. Осталось HP: {currentHitPoints}";
            }
            else
            {
                if (!enemyDodged && !enemyHealed)
                    logMessage += $"Осталось HP: {currentHitPoints}";
            }

            GoldReward = new BigNumber("0");
            return false;
        }

        // Приватный метод
        private void Die()
        {
            isDead = true;
            currentHitPoints = new BigNumber("0");
        }

        // НОВЫЕ методы для BattleWindow (если нужны)
        public string GetEnemyType() => enemyType;
        public double GetArmor() => armor;
        public double GetDodgeChance() => dodgeChance;
        public double GetHealChance() => healChance;
        public double GetHealPercentage() => healPercentage;
    }
}
