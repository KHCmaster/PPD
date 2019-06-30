namespace BezierDrawControl
{
    class RingBuffer<T>
    {
        class CustomList<C>
        {
            C data;
            CustomList<C> previous;
            CustomList<C> next;
            public CustomList<C> Previous
            {
                get
                {
                    return previous;
                }
                set
                {
                    previous = value;
                }
            }
            public CustomList<C> Next
            {
                get
                {
                    return next;
                }
                set
                {
                    next = value;
                }
            }
            public C Data
            {
                get
                {
                    return data;
                }
                set
                {
                    data = value;
                }
            }
        }

        CustomList<T> head;
        CustomList<T> tail;
        int maximumSize;
        int currentSize;
        int index = -1;

        public RingBuffer(int a)
        {
            maximumSize = a;
        }

        public void Add(T s)
        {
            if (index != -1)
            {
                currentSize += index + 1;
                index = -1;
                head.Next = null;

            }
            var temp = new RingBuffer<T>.CustomList<T>
            {
                Data = s
            };
            if (head == null && tail == null)
            {
                head = temp;
                tail = temp;
                currentSize++;
                CheckSize();
            }
            else
            {
                temp.Previous = head;
                head.Next = temp;
                head = temp;
                currentSize++;
                CheckSize();
            }
        }

        private void CheckSize()
        {
            while (currentSize > maximumSize)
            {
                if (tail.Next != null)
                {
                    tail = tail.Next;
                    tail.Previous = null;
                    currentSize--;
                }
            }
        }

        public bool CanUndo
        {
            get
            {
                if (head.Previous != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool CanRedo
        {
            get
            {
                if (head.Next != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public T Previous
        {
            get
            {
                T ret = head.Previous.Data;
                head = head.Previous;
                index--;
                return ret;
            }
        }

        public T Next
        {
            get
            {
                T ret = head.Next.Data;
                head = head.Next;
                index++;
                return ret;
            }
        }
    }
}
