using Effect2D;
using PPDFramework.PPDStructure;
using PPDFramework.PPDStructure.PPDData;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace PPDEditor.Models
{
    /// <summary>
    /// リソースの種類です。
    /// </summary>
    public enum ResourceKind
    {
        Folder,
        Image,
        Effect,
        Sound,
        Others
    }

    /// <summary>
    /// プロジェクトの Resource フォルダーの中のファイル（画像、エフェクト、音、その他）を扱います。
    /// </summary>
    static class ResourceStore
    {
        public const string ResourcePath = "Resource";

        static readonly string[] allowImageExtensions = [".jpg", ".jpeg", ".gif", ".png"];
        static readonly string[] allowSoundExtensions = [".wav", ".ogg"];

        /// <summary>
        /// 種類ごとのフォルダー（Resource の中）の名前です。
        /// </summary>
        public static string GetFolderName(ResourceKind kind)
        {
            return kind switch
            {
                ResourceKind.Image => "Image",
                ResourceKind.Effect => "Effect",
                ResourceKind.Sound => "Sound",
                _ => "Others",
            };
        }

        private static string GetDir(string projectDir, ResourceKind kind)
        {
            return Path.Combine(projectDir, ResourcePath, GetFolderName(kind));
        }

        public static void CreateFolders(string projectDir)
        {
            foreach (var kind in new[] { ResourceKind.Image, ResourceKind.Effect, ResourceKind.Sound, ResourceKind.Others })
            {
                Utility.RecursiveCreateFolder(GetDir(projectDir, kind));
            }
        }

        /// <summary>
        /// 種類のフォルダーにあるファイルの名前です（その種類として扱える拡張子のものだけ）。
        /// </summary>
        public static IEnumerable<string> GetFiles(string projectDir, ResourceKind kind)
        {
            var dir = GetDir(projectDir, kind);
            Utility.RecursiveCreateFolder(dir);
            foreach (string filePath in Directory.GetFiles(dir))
            {
                var extension = Path.GetExtension(filePath).ToLower();
                var allowed = kind switch
                {
                    ResourceKind.Image => allowImageExtensions.Contains(extension),
                    ResourceKind.Effect => extension == ".etd",
                    ResourceKind.Sound => allowSoundExtensions.Contains(extension),
                    _ => true,
                };
                if (allowed)
                {
                    yield return Path.GetFileName(filePath);
                }
            }
        }

        /// <summary>
        /// エフェクトが使う画像（Effect\エフェクト名\画像）のパスです（Resource からの相対パス）。
        /// </summary>
        public static IEnumerable<string> GetEffectImages(string projectDir, string effectName)
        {
            var name = Path.GetFileNameWithoutExtension(effectName);
            var dir = Path.Combine(GetDir(projectDir, ResourceKind.Effect), name);
            foreach (string filePath in Directory.GetFiles(dir))
            {
                if (allowImageExtensions.Contains(Path.GetExtension(filePath)))
                {
                    yield return String.Format("Effect\\{0}\\{1}", name, Path.GetFileName(filePath));
                }
            }
        }

        /// <summary>
        /// ファイルを種類のフォルダーにコピーします。エフェクトは使う画像と、参照しているエフェクトもコピーします。
        /// </summary>
        /// <param name="onAdded">コピーしたファイルの名前を受け取ります（参照しているエフェクトも含む）。</param>
        /// <param name="onExists">同じ名前のファイルがすでにあったときに、そのパスを受け取ります。</param>
        public static void Add(string projectDir, ResourceKind kind, string filePath, Action<string> onAdded, Action<string> onExists)
        {
            var copyPath = Path.Combine(GetDir(projectDir, kind), Path.GetFileName(filePath));
            if (File.Exists(copyPath))
            {
                onExists(filePath);
                return;
            }

            CreateLowerCaseFolders(projectDir);
            File.Copy(filePath, copyPath, true);
            if (kind == ResourceKind.Effect)
            {
                foreach (string imagePath in Directory.GetFiles(Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath))))
                {
                    var newPath = Path.Combine(Path.GetDirectoryName(copyPath), Path.GetFileNameWithoutExtension(filePath), Path.GetFileName(imagePath));
                    Utility.RecursiveCreateFolder(newPath);
                    File.Copy(imagePath, newPath, true);
                }
            }
            onAdded(Path.GetFileName(filePath));

            if (kind == ResourceKind.Effect)
            {
                var dir = Path.GetDirectoryName(filePath);
                foreach (string fileName in EffectLoader.GetEffectReference(filePath))
                {
                    var path = Path.Combine(dir, fileName);
                    if (File.Exists(path))
                    {
                        Add(projectDir, kind, path, onAdded, onExists);
                    }
                }
            }
        }

        // 以前と同じく、小文字の名前のフォルダーも作っておく（Windows では同じフォルダー）
        private static void CreateLowerCaseFolders(string projectDir)
        {
            var dir = Path.Combine(projectDir, "resource");
            Directory.CreateDirectory(dir);
            foreach (string content in new[] { "image", "effect", "sound", "others" })
            {
                Directory.CreateDirectory(Path.Combine(dir, content));
            }
        }

        /// <summary>
        /// ファイルを消します。エフェクトは使う画像のフォルダーも消します。
        /// </summary>
        /// <param name="fullPath">Resource からの相対パスです。</param>
        public static void Delete(string projectDir, string fullPath)
        {
            var path = Path.Combine(projectDir, ResourcePath, fullPath);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            if (Path.GetExtension(path).Equals(".etd", StringComparison.CurrentCultureIgnoreCase))
            {
                var dir = Path.Combine(projectDir, Path.GetDirectoryName(path), Path.GetFileName(path));
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
            }
        }

        /// <summary>
        /// ファイルの中身をストリームに書きます（MOD に入れるときなど）。
        /// </summary>
        public static void Write(string projectDir, Stream stream, string fileName)
        {
            using FileStream fs = File.Open(Path.Combine(projectDir, ResourcePath, fileName), FileMode.Open);
            fs.CopyTo(stream);
        }

        /// <summary>
        /// 画像の情報（種類、大きさ、解像度、埋め込まれた情報）です。
        /// </summary>
        public static List<KeyValuePair<object, object>> GetImageInfo(string path)
        {
            var info = new List<KeyValuePair<object, object>>();
            using var image = System.Drawing.Image.FromFile(path);
            info.Add(new(Utility.Language["ImageKind"], GetImageFormatString(image.RawFormat)));
            info.Add(new(Utility.Language["ImageWidth"], image.Width));
            info.Add(new(Utility.Language["ImageHeight"], image.Height));
            info.Add(new(Utility.Language["ImageHorizontalDPI"], image.HorizontalResolution));
            info.Add(new(Utility.Language["ImageVerticalDPI"], image.VerticalResolution));
            foreach (PropertyItem propertyItem in image.PropertyItems)
            {
                string val = propertyItem.Type switch
                {
                    2 => System.Text.Encoding.ASCII.GetString(propertyItem.Value).Trim('\0'),
                    3 => BitConverter.ToUInt16(propertyItem.Value, 0).ToString(),
                    4 => BitConverter.ToUInt32(propertyItem.Value, 0).ToString(),
                    _ => "",
                };
                info.Add(new(propertyItem.Id, val));
            }
            return info;
        }

        private static string GetImageFormatString(ImageFormat format)
        {
            (ImageFormat Format, string Name)[] formats = [
                (ImageFormat.Bmp, "BMP"), (ImageFormat.Emf, "EMF"), (ImageFormat.Exif, "EXIF"), (ImageFormat.Gif, "GIF"),
                (ImageFormat.Icon, "ICON"), (ImageFormat.Jpeg, "JPEG"), (ImageFormat.MemoryBmp, "MEMORYBMP"), (ImageFormat.Png, "PNG"),
                (ImageFormat.Tiff, "TIFF"), (ImageFormat.Wmf, "WMF")
            ];
            return formats.FirstOrDefault(f => f.Format.Guid == format.Guid).Name ?? "";
        }

        /// <summary>
        /// エフェクトの情報（参照しているエフェクト、フレーム数、レイヤーの数など）です。
        /// </summary>
        public static List<KeyValuePair<object, object>> GetEffectInfo(string path)
        {
            var info = new List<KeyValuePair<object, object>>();
            var refs = EffectLoader.GetEffectReference(path);
            var manager = EffectLoader.Load(path, false, fn => { });
            info.Add(new(Utility.Language["ReferenceEffect"], String.Join(", ", refs)));
            info.Add(new(Utility.Language["FrameCount"], manager.FrameLength));
            info.Add(new(Utility.Language["StartFrame"], manager.StartFrame));
            int layerCount = 0;
            var effects = new Queue<IEffect>();
            effects.Enqueue(manager);
            while (effects.Count > 0)
            {
                var effect = effects.Dequeue();
                if (effect.Effects.Count == 0)
                {
                    layerCount++;
                }
                else
                {
                    foreach (IEffect childEffect in effect.Effects)
                    {
                        effects.Enqueue(childEffect);
                    }
                }
            }
            info.Add(new(Utility.Language["LayerCount"], layerCount));
            info.Add(new(Utility.Language["FPS"], manager.FPS));
            return info;
        }

        /// <summary>
        /// その他のファイルの情報です。譜面のファイルならマークの数と時間、そうでなければ大きさです。
        /// </summary>
        public static List<KeyValuePair<object, object>> GetOthersInfo(string path)
        {
            var info = new List<KeyValuePair<object, object>>();
            using FileStream fs = File.Open(path, FileMode.Open);
            bool notScore = false;
            var ppdData = PPDReader.Read(fs, ref notScore);
            if (notScore)
            {
                info.Add(new("サイズ", fs.Length));
                return info;
            }

            int normalCount = 0, longCount = 0;
            float startTime = float.MaxValue, endTime = float.MinValue;
            foreach (MarkDataBase data in ppdData)
            {
                startTime = Math.Min(startTime, data.Time);
                endTime = Math.Max(endTime, data.Time);
                if (data is ExMarkData exData)
                {
                    longCount++;
                    endTime = Math.Max(endTime, exData.EndTime);
                }
                else if (data is MarkData)
                {
                    normalCount++;
                }
            }
            if (ppdData.Length == 0)
            {
                startTime = endTime = 0;
            }
            info.Add(new("通常マークの数", normalCount));
            info.Add(new("長押しマークの数", longCount));
            info.Add(new("最初のマークの時間", startTime));
            info.Add(new("最後のマークの時間", endTime));
            return info;
        }
    }
}
