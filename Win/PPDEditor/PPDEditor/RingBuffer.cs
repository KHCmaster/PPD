using System;
using System.Collections.Generic;
using System.Text;

namespace PPDEditor
{
    class RingBuffer<T>
    {
        internal class CustomList<C>
        {
            C data;
            CustomList<C> previous;
            CustomList<C> next;
            public CustomList()
            {
            }
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
        int maximumsize;
        int currentsize;
        int index = -1;
        public RingBuffer(int a)
        {
            maximumsize = a;
        }
        public void add(T s)
        {
            if (index != -1)
            {
                currentsize += index + 1;
                index = -1;
                head.Next = null;

            }
            CustomList<T> temp = new RingBuffer<T>.CustomList<T>();
            temp.Data = s;
            if (head == null && tail == null)
            {
                head = temp;
                tail = temp;
                currentsize++;
                checksize();
            }
            else
            {
                temp.Previous = head;
                head.Next = temp;
                head = temp;
                currentsize++;
                checksize();
            }
        }
        private void checksize()
        {
            while (currentsize > maximumsize)
            {
                if (tail.Next != null)
                {
                    tail = tail.Next;
                    tail.Previous = null;
                    currentsize--;
                }
            }
        }
        public bool canundo()
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
        public bool canredo()
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
        public T getprevious()
        {
            T ret = head.Previous.Data;
            head = head.Previous;
            index--;
            return ret;
        }
        public T getnext()
        {
            T ret = head.Next.Data;
            head = head.Next;
            index++;
            return ret;
        }
    }
}
