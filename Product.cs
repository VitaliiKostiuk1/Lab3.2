using System;
using System.Globalization;

namespace Lab32
{
    /// <summary>
    /// Завдання 1. Клас «Товар» (варіант 13).
    /// Дані: назва, код, дата виготовлення, термін придатності (у днях).
    /// Методи: визначення придатності до споживання, визначення кінцевого строку реалізації,
    /// виведення інформації про товар.
    /// Додатково реалізує інтерфейси порівняння (для завдання 4):
    /// IComparable&lt;Product&gt; (природний порядок — за кодом), IComparable, IEquatable&lt;Product&gt;.
    /// </summary>
    public class Product : IComparable<Product>, IComparable, IEquatable<Product>
    {
        private int _shelfLifeDays;

        /// <summary>Назва товару.</summary>
        public string Name { get; private set; }

        /// <summary>Код товару (унікальний ідентифікатор).</summary>
        public int Code { get; private set; }

        /// <summary>Дата виготовлення.</summary>
        public DateTime ManufactureDate { get; private set; }

        /// <summary>Термін придатності, у днях від дати виготовлення.</summary>
        public int ShelfLifeDays
        {
            get { return _shelfLifeDays; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Термін придатності має бути додатним.");
                _shelfLifeDays = value;
            }
        }

        public Product(string name, int code, DateTime manufactureDate, int shelfLifeDays)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Назва товару не може бути порожньою.", nameof(name));
            if (code <= 0)
                throw new ArgumentOutOfRangeException(nameof(code), "Код товару має бути додатним.");

            Name = name;
            Code = code;
            ManufactureDate = manufactureDate.Date;
            ShelfLifeDays = shelfLifeDays;
        }

        // ---------- Методи за варіантом ----------

        /// <summary>
        /// Визначення кінцевого строку реалізації:
        /// дата виготовлення + термін придатності (останній день, коли товар можна продавати).
        /// </summary>
        public DateTime GetSaleDeadline()
        {
            return ManufactureDate.AddDays(ShelfLifeDays);
        }

        /// <summary>
        /// Визначення придатності до споживання на задану дату:
        /// товар вже виготовлений і строк реалізації ще не минув.
        /// </summary>
        public bool IsFitForConsumption(DateTime onDate)
        {
            DateTime day = onDate.Date;
            return day >= ManufactureDate && day <= GetSaleDeadline();
        }

        /// <summary>Придатність до споживання на сьогодні.</summary>
        public bool IsFitForConsumption()
        {
            return IsFitForConsumption(DateTime.Today);
        }

        /// <summary>Виведення інформації про товар на екран (на задану дату).</summary>
        public void PrintInfo(DateTime onDate)
        {
            string fit = IsFitForConsumption(onDate) ? "так" : "ні";
            Console.WriteLine(this);
            Console.WriteLine("    Кінцевий строк реалізації: " + Format(GetSaleDeadline()));
            Console.WriteLine("    Придатний до споживання на " + Format(onDate) + ": " + fit);
        }

        /// <summary>Виведення інформації про товар на екран (на сьогодні).</summary>
        public void PrintInfo()
        {
            PrintInfo(DateTime.Today);
        }

        public override string ToString()
        {
            return "Товар \"" + Name + "\" (код " + Code + "), виготовлено " + Format(ManufactureDate) +
                   ", термін придатності " + ShelfLifeDays + " дн.";
        }

        private static string Format(DateTime date)
        {
            return date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
        }

        // ---------- Порівняння (завдання 4) ----------

        /// <summary>Природний порядок товарів — за кодом (IComparable&lt;T&gt;).</summary>
        public int CompareTo(Product other)
        {
            if (other == null) return 1;
            return Code.CompareTo(other.Code);
        }

        /// <summary>Неузагальнений варіант (IComparable) — для ArrayList, Array.Sort(Array) тощо.</summary>
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            Product other = obj as Product;
            if (other == null)
                throw new ArgumentException("Об'єкт має бути типу Product.", nameof(obj));
            return CompareTo(other);
        }

        // Рівність за кодом — потрібна для Contains, IndexOf, Remove у колекціях.
        public bool Equals(Product other)
        {
            return other != null && Code == other.Code;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Product);
        }

        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }
    }
}
