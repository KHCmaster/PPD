using Effect2D;
using PPDFramework;
using PPDFramework.Resource;
using System;
using System.IO;

namespace UnitTests.Graphics
{
    /// <summary>
    /// ゲームの部品（PPDCore）を、ゲームの画像なしで作れるようにします。
    /// 画像はどの名前でもテストの赤い画像を返し、エフェクト（.etd）は今のフォルダー（一時フォルダー）に書きます。
    /// </summary>
    /// <remarks>
    /// エフェクトは読めないとメッセージボックスを出すので、使うエフェクトは先に WriteEffect で書いておく。
    /// </remarks>
    sealed class GameResources : IDisposable
    {
        readonly TempFolder folder = new(setCurrentDirectory: true);
        readonly ImageResourceBase image;
        readonly string imagePath;

        public ResourceManager Resources { get; }

        public GameResources(GraphicsDevice graphics)
        {
            imagePath = folder.Combine("red.png");
            using (var bitmap = new System.Drawing.Bitmap(16, 16))
            {
                using (var g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    g.Clear(System.Drawing.Color.Red);
                }
                bitmap.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);
            }
            image = ImageResourceFactoryManager.Factory.Create(graphics.Device, PathObject.Absolute(imagePath), false);
            Resources = new AnyImageResourceManager(image);
        }

        /// <summary>
        /// どの名前の画像にも、同じ画像を返すリソースマネージャーです。
        /// </summary>
        sealed class AnyImageResourceManager(ImageResourceBase image) : ResourceManager
        {
            public override bool TryGetResource<T>(string resourceName, out T resource)
            {
                if (base.TryGetResource(resourceName, out resource))
                {
                    return true;
                }
                if (image is T found)
                {
                    resource = found;
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 今のフォルダーからの相対パスに、0.1 秒の短いエフェクトを書きます。
        /// </summary>
        public void WriteEffect(string relativePath)
        {
            var path = folder.Combine(relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var effect = new BaseEffect { Filename = imagePath };
            var set = new EffectStateRatioSet
            {
                StartFrame = 0,
                EndFrame = 6,
                StartState = new EffectStateStructure { Alpha = 1, ScaleX = 1, ScaleY = 1 },
                EndState = new EffectStateStructure { Alpha = 1, ScaleX = 1, ScaleY = 1 },
            };
            foreach (RatioType type in Effect2D.Utility.RatioTypeArray)
            {
                set[type] = new ConstantRatioMaker();
            }
            effect.Sets.Add(0, set);
            effect.CheckFrameLength();
            var manager = new EffectManager { FPS = 60 };
            manager.Effects.Add(effect);
            manager.Save(path);
        }

        public void Dispose()
        {
            Resources.Dispose();
            image.Dispose();
            folder.Dispose();
        }
    }
}
