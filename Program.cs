using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Lab32
{
    /// <summary>
    /// Лабораторна робота 3.2. Дослідження колекцій та узагальнених колекцій C#.
    /// Варіант 13 — клас «Товар», обхід дерева — прямий порядок (preorder).
    /// </summary>
    internal static class Program
    {
        // Фіксована "сьогоднішня" дата, щоб результати роботи були відтворюваними.
        private static readonly DateTime Today = new DateTime(2026, 11, 12);

        private static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            Task1And2();
            Task3();
            Task4();
        }

        // =====================================================================
        //  ЗАВДАННЯ 1-2. Клас «Товар». Неузагальнена (ArrayList) та узагальнена
        //  (List<Product>) колекції + масив. Основні операції та їх порівняння.
        // =====================================================================
        private static void Task1And2()
        {
            Header("ЗАВДАННЯ 1-2. Колекції об'єктів класу «Товар»");

            Product milk = new Product("Молоко 2,5%", 101, new DateTime(2026, 11, 05), 10);
            Product bread = new Product("Хліб житній", 102, new DateTime(2026, 11, 08), 3);
            Product cheese = new Product("Сир твердий", 103, new DateTime(2026, 09, 01), 120);

            // ---------- Масив ----------
            Sub("Масив Product[] — фіксований розмір");
            Product[] array = new Product[3];
            array[0] = milk;
            array[1] = bread;
            array[2] = cheese;
            Console.WriteLine("Довжина масиву: " + array.Length + " (змінити не можна без створення нового масиву)");
            PrintAll(array);

            // ---------- Неузагальнена колекція ArrayList ----------
            Sub("ArrayList — звичайна (non-generic) колекція");
            ArrayList list = new ArrayList();
            list.Add(milk);                       // додавання
            list.Add(bread);
            list.Add(cheese);
            Console.WriteLine("Після додавання: Count = " + list.Count + ", Capacity = " + list.Capacity);

            list.Insert(1, new Product("Кефір 1%", 104, new DateTime(2026, 11, 08), 7));  // вставка
            Console.WriteLine("Після Insert(1, Кефір): Count = " + list.Count);

            list.Remove(bread);                   // видалення за значенням
            Console.WriteLine("Після Remove(Хліб): Count = " + list.Count);

            int idx = list.IndexOf(cheese);       // пошук
            Console.WriteLine("IndexOf(Сир) = " + idx + ", Contains(Сир) = " + list.Contains(cheese));

            // ОНОВЛЕННЯ елемента: тут обов'язкове приведення типу (розпакування посилання).
            ((Product)list[0]).ShelfLifeDays = 14;
            Console.WriteLine("Оновлено термін придатності 1-го елемента: " + list[0]);

            Console.WriteLine("Прохід по ArrayList (елементи мають тип object → потрібне приведення):");
            foreach (object o in list)
            {
                Product p = (Product)o;           // приведення типу на кожній ітерації
                Console.WriteLine("    " + p);
            }

            // Небезпека неузагальненої колекції: компілятор не заважає додати що завгодно.
            Sub("Чому ArrayList небезпечний: типи не перевіряються при компіляції");
            list.Add("я не товар");               // компілюється без помилок!
            list.Add(42);                         // int → упаковка (boxing) у object
            Console.WriteLine("Додано string та int у колекцію товарів — компілятор промовчав, Count = " + list.Count);
            try
            {
                Product broken = (Product)list[list.Count - 1];   // помилка тільки під час виконання
                Console.WriteLine(broken);
            }
            catch (InvalidCastException ex)
            {
                Console.WriteLine("    InvalidCastException під час виконання: " + ex.Message);
            }
            list.RemoveAt(list.Count - 1);
            list.RemoveAt(list.Count - 1);

            // ---------- Узагальнена колекція List<Product> ----------
            Sub("List<Product> — узагальнена (generic) колекція");
            List<Product> glist = new List<Product>();
            glist.Add(milk);
            glist.Add(bread);
            glist.Add(cheese);
            glist.Insert(1, new Product("Кефір 1%", 104, new DateTime(2026, 11, 08), 7));
            Console.WriteLine("Після додавання і вставки: Count = " + glist.Count);

            glist.Remove(bread);
            Console.WriteLine("Після Remove(Хліб): Count = " + glist.Count);
            Console.WriteLine("IndexOf(Сир) = " + glist.IndexOf(cheese) + ", Contains(Сир) = " + glist.Contains(cheese));

            glist[0].ShelfLifeDays = 12;          // оновлення БЕЗ приведення типу
            Console.WriteLine("Оновлено термін придатності 1-го елемента: " + glist[0]);

            Console.WriteLine("Прохід по List<Product> (тип елемента відомий компілятору):");
            foreach (Product p in glist)          // без приведення типу
                Console.WriteLine("    " + p);

            // glist.Add("я не товар");           // ← ПОМИЛКА КОМПІЛЯЦІЇ (саме в цьому перевага)

            Sub("Пошук за умовою в узагальненій колекції");
            Product found = glist.Find(p => p.Code == 103);
            Console.WriteLine("Find(Code == 103): " + found);
            List<Product> fresh = glist.FindAll(p => p.IsFitForConsumption(Today));
            Console.WriteLine("Придатних до споживання на " + D(Today) + ": " + fresh.Count);

            Sub("Висновок: порівняння поведінки");
            Console.WriteLine("  Масив       — фіксований розмір, типізований, найшвидший доступ за індексом.");
            Console.WriteLine("  ArrayList   — динамічний розмір, елементи зберігаються як object:");
            Console.WriteLine("                немає перевірки типів на етапі компіляції, потрібне приведення типу,");
            Console.WriteLine("                для значущих типів відбувається упаковка/розпакування (boxing/unboxing).");
            Console.WriteLine("  List<T>     — динамічний розмір + типобезпека на етапі компіляції,");
            Console.WriteLine("                без приведення типів і без boxing → швидше і надійніше.");
        }

        // =====================================================================
        //  ЗАВДАННЯ 3. Бінарне дерево з об'єктів класу «Товар».
        //  Дерево узагальнене (BinaryTree<T>), обхід — прямий порядок (preorder).
        // =====================================================================
        private static void Task3()
        {
            Header("ЗАВДАННЯ 3. Узагальнене бінарне дерево BinaryTree<T>");

            BinaryTree<Product> tree = new BinaryTree<Product>();
            tree.Add(new Product("Молоко 2,5%", 101, new DateTime(2026, 11, 05), 10));
            tree.Add(new Product("Хліб житній", 102, new DateTime(2026, 11, 08), 3));
            tree.Add(new Product("Сир твердий", 103, new DateTime(2026, 09, 01), 120));

            Console.WriteLine("Кількість вузлів: " + tree.Count + ", висота дерева: " + tree.Height);

            Sub("Прямий обхід (preorder) — варіант 13");
            int n = 1;
            foreach (Product p in tree)           // працює власний ітератор дерева
            {
                Console.WriteLine(n++ + ") " + p);
            }

            Sub("Повна інформація про товари (методи класу за варіантом)");
            foreach (Product p in tree)
            {
                p.PrintInfo(Today);
            }

            Sub("Дерево — узагальнене: елементами може бути будь-який тип-клас");
            BinaryTree<string> strings = new BinaryTree<string>();
            strings.Add("перший");
            strings.Add("другий");
            strings.Add("третій");
            Console.WriteLine("BinaryTree<string>, preorder: " + string.Join(", ", strings));

            Console.WriteLine();
            Console.WriteLine("Обмеження where T : class означає, що структури елементами бути не можуть:");
            Console.WriteLine("    // BinaryTree<int> t = new BinaryTree<int>();  ← помилка компіляції");
        }

        // =====================================================================
        //  ЗАВДАННЯ 4. Операція порівняння: IComparable<T> та IComparer<T>,
        //  сортування, власний ітератор (IEnumerable / IEnumerator).
        // =====================================================================
        private static void Task4()
        {
            Header("ЗАВДАННЯ 4. Порівняння, сортування, власний ітератор");

            Product[] items =
            {
                new Product("Сир твердий", 103, new DateTime(2026, 09, 01), 120),
                new Product("Молоко 2,5%", 101, new DateTime(2026, 11, 05), 10),
                new Product("Хліб житній", 102, new DateTime(2026, 11, 08), 3),
                new Product("Кефір 1%",    104, new DateTime(2026, 11, 08), 7)
            };

            // ---------- Дерево пошуку за IComparable<Product> (природний порядок — код) ----------
            Sub("SortedBinaryTree<Product> без порівнювача → IComparable<Product> (за кодом)");
            SortedBinaryTree<Product> byCode = new SortedBinaryTree<Product>();
            foreach (Product p in items) byCode.Add(p);

            Console.WriteLine("Прямий обхід (preorder) дерева пошуку:");
            PrintNumbered(byCode);
            Console.WriteLine("Симетричний обхід (inorder) — впорядковано за кодом:");
            PrintNumbered(byCode.InOrder());
            Console.WriteLine("Min = " + byCode.Min().Code + ", Max = " + byCode.Max().Code);
            Console.WriteLine("Contains(код 102) = " +
                byCode.Contains(new Product("пошук", 102, Today, 1)));

            // ---------- Дерево пошуку за зовнішнім IComparer<Product> ----------
            Sub("SortedBinaryTree<Product> з IComparer<Product> — за кінцевим строком реалізації");
            SortedBinaryTree<Product> byDeadline =
                new SortedBinaryTree<Product>(new ProductBySaleDeadlineComparer());
            foreach (Product p in items) byDeadline.Add(p);

            Console.WriteLine("Прямий обхід (preorder):");
            PrintNumbered(byDeadline);
            Console.WriteLine("Симетричний обхід (inorder) — за зростанням строку реалізації:");
            foreach (Product p in byDeadline.InOrder())
                Console.WriteLine("    " + D(p.GetSaleDeadline()) + "  —  " + p.Name);

            // ---------- Сортування вмісту дерева ----------
            Sub("Сортування вмісту дерева методом List<T>.Sort");
            BinaryTree<Product> tree = new BinaryTree<Product>();
            foreach (Product p in items) tree.Add(p);

            Console.WriteLine("Без порівнювача (IComparable<Product>, за кодом):");
            foreach (Product p in tree.ToSortedList())
                Console.WriteLine("    " + p.Code + "  —  " + p.Name);

            Console.WriteLine("З IComparer<Product> (за назвою):");
            foreach (Product p in tree.ToSortedList(new ProductByNameComparer()))
                Console.WriteLine("    " + p.Name);

            Console.WriteLine("З IComparer<Product> (за строком реалізації):");
            foreach (Product p in tree.ToSortedList(new ProductBySaleDeadlineComparer()))
                Console.WriteLine("    " + D(p.GetSaleDeadline()) + "  —  " + p.Name);

            // ---------- Неузагальнені IComparable / IComparer ----------
            Sub("Неузагальнені інтерфейси: Array.Sort та ArrayList.Sort");
            Product[] copy = (Product[])items.Clone();
            Array.Sort(copy);                                     // використовує IComparable
            Console.WriteLine("Array.Sort(array) → IComparable: " +
                string.Join(", ", copy.Select(p => p.Code.ToString())));

            ArrayList al = new ArrayList(items);
            al.Sort(new ProductByNameComparer());                 // використовує IComparer
            Console.WriteLine("ArrayList.Sort(IComparer) → за назвою: " +
                string.Join(", ", al.Cast<Product>().Select(p => p.Name)));

            // ---------- Власний ітератор ----------
            Sub("Власний ітератор: IEnumerable<T> / IEnumerator<T>");
            Console.WriteLine("а) неявне використання — foreach:");
            foreach (Product p in tree) Console.WriteLine("    " + p.Name);

            Console.WriteLine("б) явне використання — GetEnumerator / MoveNext / Current / Reset:");
            IEnumerator<Product> e = tree.GetEnumerator();
            while (e.MoveNext()) Console.WriteLine("    " + e.Current.Name);
            e.Reset();
            e.MoveNext();
            Console.WriteLine("    після Reset() перший елемент знову: " + e.Current.Name);
            e.Dispose();

            Console.WriteLine("в) ітератор робить дерево сумісним з LINQ:");
            Console.WriteLine("    придатних до споживання на " + D(Today) + ": " +
                tree.Where(p => p.IsFitForConsumption(Today)).Count() + " з " + tree.Count);
            Console.WriteLine("    найближче спливає: " +
                tree.OrderBy(p => p.GetSaleDeadline()).First().Name);
        }

        // ---------------------- допоміжні методи ----------------------

        private static void PrintAll(IEnumerable<Product> source)
        {
            foreach (Product p in source) Console.WriteLine("    " + p);
        }

        private static void PrintNumbered(IEnumerable<Product> source)
        {
            int n = 1;
            foreach (Product p in source)
                Console.WriteLine("    " + n++ + ") код " + p.Code + "  —  " + p.Name);
        }

        private static string D(DateTime date)
        {
            return date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
        }

        private static void Header(string text)
        {
            Console.WriteLine();
            Console.WriteLine(new string('=', 70));
            Console.WriteLine("  " + text);
            Console.WriteLine(new string('=', 70));
        }

        private static void Sub(string text)
        {
            Console.WriteLine();
            Console.WriteLine("--- " + text + " ---");
        }
    }
}
