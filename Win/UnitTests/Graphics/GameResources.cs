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
        readonly TempFolder folder;
        readonly ImageResourceBase image;
        readonly string imagePath;

        public ResourceManager Resources { get; }

        /// <summary>
        /// マークの画像（どれも赤い画像。エフェクトはない）です。
        /// </summary>
        public MarkImagePaths MarkImages { get; }

        /// <param name="setCurrentDirectory">今のフォルダーを一時フォルダーにするかどうか（しないときは、エフェクトを今のフォルダーに書く）。</param>
        public GameResources(GraphicsDevice graphics, bool setCurrentDirectory = true)
        {
            folder = new TempFolder(setCurrentDirectory);
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
            MarkImages = new MarkImagePaths(PathObject.Absolute(imagePath), PathObject.Absolute(folder.Combine("none.etd")));
        }

        public sealed class MarkImagePaths(PathObject image, PathObject missing) : PPDCore.MarkImagePathsBase
        {
            public override PathObject GetMarkColorImagePath(ButtonType buttontype) => image;
            public override PathObject GetMarkImagePath(ButtonType buttontype) => image;
            public override PathObject GetTraceImagePath(ButtonType buttontype) => image;
            public override void GetLongNoteCircleInfo(out PathObject imagepath, out float innerradius, out float outerradius)
            {
                imagepath = image;
                innerradius = 20;
                outerradius = 30;
            }
            public override PathObject GetCircleAxisImagePath() => image;
            public override PathObject GetClockAxisImagePath() => image;
            public override PathObject GetEvaluateEffectPath(MarkEvaluateType evatype) => missing;
            public override PathObject GetAppearEffectPath() => missing;
            public override PathObject GetSlideEffectPath() => missing;
            public override void GetHoldInfo(out PathObject imagepath, out float x, out float y)
            {
                imagepath = image;
                x = 0;
                y = 0;
            }
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
        /// 今のフォルダーからの相対パスに、0.1 秒の短いエフェクトを書きます（読み込みは今のフォルダーからなので）。
        /// </summary>
        public void WriteEffect(string relativePath)
        {
            var path = Path.Combine(Environment.CurrentDirectory, relativePath);
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
