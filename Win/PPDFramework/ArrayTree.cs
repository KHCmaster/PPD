using System.Collections.Generic;

namespace PPDFramework
{
    class ArrayTree<T, V>
        where T : struct
        where V : class
    {
        Dictionary<T, ArrayTree<T, V>> subtrees;
        private V value;
        private int depth;
        private ArrayTree<T, V> parent;


        public ArrayTree()
        {
            subtrees = new Dictionary<T, ArrayTree<T, V>>();
        }

        private ArrayTree(ArrayTree<T, V> parent, int depth)
        {
            subtrees = new Dictionary<T, ArrayTree<T, V>>();
            this.parent = parent;
            this.depth = depth;
        }

        public void Add(T[] array, V value)
        {
            Add(array, 0, value);
        }

        public void Add(T[] array, int index, V value)
        {
            if (array.Length == index)
            {
                this.value = value;
                return;
            }

            if (!subtrees.TryGetValue(array[index], out ArrayTree<T, V> tree))
            {
                tree = new ArrayTree<T, V>(this, depth + 1);
                subtrees.Add(array[index], tree);
            }

            tree.Add(array, index + 1, value);
        }

        public void Remove(T[] array)
        {
            var existTree = GetExistTree(array, 0);
            if (existTree != null)
            {
                existTree.subtrees.Remove(array[existTree.depth - 1]);
            }
        }

        public V Find(T[] array)
        {
            var existTree = GetExistTree(array, 0);
            if (existTree != null)
            {
                return existTree.value;
            }

            return null;
        }

        private ArrayTree<T, V> GetExistTree(T[] array, int index)
        {
            if (array.Length == index)
            {
                return this;
            }

            if (subtrees.TryGetValue(array[index], out ArrayTree<T, V> tree))
            {
                return tree.GetExistTree(array, index + 1);
            }

            return null;
        }
    }
}
