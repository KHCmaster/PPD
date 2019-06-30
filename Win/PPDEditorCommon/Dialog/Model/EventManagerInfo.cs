using PPDFramework;
using System.Collections.Generic;
using System.Linq;

namespace PPDEditorCommon.Dialog.Model
{
    public class EventManagerInfo
    {
        SortedList<float, Event> events;

        public KeyValuePair<float, Event>[] Events
        {
            get
            {
                return events.ToArray();
            }
        }

        public EventManagerInfo()
        {
            events = new SortedList<float, Event>();
        }

        public void Add(float time, Event e)
        {
            if (!events.ContainsKey(time))
            {
                events[time] = e;
            }
        }
    }

    public class Event
    {
        public NoteType NoteType
        {
            get;
            set;
        }

        public float BPM
        {
            get;
            set;
        }

        public int[] InitializeOrders
        {
            get;
            set;
        }

        public Event()
        {
            NoteType = NoteType.Normal;
            BPM = -1;
            InitializeOrders = new int[10];
            for (int i = 0; i < InitializeOrders.Length; i++)
            {
                InitializeOrders[i] = i;
            }
        }
    }
}
