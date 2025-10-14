using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    internal class CEnemy
    {
        //Приватные поля
        private string name;
        private BigNumber maxHitPoints;
        private BigNumber currentHitPoints;
        private BigNumber goldReward;
        private bool isDead;
        private Icon icon;

        //Публичные свойства (инкапсуляция)
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

        public Icon Icon
        {
            get { return icon; }
            private set { icon = value; }
        }

        //Конструктор
        public CEnemy()
        {
            name = "Unknown";
            maxHitPoints = new BigNumber("100");
            currentHitPoints = new BigNumber("100");
            goldReward = new BigNumber("10");
            isDead = false;
            icon = null; // можно присвоить позже
        }

        //Публичный метод
        public bool TakeDamage(BigNumber dmg, out BigNumber GoldReward)
        {
            // Проверка: если враг уже мёртв
            if (isDead)
            {
                GoldReward = new BigNumber("0");
                return false;
            }

            // Вычитаем урон
            if (dmg > currentHitPoints)
                currentHitPoints = new BigNumber("0");
            else
                currentHitPoints = currentHitPoints - dmg;

            // Проверяем, жив ли враг
            if (currentHitPoints <= new BigNumber("0"))
            {
                Die();
                GoldReward = goldReward.Clone();
                return true;
            }

            GoldReward = new BigNumber("0");
            return false;
        }

        //Приватный метод
        private void Die()
        {
            isDead = true;
            currentHitPoints = new BigNumber("0");
        }
    }
}
