using Effect2D;
using PPDFramework;
using PPDFramework.Resource;
using System.Numerics;

namespace PPDTest
{
    /// <summary>
    /// Effect2D（.etd）の描画を確認するシーンです。
    /// </summary>
    /// <remarks>
    /// 撮影した画像を比較できるように、再生はせず、決まったフレームにシークして止めたものを並べる。
    /// Loading.etd は、別の .etd（greenflare.etd）の参照、ベジエの移動、親子の合成（行列と透明度）を含む。
    /// </remarks>
    class Effect2DScene(TestSceneManager testSceneManager, PPDDevice device) : TestSceneBase(testSceneManager, device)
    {
        // 全体の長さに対する、表示するフレームの位置
        static readonly float[] frameRatios = [0.1f, 0.3f, 0.5f, 0.7f, 0.9f];

        protected override string Title
        {
            get
            {
                return "Effect2D のテスト";
            }
        }

        protected override void OnInitialize()
        {
            var path = new PathManager(@"img\PPD\single").Combine("Loading.etd");
            for (var i = 0; i < frameRatios.Length; i++)
            {
                // 2 つ目以降はキャッシュから複製される（Clone の経路も確認する）
                var manager = EffectLoader.Load(path, LoadImage);
                // 上の段に 3 つ、下の段に 2 つ並べる
                var position = i < 3 ? new Vector2(140 + 260 * i, 110) : new Vector2(270 + 260 * (i - 3), 290);
                var effect = new EffectObject(device, ResourceManager, manager)
                {
                    Position = position
                };
                manager.Seek(manager.FrameLength * frameRatios[i]);
                contentSprite.AddChild(effect);
                contentSprite.AddChild(new TextureString(device, string.Format("frame {0:F0}", manager.CurrentFrame), 16, PPDColors.White)
                {
                    Position = position + new Vector2(-30, 60)
                });
            }
        }

        private void LoadImage(string filename)
        {
            if (ResourceManager.GetResource<ImageResourceBase>(filename) == null)
            {
                ResourceManager.Add(filename, ImageResourceFactoryManager.Factory.Create(device, filename, false));
            }
        }
    }
}
