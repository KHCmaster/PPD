using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace PPDFramework.Sprites
{
    /// <summary>
    /// スプライトマネージャーの基底クラスです。
    /// </summary>
    public abstract class SpriteManagerBase
    {
        /// <summary>
        /// ルートディレクトリです。
        /// </summary>
        public const string RootDir = "sprites";

        /// <summary>
        /// バージョンです。
        /// </summary>
        public const string Version = "3.0";

        /// <summary>
        /// 最大のサイズです。
        /// </summary>
        public const int MaxSize = 4096;

        /// <summary>
        /// 許可された拡張子の一覧です。
        /// </summary>
        protected string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

        /// <summary>
        /// スプライト辞書の一覧を取得します。
        /// </summary>
        public SpriteDictionary[] Dicts
        {
            get;
            private set;
        }

        /// <summary>
        /// スプライト画像のフォルダを取得します。
        /// </summary>
        public string ImageDir
        {
            get;
            private set;
        }

        /// <summary>
        /// メタXMLのパスを取得します。
        /// </summary>
        public string MetaXmlPath
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="spriteDirName"></param>
        protected SpriteManagerBase(string spriteDirName)
        {
            if (!Directory.Exists(RootDir))
            {
                Directory.CreateDirectory(RootDir);
            }

            ImageDir = Path.Combine(RootDir, spriteDirName);
            MetaXmlPath = Path.Combine(ImageDir, "meta.xml");
            if (!Directory.Exists(ImageDir))
            {
                Directory.CreateDirectory(ImageDir);
            }
        }

        /// <summary>
        /// 初期化します。
        /// </summary>
        public void Initialize()
        {
            Dicts = GetDicts(out string version);
        }

        /// <summary>
        /// 必要な場合のみパッキングします。
        /// </summary>
        public void Pack()
        {
            var infos = GetImageInfos();
            Dicts = GetDicts(out string version);
            if (!CheckUpdate(infos) || version != Version)
            {
                Write(infos);
                Dicts = GetDicts(out version);
                OnPacked();
            }
        }

        /// <summary>
        /// 画像の情報の一覧を取得します。
        /// </summary>
        /// <returns></returns>
        protected abstract ImageInfo[] GetImageInfos();

        /// <summary>
        /// 更新があるかどうかをチェックします。
        /// </summary>
        /// <param name="infos"></param>
        /// <returns></returns>
        protected abstract bool CheckUpdate(ImageInfo[] infos);

        /// <summary>
        /// 更新があったときにパックされたときの処理です。
        /// </summary>
        protected virtual void OnPacked()
        {
#if DEBUG
            Console.WriteLine("{0} Packed", ImageDir);
#endif
        }

        private SpriteDictionary[] GetDicts(out string version)
        {
            version = "";
            var dicts = new List<SpriteDictionary>();
            if (File.Exists(MetaXmlPath))
            {
                var document = XDocument.Load(MetaXmlPath);
                version = document.Root.Element("Version").Value;
                var scales = document.Root.Element("Scales");
                var spriteCounts = new Dictionary<ImageScale, int>();
                if (scales != null)
                {
                    foreach (var scale in scales.Elements("Scale"))
                    {
                        var imageScale = new ImageScale(
                             int.Parse(scale.Element("Numerator").Value),
                             int.Parse(scale.Element("Denominator").Value));
                        var spriteCount = int.Parse(scale.Element("SpriteCount").Value);
                        spriteCounts.Add(imageScale, spriteCount);
                    }
                }
                foreach (var pair in spriteCounts)
                {
                    var scale = pair.Key;
                    for (var i = 0; i < pair.Value; i++)
                    {
                        dicts.Add(new SpriteDictionary(ImageDir, i, scale));
                    }
                }
            }
            return dicts.ToArray();
        }

        private void Write(ImageInfo[] imageInfos)
        {
            var spriteCounts = new Dictionary<ImageScale, int>();
            foreach (var group in imageInfos.GroupBy(info => info.Scale))
            {
                var scale = group.Key;
                var infos = group.ToArray();
                Array.Sort(infos, (i1, i2) =>
                {
                    if (i1.WidthPowerOfTwo == i2.WidthPowerOfTwo)
                    {
                        return i1.HeightPowerOfTwo - i2.HeightPowerOfTwo;
                    }
                    return i1.WidthPowerOfTwo - i2.WidthPowerOfTwo;
                });
                Array.Reverse(infos);
                int startIndex = 0;
                var spriteIndex = 0;
                while (startIndex < infos.Length)
                {
                    var packInfos = PackImpl(infos, startIndex, out startIndex, out int size);
                    var imagePath = Path.Combine(ImageDir, String.Format("{0}.{1}_{2}.png", spriteIndex, scale.Numerator, scale.Denominator));
                    var imageXmlPath = Path.Combine(ImageDir, String.Format("{0}.{1}_{2}.xml", spriteIndex, scale.Numerator, scale.Denominator));
                    SpriteDictionary.Write(packInfos, size, imagePath, imageXmlPath);
                    spriteIndex++;
                }
                spriteCounts.Add(scale, spriteIndex);
            }
            WriteMetaXml(spriteCounts);
        }

        private void WriteMetaXml(Dictionary<ImageScale, int> spriteCounts)
        {
            var doc = new XDocument();
            doc.Add(new XElement("Root"));
            doc.Root.Add(new XElement("Version", Version));
            doc.Root.Add(new XElement("SpriteCount", spriteCounts.Values.Sum()));
            doc.Root.Add(new XElement("Scales",
                spriteCounts.Select(kvp =>
                    new XElement("Scale",
                        new XElement("Numerator", kvp.Key.Numerator),
                        new XElement("Denominator", kvp.Key.Denominator),
                        new XElement("SpriteCount", kvp.Value)))));
            doc.Save(MetaXmlPath);
        }

        private PackInfo[] PackImpl(ImageInfo[] infos, int startIndex, out int nextIndex, out int size)
        {
            size = infos.Skip(startIndex).Max(i => i.WidthPowerOfTwo);
            nextIndex = startIndex;
            while (true)
            {
                var packInfos = new List<PackInfo>();
                var availables = new List<AvailableInfo>
                {
                    new AvailableInfo { Position = Vector2.Zero, Width = size }
                };
                bool overflow = false;
                var i = startIndex;
                for (; i < infos.Length; i++)
                {
                    var info = infos[i];
                    var foundAvailables = new List<AvailableInfo>();
                    foreach (var available in availables)
                    {
                        if (info.WidthPowerOfTwo <= available.Width)
                        {
                            foundAvailables.Add(available);
                        }
                    }
                    if (foundAvailables.Count == 0)
                    {
                        throw new Exception("Pos not found");
                    }
                    foundAvailables.Sort((fa1, fa2) =>
                    {
                        if (fa1.Position.Y == fa2.Position.Y)
                        {
                            return Math.Sign(fa1.Position.X - fa2.Position.X);
                        }
                        return Math.Sign(fa1.Position.Y - fa2.Position.Y);
                    });
                    var usingAvailable = foundAvailables[0];
                    if (usingAvailable.Position.Y + info.HeightPowerOfTwo > size)
                    {
                        overflow = true;
                        break;
                    }
                    packInfos.Add(new PackInfo(info, usingAvailable.Position));
                    availables.Remove(usingAvailable);
                    availables.Add(new AvailableInfo { Position = usingAvailable.Position + new Vector2(0, info.HeightPowerOfTwo), Width = info.WidthPowerOfTwo });
                    if (usingAvailable.Width - info.WidthPowerOfTwo > 0)
                    {
                        availables.Add(new AvailableInfo { Position = usingAvailable.Position + new Vector2(info.WidthPowerOfTwo, 0), Width = usingAvailable.Width - info.WidthPowerOfTwo });
                    }
                    while (true)
                    {
                        var removeAvailables = new HashSet<AvailableInfo>();
                        var addAvailables = new List<AvailableInfo>();
                        foreach (var group in availables.GroupBy(a => a.Position.Y).Where(g => g.Count() > 1))
                        {
                            AvailableInfo? prevAvailable = null;
                            foreach (var available in group.OrderBy(a => a.Position.X))
                            {
                                if (!prevAvailable.HasValue)
                                {
                                    prevAvailable = available;
                                    continue;
                                }
                                if (prevAvailable.Value.Position.X + prevAvailable.Value.Width == available.Position.X)
                                {
                                    removeAvailables.Add(prevAvailable.Value);
                                    removeAvailables.Add(available);
                                    addAvailables.Add(new AvailableInfo { Position = prevAvailable.Value.Position, Width = prevAvailable.Value.Width + available.Width });
                                }
                                break;
                            }
                        }
                        if (removeAvailables.Count == 0)
                        {
                            break;
                        }
                        foreach (var removeAvailable in removeAvailables)
                        {
                            availables.Remove(removeAvailable);
                        }
                        foreach (var addAvailable in addAvailables)
                        {
                            availables.Add(addAvailable);
                        }
                    }
                }
                if (overflow)
                {
                    if (size < MaxSize)
                    {
                        size *= 2;
                        continue;
                    }
                }
                nextIndex = i;
                return packInfos.ToArray();
            }
        }

        struct AvailableInfo : IEquatable<AvailableInfo>
        {
            public Vector2 Position;
            public float Width;

            public bool Equals(AvailableInfo other)
            {
                return Position == other.Position && Width == other.Width;
            }
        }
    }
}
