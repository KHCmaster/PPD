using PPDMultiCommon.Model;
using PPDMultiCommon.Tcp;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PPDMultiServer.Tcp
{
    public abstract class ServerContextBase : IDisposable
    {
        private bool disposed;
        Dictionary<Type, Tuple<object, MethodInfo>> processors;

        public bool Disposed
        {
            get;
            private set;
        }

        public IContextManager ContextManager
        {
            get;
            set;
        }

        public Host Host
        {
            get;
            set;
        }

        protected ServerContextBase(ServerContextBase previousContext)
        {
            processors = new Dictionary<Type, Tuple<object, MethodInfo>>();
        }

        protected void AddProcessor<T>(Action<T> processor) where T : NetworkData
        {
            processors[typeof(T)] = Tuple.Create(processor.Target, processor.Method);
        }

        public virtual void Start()
        {

        }

        public void Process(NetworkData networkData)
        {
            if (processors.TryGetValue(networkData.GetType(), out Tuple<object, MethodInfo> info))
            {
                info.Item2.Invoke(info.Item1, new object[] { networkData });
            }
        }

        public virtual void Update()
        {

        }

        public virtual void OnChildPoped()
        {

        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    DisposeResource();
                }
            }
            disposed = true;
        }

        protected virtual void DisposeResource()
        {
        }
    }
}
