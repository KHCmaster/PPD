using System;
using System.Collections.Generic;

namespace PPDFramework.Resource
{
    /// <summary>
    /// リソースマネージャーのクラス
    /// </summary>
    public class ResourceManager : DisposableComponent
    {
        Dictionary<string, ResourceBase> resources;
        Tuple<ResourceManager, bool>[] parents;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ResourceManager() : this(null)
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="parents"></param>
        public ResourceManager(Tuple<ResourceManager, bool>[] parents)
        {
            resources = new Dictionary<string, ResourceBase>(StringComparer.OrdinalIgnoreCase);
            this.parents = parents;
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~ResourceManager()
        {
            Dispose();
        }

        /// <summary>
        /// 追加します。すでに同じキーがある場合には上書きされません。
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public ResourceBase Add(string resourceName, ResourceBase resource)
        {
            lock (resources)
            {
                if (!resources.ContainsKey(resourceName))
                {
                    resources.Add(resourceName, resource);
                }
                return resource;
            }
        }

        /// <summary>
        /// 追加します。すでに同じキーがある場合は上書きされます
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public ResourceBase AddAndOverride(string resourceName, ResourceBase resource)
        {
            lock (resources)
            {
                resources[resourceName] = resource;
                return resource;
            }
        }

        /// <summary>
        /// リソースを取得します
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public virtual T GetResource<T>(string resourceName)
            where T : ResourceBase
        {
            if (TryGetResource(resourceName, out T resource))
            {
                return resource;
            }
            return null;
        }

        /// <summary>
        /// リソースを取得します
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public virtual bool TryGetResource<T>(string resourceName, out T resource) where T : ResourceBase
        {
            if (parents != null)
            {
                foreach (var p in parents)
                {
                    var parent = p.Item1;
                    if (parent != null && parent.TryGetResource(resourceName, out resource))
                    {
                        return true;
                    }
                }
            }
            lock (resources)
            {
                if (resources.ContainsKey(resourceName))
                {
                    resource = resources[resourceName] as T;
                    return true;
                }
            }
#if DEBUG
            if (parents != null && parents.Length > 0)
            {
                Console.WriteLine("Resource Not Found: {0}", resourceName);
            }
#endif
            resource = null;
            return false;
        }

        /// <summary>
        /// リソースを持っているか
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public bool HasResource<T>(string resourceName) where T : ResourceBase
        {
            return TryGetResource(resourceName, out T temp);
        }

        /// <summary>
        /// リソースを削除します。
        /// </summary>
        /// <param name="resourceName"></param>
        public void RemoveResource(string resourceName)
        {
            lock (resources)
            {
                if (TryGetResource<ResourceBase>(resourceName, out ResourceBase resource))
                {
                    resource.Dispose();
                    lock (resources)
                    {
                        resources.Remove(resourceName);
                    }
                }
            }
        }

        /// <summary>
        /// 破棄します。
        /// </summary>
        protected override void DisposeResource()
        {
            base.DisposeResource();
            foreach (ResourceBase cr in resources.Values)
            {
                cr.Dispose();
            }
            resources.Clear();
            if (parents != null)
            {
                foreach (var p in parents)
                {
                    if (p.Item2)
                    {
                        p.Item1.Dispose();
                    }
                }
            }
        }
    }
}
