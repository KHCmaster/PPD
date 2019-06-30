using MoreLinq;
using PPDFramework.Sprites;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PPDFramework.Resource
{
    /// <summary>
    /// スプライトのリソースマネージャーです。
    /// </summary>
    public class SpriteResourceManager : ResourceManager
    {
        ImageResourceBase[] imageResources;

        /// <summary>
        /// イメージリソースを取得します。
        /// </summary>
        public ImageResourceBase[] ImageResources
        {
            get { return imageResources; }
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="device"></param>
        /// <param name="dir"></param>
        public SpriteResourceManager(PPDDevice device, string dir) : this(device, new DirSpriteManager(dir))
        {
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="device"></param>
        /// <param name="spriteManager"></param>
        public SpriteResourceManager(PPDDevice device, SpriteManagerBase spriteManager) : base(null)
        {
            spriteManager.Initialize();
            var loadableImages = new Dictionary<string, List<Tuple<SpriteInfo, SpriteDictionary>>>();
            foreach (var dict in spriteManager.Dicts)
            {
                if (PPDSetting.Setting.HighResolutionImageDisabled && dict.ImageScale.Ratio != 1)
                {
                    continue;
                }
                if (File.Exists(dict.ImagePath))
                {
                    foreach (var pair in dict.Sprites)
                    {
                        if (!loadableImages.TryGetValue(pair.Key, out List<Tuple<SpriteInfo, SpriteDictionary>> list))
                        {
                            list = new List<Tuple<SpriteInfo, SpriteDictionary>>();
                            loadableImages.Add(pair.Key, list);
                        }
                        list.Add(new Tuple<SpriteInfo, SpriteDictionary>(pair.Value, dict));
                    }
                }
            }
            var usedSpriteDictionary = new Dictionary<SpriteDictionary, ImageResourceBase>();
            foreach (var pair in loadableImages)
            {
                var key = pair.Key;
                var list = pair.Value;
                var overScaledList = list.Where(i => i.Item2.ImageScale.Ratio >= device.Scale.X).ToArray();
                var nearest = overScaledList.Length > 0 ? overScaledList.MinBy(i => Math.Abs(i.Item2.ImageScale.Ratio - device.Scale.X)) : list.MinBy(i => Math.Abs(i.Item2.ImageScale.Ratio - device.Scale.X));
                if (!usedSpriteDictionary.TryGetValue(nearest.Item2, out var imageResource))
                {
                    imageResource = ImageResourceFactoryManager.Factory.Create(device, nearest.Item2.ImagePath, true);
                    usedSpriteDictionary.Add(nearest.Item2, imageResource);
                }
                Add(pair.Key, new SpriteImageResource(device, imageResource, nearest.Item1, nearest.Item2.ImageScale));
            }
            this.imageResources = usedSpriteDictionary.Values.ToArray();
        }

        /// <summary>
        /// 破棄します。
        /// </summary>
        protected override void DisposeResource()
        {
            base.DisposeResource();
            if (imageResources != null)
            {
                foreach (var imageResource in imageResources)
                {
                    imageResource.Dispose();
                }
                imageResources = null;
            }
        }
    }
}
