using System.Collections.Generic;

namespace FlowScriptEngine
{
    class EventQueue
    {
        private Queue<EventQueue> queue;
        private EventQueue parent;
        EventSet _eventSet;
        public EventQueue()
        {
            queue = new Queue<EventQueue>();
        }

        public EventQueue GetEvent()
        {
            if (queue.Count > 0)
            {
                EventQueue eventQueue = null;
                while (queue.Count > 0)
                {
                    eventQueue = Dequeue();
                    EventQueue prev = eventQueue;
                    eventQueue = eventQueue.GetEvent();
                    if (eventQueue._eventSet != null)
                    {
                        Enqueue(prev);
                        break;
                    }
                }
                if (eventQueue == null || eventQueue._eventSet == null)
                {
                    return this;
                }
                return eventQueue;
            }
            else
            {
                return this;
            }
        }

        public void AddEventSet(EventSet eventSet)
        {
            if (queue.Count == 0 && this._eventSet == null)
            {
                this._eventSet = eventSet;
            }
            else
            {
                if (this._eventSet != null)
                {
                    InnerAddEventSet(this._eventSet);
                    _eventSet = null;
                }
                InnerAddEventSet(eventSet);
            }
        }

        private void InnerAddEventSet(EventSet eventSet)
        {
            var eventQueue = new EventQueue();
            eventQueue.AddEventSet(eventSet);
            Enqueue(eventQueue);
        }

        public void AddEventSetToDepth(EventSet eventSet)
        {
            var eventQueue = new EventQueue
            {
                queue = queue,
                _eventSet = _eventSet
            };
            this.queue = new Queue<EventQueue>();
            this._eventSet = null;
            this.AddEventSet(eventSet);
            Enqueue(eventQueue);
            /*EventQueue eventQueue = new EventQueue();
            eventQueue.AddEventSet(eventSet);
            queue.Enqueue(eventQueue);
            return eventQueue;*/
        }

        private void Enqueue(EventQueue eventQueue)
        {
            eventQueue.parent = this;
            this.queue.Enqueue(eventQueue);
        }

        private EventQueue Dequeue()
        {
            var ret = queue.Dequeue();
            ret.parent = null;
            return ret;
        }

        public EventSet GetEventSet()
        {
            EventSet temp = _eventSet;
            _eventSet = null;
            return temp;
        }

        public EventQueue GetLoopEndParent()
        {
            EventQueue parent = this.parent;
            while (parent != null)
            {
                if (parent._eventSet.IsLoopEnd)
                {
                    return parent;
                }
                parent = parent.parent;
            }
            return null;
        }

        public void ClearChildEvents()
        {
            while (queue.Count > 0)
            {
                var q = Dequeue();
                q.ClearChildEvents();
            }
        }

        public bool HasEventSet
        {
            get
            {
                return _eventSet != null;
            }
        }

        public bool DeeplyHasEventSet
        {
            get
            {
                if (HasEventSet)
                {
                    return true;
                }
                else
                {
                    foreach (EventQueue eq in queue)
                    {
                        if (eq.DeeplyHasEventSet)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }
        }

        public bool HasLoopEndInParents
        {
            get
            {
                EventQueue parent = this.parent;
                while (parent != null)
                {
                    if (parent._eventSet.IsLoopEnd)
                    {
                        return true;
                    }
                    parent = parent.parent;
                }
                return false;
            }
        }

        public void Clear()
        {
            queue = new Queue<EventQueue>();
            parent = null;
            _eventSet = null;
        }
    }

    class EventSet
    {
        public FlowEventHandler EventHandler;
        public FlowSourceObjectBase Source;
        public bool IsLoopEnd;
    }
}
