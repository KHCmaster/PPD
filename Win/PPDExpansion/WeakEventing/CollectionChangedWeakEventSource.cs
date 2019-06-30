using System.Collections.Specialized;

namespace PPDExpansion
{
    /// <summary>
    /// Default CollectionChanged weak-event wrapper for INotifyCollectionChanged event source.
    /// </summary>
    public class CollectionChangedWeakEventSource : WeakEventSourceBase<INotifyCollectionChanged>
    {
        protected override WeakEventListenerBase CreateWeakEventListener(INotifyCollectionChanged eventObject)
        {
            var weakListener = new WeakEventListener<CollectionChangedWeakEventSource,
                                                     INotifyCollectionChanged,
                                                     NotifyCollectionChangedEventArgs>(this, eventObject)
            {
                OnDetachAction = (listener, source) =>
                {
                    source.CollectionChanged -= listener.OnEvent;
                },
                OnEventAction = (instance, source, e) =>
                {
                    // fire event
                    instance.CollectionChanged?.Invoke(source, e);
                }
            };
            eventObject.CollectionChanged += weakListener.OnEvent;

            return weakListener;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

    }

}
