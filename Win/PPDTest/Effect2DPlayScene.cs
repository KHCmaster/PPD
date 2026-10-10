using Effect2D;
using PPDFramework;
using System.Numerics;

namespace PPDTest
{
    /// <summary>
    /// Effect2D（.etd）のアニメーションを目視で確認するシーンです（ループ再生するので、撮影した画像は比較しない）。
    /// </summary>
    class Effect2DPlayScene(TestSceneManager testSceneManager, PPDDevice device) : TestSceneBase(testSceneManager, device)
    {
        protected override string Title
        {
            get
            {
                return "Effect2D の再生のテスト";
            }
        }

        protected override void OnInitialize()
        {
            var path = new PathManager(@"img\PPD\single").Combine("Loading.etd");
            // 通常の再生と逆再生を並べる
            var types = new[] { EffectManager.PlayType.Loop, EffectManager.PlayType.ReverseLoop };
            for (var i = 0; i < types.Length; i++)
            {
                var effect = new EffectObject(device, ResourceManager, path)
                {
                    Position = new Vector2(250 + 300 * i, 200),
                    PlayType = types[i]
                };
                contentSprite.AddChild(effect);
                contentSprite.AddChild(new TextureString(device, types[i].ToString(), 16, PPDColors.White)
                {
                    Position = new Vector2(220 + 300 * i, 300)
                });
                effect.Play();
            }
        }
    }
}
