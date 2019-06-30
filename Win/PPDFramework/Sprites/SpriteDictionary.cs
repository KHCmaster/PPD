using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace PPDFramework.Sprites
{
    /// <summary>
    /// スプライト辞書のクラスです。
    /// </summary>
    public class SpriteDictionary
    {
        static readonly Regex scaleRegex = new Regex(@"\.\d+_\d+(\.\w+)$");

        /// <summary>
        /// 画像のパスを取得します。
        /// </summary>
        public string ImagePath
        {
            get;
            private set;
        }

        /// <summary>
        /// スプライトの情報一覧を取得します。
        /// </summary>
        public Dictionary<string, SpriteInfo> Sprites
        {
            get;
            private set;
        }

        /// <summary>
        /// スプライトの情報一覧を取得します。
        /// </summary>
        public Dictionary<string, SpriteInfo> SpritesWithActualPath
        {
            get;
            private set;
        }

        /// <summary>
        /// イメージのスケールを取得します。
        /// </summary>
        public ImageScale ImageScale
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="index"></param>
        /// <param name="imageScale"></param>
        public SpriteDictionary(string dir, int index, ImageScale imageScale)
        {
            Sprites = new Dictionary<string, SpriteInfo>();
            SpritesWithActualPath = new Dictionary<string, SpriteInfo>();
            ImageScale = imageScale;
            ImagePath = Path.Combine(dir, String.Format("{0}.{1}_{2}.png", index, imageScale.Numerator, imageScale.Denominator));
            var xmlPath = Path.Combine(dir, String.Format("{0}.{1}_{2}.xml", index, imageScale.Numerator, imageScale.Denominator));
            if (File.Exists(xmlPath))
            {
                var document = XDocument.Load(xmlPath);
                foreach (var imageElement in document.Descendants("Image"))
                {
                    var path = imageElement.Element("Path").Value;
                    var scaleRemovedPath = RemoveScale(path);
                    var width = int.Parse(imageElement.Element("Width").Value);
                    var height = int.Parse(imageElement.Element("Height").Value);
                    var spaceWidth = int.Parse(imageElement.Element("SpaceWidth").Value);
                    var spaceHeight = int.Parse(imageElement.Element("SpaceHeight").Value);
                    var x = int.Parse(imageElement.Element("X").Value);
                    var y = int.Parse(imageElement.Element("Y").Value);
                    var lastWriteTime = DateTime.Parse(imageElement.Element("LastWriteTime").Value);
                    var spriteInfo = new SpriteInfo(scaleRemovedPath, width, height, spaceWidth, spaceHeight, x, y, lastWriteTime);
                    Sprites[scaleRemovedPath] = spriteInfo;
                    SpritesWithActualPath[path] = spriteInfo;
                }
            }
        }

        public static string RemoveScale(string path)
        {
            return scaleRegex.Replace(path, (match) =>
            {
                return match.Groups[1].Value;
            });
        }

        internal static void Write(PackInfo[] packInfos, int size, string imagePath, string imageXmlPath)
        {
            using (var bitmap = new Bitmap(size, size))
            {
                using (var g = Graphics.FromImage(bitmap))
                {
                    foreach (var packInfo in packInfos)
                    {
                        using (var img = packInfo.ImageInfo.GetImage())
                        {
                            g.DrawImage(
                                img,
                                new System.Drawing.Rectangle((int)packInfo.Position.X, (int)packInfo.Position.Y, img.Width, img.Height),
                                new System.Drawing.Rectangle(0, 0, img.Width, img.Height),
                                GraphicsUnit.Pixel);
                        }
                    }
                }
                Utility.PreMultiplyAlpha(bitmap);
                bitmap.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);
            }
            var doc = new XDocument();
            doc.Add(new XElement("Root"));
            doc.Root.Add(new XElement("Version", DirSpriteManager.Version));
            doc.Root.Add(new XElement("Size", size));
            var imagesElement = new XElement("Images");
            doc.Root.Add(imagesElement);
            foreach (var packInfo in packInfos)
            {
                imagesElement.Add(new XElement("Image",
                    new XElement("Path", packInfo.ImageInfo.Path),
                    new XElement("Width", packInfo.ImageInfo.Width),
                    new XElement("Height", packInfo.ImageInfo.Height),
                    new XElement("SpaceWidth", packInfo.ImageInfo.WidthPowerOfTwo),
                    new XElement("SpaceHeight", packInfo.ImageInfo.HeightPowerOfTwo),
                    new XElement("X", packInfo.Position.X),
                    new XElement("Y", packInfo.Position.Y),
                    new XElement("LastWriteTime", packInfo.ImageInfo.LastWriteTime)));
            }
            doc.Save(imageXmlPath);
        }
    }
}
