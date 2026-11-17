using System;
using System.Collections;
using System.Collections.Generic;

namespace Lab32
{
    /// <summary>Вузол бінарного дерева. Тип даних — будь-який посилальний (клас).</summary>
    public class TreeNode<T> where T : class
    {
        public T Data { get; set; }
        public TreeNode<T> Left { get; set; }
        public TreeNode<T> Right { get; set; }

        public TreeNode(T data)
        {
            Data = data;
        }
    }

    /// <summary>
    /// Завдання 3. Узагальнене бінарне дерево BinaryTree&lt;T&gt;.
    /// Обмеження "where T : class" означає, що елементами дерева можуть бути тільки класи,
    /// а структури (int, DateTime, власні struct) — ні.
    /// Тут нове значення додається в перше вільне місце (по рівнях, зліва направо).
    ///
    /// Завдання 4 (частина). Власний ітератор: IEnumerable&lt;T&gt; + IEnumerator&lt;T&gt;,
    /// обхід — прямий порядок (preorder): корінь → ліве піддерево → праве піддерево.
    /// </summary>
    public class BinaryTree<T> : IEnumerable<T> where T : class
    {
        protected TreeNode<T> Root;

        /// <summary>Кількість елементів у дереві.</summary>
        public int Count { get; protected set; }

        /// <summary>Висота дерева (порожнє дерево — 0).</summary>
        public int Height
        {
            get { return GetHeight(Root); }
        }

        private static int GetHeight(TreeNode<T> node)
        {
            if (node == null) return 0;
            return 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right));
        }

        /// <summary>Додавання елемента у перше вільне місце (обхід у ширину).</summary>
        public virtual void Add(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            TreeNode<T> node = new TreeNode<T>(item);
            if (Root == null)
            {
                Root = node;
                Count++;
                return;
            }

            Queue<TreeNode<T>> queue = new Queue<TreeNode<T>>();
            queue.Enqueue(Root);
            while (queue.Count > 0)
            {
                TreeNode<T> current = queue.Dequeue();
                if (current.Left == null) { current.Left = node; break; }
                queue.Enqueue(current.Left);
                if (current.Right == null) { current.Right = node; break; }
                queue.Enqueue(current.Right);
            }
            Count++;
        }

        /// <summary>Пошук елемента повним перебором (у звичайному дереві немає порядку).</summary>
        public virtual bool Contains(T item)
        {
            foreach (T x in this)
            {
                if (EqualityComparer<T>.Default.Equals(x, item)) return true;
            }
            return false;
        }

        /// <summary>Очищення дерева.</summary>
        public void Clear()
        {
            Root = null;
            Count = 0;
        }

        /// <summary>
        /// Симетричний обхід (inorder): ліве піддерево → корінь → праве піддерево.
        /// Для дерева пошуку дає відсортовану послідовність.
        /// </summary>
        public IEnumerable<T> InOrder()
        {
            return InOrder(Root);
        }

        private static IEnumerable<T> InOrder(TreeNode<T> node)
        {
            if (node == null) yield break;
            foreach (T x in InOrder(node.Left)) yield return x;
            yield return node.Data;
            foreach (T x in InOrder(node.Right)) yield return x;
        }

        /// <summary>
        /// Сортування вмісту дерева викликом List.Sort.
        /// comparer == null — використовується IComparable&lt;T&gt; самого елемента (Comparer&lt;T&gt;.Default),
        /// інакше — зовнішній порівнювач IComparer&lt;T&gt;.
        /// </summary>
        public List<T> ToSortedList(IComparer<T> comparer = null)
        {
            List<T> list = new List<T>(this);
            if (comparer == null) list.Sort();
            else list.Sort(comparer);
            return list;
        }

        // ---------- Власний ітератор (IEnumerable / IEnumerator) ----------

        public IEnumerator<T> GetEnumerator()
        {
            return new PreorderEnumerator(Root);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Ітератор прямого обходу (preorder) без рекурсії — на основі стека.
        /// Стан «до першого MoveNext»: у стеку лежить тільки корінь, Current ще не визначений.
        /// </summary>
        private sealed class PreorderEnumerator : IEnumerator<T>
        {
            private readonly TreeNode<T> _root;
            private readonly Stack<TreeNode<T>> _stack = new Stack<TreeNode<T>>();
            private T _current;

            public PreorderEnumerator(TreeNode<T> root)
            {
                _root = root;
                Reset();
            }

            public T Current
            {
                get { return _current; }
            }

            object IEnumerator.Current
            {
                get { return _current; }
            }

            public bool MoveNext()
            {
                if (_stack.Count == 0)
                {
                    _current = null;
                    return false;
                }

                TreeNode<T> node = _stack.Pop();
                _current = node.Data;

                // Правого потомка кладемо першим, щоб лівий дістався зі стека раніше.
                if (node.Right != null) _stack.Push(node.Right);
                if (node.Left != null) _stack.Push(node.Left);
                return true;
            }

            public void Reset()
            {
                _stack.Clear();
                _current = null;
                if (_root != null) _stack.Push(_root);
            }

            public void Dispose()
            {
                // Некерованих ресурсів немає.
            }
        }
    }

    /// <summary>
    /// Завдання 4. Бінарне дерево пошуку з операцією порівняння.
    /// Порядок елементів задається або IComparable&lt;T&gt; самого типу (конструктор без параметрів),
    /// або зовнішнім IComparer&lt;T&gt; (конструктор з параметром).
    /// Менші елементи йдуть у ліве піддерево, рівні та більші — у праве.
    /// </summary>
    public class SortedBinaryTree<T> : BinaryTree<T> where T : class
    {
        private readonly IComparer<T> _comparer;

        public SortedBinaryTree() : this(null)
        {
        }

        public SortedBinaryTree(IComparer<T> comparer)
        {
            if (comparer == null)
            {
                bool comparable = typeof(IComparable<T>).IsAssignableFrom(typeof(T)) ||
                                  typeof(IComparable).IsAssignableFrom(typeof(T));
                if (!comparable)
                    throw new ArgumentException("Тип " + typeof(T).Name +
                        " не реалізує IComparable<T>/IComparable — передайте IComparer<T>.");
                comparer = Comparer<T>.Default;
            }
            _comparer = comparer;
        }

        public override void Add(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            TreeNode<T> node = new TreeNode<T>(item);
            if (Root == null)
            {
                Root = node;
                Count++;
                return;
            }

            TreeNode<T> current = Root;
            while (true)
            {
                if (_comparer.Compare(item, current.Data) < 0)
                {
                    if (current.Left == null) { current.Left = node; break; }
                    current = current.Left;
                }
                else
                {
                    if (current.Right == null) { current.Right = node; break; }
                    current = current.Right;
                }
            }
            Count++;
        }

        /// <summary>Пошук за порівнювачем: елемент знайдено, якщо Compare повертає 0.</summary>
        public override bool Contains(T item)
        {
            TreeNode<T> current = Root;
            while (current != null)
            {
                int cmp = _comparer.Compare(item, current.Data);
                if (cmp == 0) return true;
                current = cmp < 0 ? current.Left : current.Right;
            }
            return false;
        }

        /// <summary>Найменший елемент (за порядком порівняння).</summary>
        public T Min()
        {
            if (Root == null) throw new InvalidOperationException("Дерево порожнє.");
            TreeNode<T> current = Root;
            while (current.Left != null) current = current.Left;
            return current.Data;
        }

        /// <summary>Найбільший елемент (за порядком порівняння).</summary>
        public T Max()
        {
            if (Root == null) throw new InvalidOperationException("Дерево порожнє.");
            TreeNode<T> current = Root;
            while (current.Right != null) current = current.Right;
            return current.Data;
        }
    }
}
