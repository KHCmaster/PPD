using System;
using System.Collections.Generic;
using System.Linq;

namespace PPDFramework.Chars
{
    /// <summary>
    /// 文字画像をキャッシュするマネージャーです。
    /// </summary>
    public class CharCacheManager : DisposableComponent
    {
        Dictionary<char, Dictionary<float, Dictionary<string, CharCacheInfo>>> cache;
        Dictionary<float, Dictionary<string, SizeTextureManager>> sizeTextures;
        PPDDevice device;
        Direct2DImageEncoder encoder;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="device"></param>
        public CharCacheManager(PPDDevice device)
        {
            this.device = device;
            cache = new Dictionary<char, Dictionary<float, Dictionary<string, CharCacheInfo>>>();
            sizeTextures = new Dictionary<float, Dictionary<string, SizeTextureManager>>();
            encoder = new Direct2DImageEncoder(2048, 2048, 96);
        }

        internal CharCacheInfo Get(float fontsize, string facename, char c)
        {
            var info = GetCharCacheInfo(c, fontsize, facename);
            info.Increment();
            return info;
        }

        private CharCacheInfo GetCharCacheInfo(char c, float fontSize, string faceName)
        {
            Dictionary<float, Dictionary<string, CharCacheInfo>> sizeCache;
            Dictionary<string, CharCacheInfo> faceCache;
            CharCacheInfo info;
            lock (cache)
            {
                if (!cache.TryGetValue(c, out sizeCache))
                {
                    sizeCache = new Dictionary<float, Dictionary<string, CharCacheInfo>>();
                    cache[c] = sizeCache;
                }
            }
            lock (sizeCache)
            {
                if (!sizeCache.TryGetValue(fontSize, out faceCache))
                {
                    faceCache = new Dictionary<string, CharCacheInfo>();
                    sizeCache[fontSize] = faceCache;
                }
            }
            lock (faceCache)
            {
                if (!faceCache.TryGetValue(faceName, out info) || info.Disposed)
                {
                    info = new CharCacheInfo(fontSize, faceName, c);
                    SizeTextureManager sizeTextureManager = null;
                    if (!PPDSetting.Setting.CharacterTexturePackingDisabled)
                    {
                        sizeTextureManager = GetSizeTextureManager(fontSize, faceName);
                    }
                    info.CreateTexture(device, encoder, sizeTextureManager);
                    faceCache[faceName] = info;
                }
            }
            return info;
        }

        private SizeTextureManager GetSizeTextureManager(float fontSize, string faceName)
        {
            Dictionary<string, SizeTextureManager> faceTextures;
            SizeTextureManager sizeTextureManager;
            lock (sizeTextures)
            {
                if (!sizeTextures.TryGetValue(fontSize, out faceTextures))
                {
                    faceTextures = new Dictionary<string, SizeTextureManager>();
                    sizeTextures[fontSize] = faceTextures;
                }
            }
            lock (faceTextures)
            {
                if (!faceTextures.TryGetValue(faceName, out sizeTextureManager))
                {
                    sizeTextureManager = new SizeTextureManager(device);
                    faceTextures[faceName] = sizeTextureManager;
                }
            }
            return sizeTextureManager;
        }

        internal void Stats()
        {
            var usingCount = 0;
            var unusingCount = 0;
            lock (cache)
            {
                foreach (CharCacheInfo info in cache.SelectMany(c => c.Value.SelectMany(sc => sc.Value.Select(fc => fc.Value))))
                {
                    if (info.Count > 0)
                    {
                        usingCount++;
                    }
                    else if (!info.Disposed)
                    {
                        unusingCount++;
                    }
                }
            }
            Console.WriteLine("Using {0}, Unusing {1}", usingCount, unusingCount);
        }

        internal void ClearUnUsed()
        {
            lock (cache)
            {
                foreach (CharCacheInfo info in cache.SelectMany(c => c.Value.SelectMany(sc => sc.Value.Where(fc =>
                fc.Value.Count <= 0).Select(fc => fc.Value))))
                {
                    info.Dispose();
                }
            }
        }

        internal void FinalizeInternal()
        {
        }

        /// <summary>
        /// 破棄します。
        /// </summary>
        protected override void DisposeResource()
        {
            base.DisposeResource();
            lock (cache)
            {
                foreach (CharCacheInfo info in cache.SelectMany(c => c.Value.SelectMany(sc => sc.Value).Select(fc => fc.Value)))
                {
                    info.Dispose();
                }
                cache.Clear();
            }
            lock (sizeTextures)
            {
                foreach (var p in sizeTextures)
                {
                    lock (p.Value)
                    {
                        foreach (var pp in p.Value)
                        {
                            pp.Value.Dispose();
                        }
                        p.Value.Clear();
                    }
                }
                sizeTextures.Clear();
            }
        }
    }
}
