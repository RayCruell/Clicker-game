using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class CPlayer
    {
        private int lvl;
        private BigNumber gold;
        private BigNumber damage;
        private double damageModifier;
        private BigNumber upgradeCost;
        private double upgradeModifier;

        public int Lvl
        {
            get { return lvl; }
            private set { lvl = value; }
        }

        public BigNumber Gold
        {
            get { return gold; }
            private set { gold = value; }
        }

        public BigNumber Damage
        {
            get { return damage; }
            private set { damage = value; }
        }

        public double DamageModifier
        {
            get { return damageModifier; }
            private set { damageModifier = value; }
        }

        public BigNumber UpgradeCost
        {
            get { return upgradeCost; }
            private set { upgradeCost = value; }
        }

        public double UpgradeModifier
        {
            get { return upgradeModifier; }
            private set { upgradeModifier = value; }
        }

        // Конструктор
        public CPlayer()
        {
            lvl = 1;
            gold = new BigNumber("0");
            damage = new BigNumber("10");
            damageModifier = 1.2;
            upgradeCost = new BigNumber("50");
            upgradeModifier = 1.5;
        }

        //Добавление золота
        public void AddGold(BigNumber amount)
        {
            gold = gold + amount;
        }

        //Нанесение урона
        public BigNumber DealDamage()
        {
            return damage.Clone();
        }

        //Попытка апгрейда
        public bool TryUpgrade()
        {
            if (!TrySpendGold(upgradeCost))
                return false;

            lvl++;
            RecalculateStats();
            return true;
        }

        //Пересчёт характеристик после апгрейда
        private void RecalculateStats()
        {
            damage = damage * damageModifier;
            upgradeCost = CalculateNextUpgradeCost();
        }

        //Расчёт стоимости следующего апгрейда
        private BigNumber CalculateNextUpgradeCost()
        {
            return upgradeCost * upgradeModifier;
        }

        //Расчёт полного урона с учётом уровня
        private BigNumber CalculateTotalDamage()
        {
            BigNumber baseDamage = damage * (1.0 + (lvl - 1) * 0.1);
            return baseDamage;
        }

        //Проверка и списание золота при апгрейде
        public bool TrySpendGold(BigNumber amount)
        {
            if (gold >= amount)
            {
                gold = gold - amount;
                return true;
            }
            return false;
        }

        public void SpendGold(BigNumber amount)
        {
            if (gold >= amount)
                gold = gold - amount;
            else
                throw new InvalidOperationException("Недостаточно золота!");
        }
    }
}
