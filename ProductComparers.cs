using System;
using System.Collections;
using System.Collections.Generic;

namespace Lab32
{
    /// <summary>
    /// Базовий клас для зовнішніх порівнювачів товарів.
    /// Реалізує і узагальнений IComparer&lt;Product&gt;, і неузагальнений IComparer
    /// (останній потрібен, наприклад, для ArrayList.Sort(IComparer)).
    /// </summary>
    public abstract class ProductComparerBase : IComparer<Product>, IComparer
    {
        public int Compare(Product x, Product y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return CompareCore(x, y);
        }

        int IComparer.Compare(object x, object y)
        {
            return Compare(x as Product, y as Product);
        }

        protected abstract int CompareCore(Product x, Product y);
    }

    /// <summary>Порівняння за назвою (за алфавітом).</summary>
    public class ProductByNameComparer : ProductComparerBase
    {
        protected override int CompareCore(Product x, Product y)
        {
            return string.Compare(x.Name, y.Name, StringComparison.CurrentCulture);
        }
    }

    /// <summary>Порівняння за кінцевим строком реалізації (раніше спливає — «менший»).</summary>
    public class ProductBySaleDeadlineComparer : ProductComparerBase
    {
        protected override int CompareCore(Product x, Product y)
        {
            return x.GetSaleDeadline().CompareTo(y.GetSaleDeadline());
        }
    }
}
