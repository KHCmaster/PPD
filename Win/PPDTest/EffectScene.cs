using PPDMath;
using PPDFramework;
using PPDFramework.RenderMask;
using PPDFramework.Shaders;

namespace PPDTest
{
    /// <summary>
    /// ガラス、モザイク、ボーダー、ブレンドとフィルタとマスクの組み合わせを確認するシーンです。
    /// </summary>
    class EffectScene(TestSceneManager testSceneManager, PPDDevice device) : TestSceneBase(testSceneManager, device)
    {
        protected override string Title
        {
            get
            {
                return "特殊効果テスト";
            }
        }

        protected override void OnInitialize()
        {
            var pathManager = new PathManager(@"img\PPD\main_game");

            // 手前に描くものから順に追加する（先に追加したものが手前に描かれる）

            // ガラス: 模様のある背景の上に置く
            contentSprite.AddChild(new TextureString(device, "GLASS", 60, PPDColors.White)
            {
                Position = new System.Numerics.Vector2(10, 0),
                BlendMode = Effect2D.BlendMode.Glass
            });

            // モザイク
            var mosaic = new PictureObject(device, ResourceManager, pathManager.Combine("num.png"))
            {
                Position = new System.Numerics.Vector2(400, 0)
            };
            mosaic.RenderMasks.Add(new MosaicMask { Size = 6 });
            contentSprite.AddChild(mosaic);

            // ボーダー: 内側・中央、乗算
            contentSprite.AddChild(new TextureString(device, "Inside", 40, PPDColors.White)
            {
                Position = new System.Numerics.Vector2(400, 60),
                Border = new Border { Thickness = 3, Color = new Vortice.Mathematics.Color4(1, 0, 0, 1), Type = BorderType.Inside }
            });
            contentSprite.AddChild(new TextureString(device, "Center", 40, PPDColors.White)
            {
                Position = new System.Numerics.Vector2(560, 60),
                Border = new Border { Thickness = 4, Color = new Vortice.Mathematics.Color4(0, 0, 1, 1), Type = BorderType.Center, Blend = Effect2D.BlendMode.Multiply }
            });

            // ブレンド + 半透明 + フィルタ + マスク
            var combined = new PictureObject(device, ResourceManager, pathManager.Combine("num.png"))
            {
                Position = new System.Numerics.Vector2(400, 130),
                Alpha = 0.7f,
                BlendMode = Effect2D.BlendMode.Screen,
                Mask = new PictureObject(device, ResourceManager, pathManager.Combine("num.png"))
                {
                    Position = new System.Numerics.Vector2(15, 5)
                }
            };
            combined.ColorFilters.Add(new HueColorFilter { Weight = 1, Rotation = 2 });
            contentSprite.AddChild(combined);

            // フィルタを 2 つ重ねる（中間テクスチャを使う経路）
            var twoFilters = new PictureObject(device, ResourceManager, pathManager.Combine("num.png"))
            {
                Position = new System.Numerics.Vector2(400, 180),
                Alpha = 0.8f
            };
            twoFilters.ColorFilters.Add(new InvertColorFilter { Weight = 1 });
            twoFilters.ColorFilters.Add(new SaturationColorFilter { Weight = 1, Scale = 2 });
            contentSprite.AddChild(twoFilters);

            // 模様のある背景（ガラスの下）
            for (var y = 0; y < 4; y++)
            {
                contentSprite.AddChild(new PictureObject(device, ResourceManager, pathManager.Combine("num.png"))
                {
                    Position = new System.Numerics.Vector2(0, 5 + y * 22),
                    Alpha = 0.9f
                });
            }
        }
    }
}
