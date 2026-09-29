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
                // ラムダ式は（何も捕捉しなくても）インスタンスメソッドにコンパイルされ、WeakEventListener の静的メソッドのチェックで例外になるので、静的メソッドを渡す
                OnDetachAction = Detach,
                OnEventAction = FireCollectionChanged
            };
            eventObject.CollectionChanged += weakListener.OnEvent;

            return weakListener;
        }

        private static void Detach(WeakEventListener<CollectionChangedWeakEventSource, INotifyCollectionChanged, NotifyCollectionChangedEventArgs> listener, INotifyCollectionChanged source)
        {
            source.CollectionChanged -= listener.OnEvent;
        }

        private static void FireCollectionChanged(CollectionChangedWeakEventSource instance, object source, NotifyCollectionChangedEventArgs e)
        {
            instance.CollectionChanged?.Invoke(source, e);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

    }

}
