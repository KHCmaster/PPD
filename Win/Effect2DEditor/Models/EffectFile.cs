using Effect2D;
using System;
using System.IO;
using System.Linq;

namespace Effect2DEditor.Models
{
    /// <summary>
    /// エフェクトのファイル（*.etd）を読み書きします。
    /// </summary>
    public static class EffectFile
    {
        public const string Filter = "ETD|*.etd";

        /// <summary>
        /// エフェクトに追加できるファイル（画像と *.etd）を選ぶときのフィルターです。
        /// </summary>
        public const string AddFilter = "JPEG, GIF, PNG, ETD|*.jpg;*.jpeg;*.gif;*.png;*.etd|JPEG|*.jpg;*.jpeg|GIF|*.gif|PNG|*.png|ETD|*.etd";

        /// <summary>
        /// エフェクトに追加できるファイルかどうかを、拡張子で調べます。
        /// </summary>
        public static bool CanAdd(string path)
        {
            var extension = Path.GetExtension(path).ToLower();
            return extension == ".jpg" || extension == ".jpeg" || extension == ".gif" || extension == ".png" || extension == ".etd";
        }

        /// <summary>
        /// エフェクトを読み込みます。
        /// </summary>
        /// <param name="onImageError">画像を読めなかったときの処理。</param>
        public static EffectManager Load(string path, Action onImageError)
        {
            return EffectLoader.Load(path, filename =>
            {
                try
                {
                    Utility.ValidateImage(filename);
                }
                catch
                {
                    onImageError();
                }
            });
        }

        /// <summary>
        /// エフェクトを保存し、使っている画像を、ファイルと同じ名前のフォルダにコピーします。
        /// </summary>
        public static void Save(EffectManager effectManager, string path)
        {
            effectManager.Save(path);
            string dir = Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            foreach (BaseEffect be in effectManager.Effects.Cast<BaseEffect>())
            {
                if (be.Effects.Count != 0) continue;
                var resourcefname = Path.GetFileName(be.Filename);
                if (!File.Exists(dir + "/" + resourcefname)) File.Copy(be.Filename, dir + "/" + resourcefname, true);
            }
        }
    }
}
