using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class BigNumber
    {
        //Атрибуты класса
        private readonly int[] number;
        private const int Base = 1000;

        //Свойства
        public int ArrayLength => number.Length;

        //Конструктор
        public BigNumber(string value)
        {
            value = value.Trim().TrimStart('0'); //Убираем пробелы и нули
            if (value.Length == 0)
                value = "0";

            int len = (value.Length + 2) / 3; //Вычисляем число блоков
            number = new int[len];
            int index = 0;

            for (int i = value.Length; i > 0; i -= 3)
            {
                int start = Math.Max(0, i - 3);
                string part = value.Substring(start, i - start);
                number[index++] = int.Parse(part);
            }
        }

        private BigNumber(int[] blocks)
        {
            number = blocks;
        }

        //Методы
        public BigNumber Clone() //Клонирование
        {
            int[] copy = new int[number.Length];
            Array.Copy(number, copy, number.Length);
            return new BigNumber(copy);
        }

        public override string ToString() //Переделываем в строку
        {
            StringBuilder sb = new StringBuilder();
            for (int i = number.Length - 1; i >= 0; i--)
            {
                if (i == number.Length - 1)
                    sb.Append(number[i].ToString());
                else
                    sb.Append(number[i].ToString("D3"));
            }
            return sb.ToString();
        }

        //Приватные методы для математики
        //Сложение поблочно
        private BigNumber Add(BigNumber bnum) 
        {
            int maxLength = Math.Max(number.Length, bnum.number.Length);
            int[] result = new int[maxLength + 1];
            int carry = 0;

            for (int i = 0; i < result.Length; i++)
            {
                int a = (i < number.Length) ? number[i] : 0;
                int b = (i < bnum.number.Length) ? bnum.number[i] : 0;
                int sum = a + b + carry;
                result[i] = sum % Base;
                carry = sum / Base;
            }

            return new BigNumber(TrimLeadingZeros(result));
        }
        //Вычитание
        private BigNumber Substruct(BigNumber bnum)
        {
            if (this.CompareTo(bnum) < 0)
                throw new InvalidOperationException("Результат вычитания отрицательный — не поддерживается.");

            int[] result = new int[number.Length];
            int borrow = 0;

            for (int i = 0; i < number.Length; i++)
            {
                int a = number[i] - borrow;
                int b = (i < bnum.number.Length) ? bnum.number[i] : 0;
                if (a < b)
                {
                    a += Base;
                    borrow = 1;
                }
                else
                {
                    borrow = 0;
                }
                result[i] = a - b;
            }

            return new BigNumber(TrimLeadingZeros(result));
        }
        //Умножение с округлением вниз
        private BigNumber Multiply(double multiplier)
        {
            int[] result = new int[number.Length + 1];
            long carry = 0;

            for (int i = 0; i < number.Length; i++)
            {
                long prod = (long)Math.Floor(number[i] * multiplier + carry);
                result[i] = (int)(prod % Base);
                carry = prod / Base;
            }

            if (carry > 0)
                result[number.Length] = (int)carry;

            return new BigNumber(TrimLeadingZeros(result));
        }
        //Деление
        private BigNumber Divide(double divisor)
        {
            if (divisor == 0)
                throw new DivideByZeroException();

            int[] result = new int[number.Length];
            double remainder = 0;

            for (int i = number.Length - 1; i >= 0; i--)
            {
                double current = number[i] + remainder * Base;
                result[i] = (int)(current / divisor);
                remainder = current % divisor;
            }

            return new BigNumber(TrimLeadingZeros(result));
        }
        //Убираем лишние нули
        private static int[] TrimLeadingZeros(int[] arr)
        {
            int i = arr.Length - 1;
            while (i > 0 && arr[i] == 0) i--;
            int[] res = new int[i + 1];
            Array.Copy(arr, res, i + 1);
            return res;
        }
        //Сравнение больших чисел
        private int CompareTo(BigNumber bnum)
        {
            if (bnum == null) return 1;
            if (number.Length != bnum.number.Length)
                return number.Length.CompareTo(bnum.number.Length);

            for (int i = number.Length - 1; i >= 0; i--)
            {
                if (number[i] != bnum.number[i])
                    return number[i].CompareTo(bnum.number[i]);
            }

            return 0;
        }

        // Операторы 
        public static BigNumber operator +(BigNumber a, BigNumber b) => a.Add(b);
        public static BigNumber operator -(BigNumber a, BigNumber b) => a.Substruct(b);
        public static BigNumber operator *(BigNumber a, double b) => a.Multiply(b);
        public static BigNumber operator /(BigNumber a, double b) => a.Divide(b);

        // Операторы сравнения
        public static bool operator >(BigNumber a, BigNumber b) => a.CompareTo(b) > 0;
        public static bool operator <(BigNumber a, BigNumber b) => a.CompareTo(b) < 0;
        public static bool operator >=(BigNumber a, BigNumber b) => a.CompareTo(b) >= 0;
        public static bool operator <=(BigNumber a, BigNumber b) => a.CompareTo(b) <= 0;

        //Операторы равенства
        public static bool operator ==(BigNumber a, BigNumber b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.CompareTo(b) == 0;
        }

        public static bool operator !=(BigNumber a, BigNumber b) => !(a == b);

        //Equals и GetHashCode (для ==)
        public override bool Equals(object obj)
        {
            if (obj is BigNumber other)
                return this == other;
            return false;
        }
        // Суммарный хэш по содержимому
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + number.Length;
                for (int i = 0; i < number.Length; i += Math.Max(1, number.Length / 8))
                {
                    hash = hash * 31 + number[i];
                }
                return hash;
            }
        }
    }
}
