using PPDFramework;
using PPDFramework.Scene;
using PPDMultiCommon.Model;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PPDMulti
{
    public abstract class NetworkSceneBase(PPDDevice device) : SceneBase(device)
    {
        readonly Dictionary<Type, Tuple<object, MethodInfo>> processors = [];

        protected void AddProcessor<T>(Action<T> processor) where T : NetworkData
        {
            processors[typeof(T)] = Tuple.Create(processor.Target, processor.Method);
        }

        public void Process(NetworkData networkData)
        {
            if (processors.TryGetValue(networkData.GetType(), out Tuple<object, MethodInfo> info))
            {
                info.Item2.Invoke(info.Item1, [networkData]);
            }
        }
    }
}
