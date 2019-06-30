using PPDFramework.Shaders;
using System.Collections.Generic;

namespace PPDFramework
{
    /// <summary>
    /// 後でまとめて描画するマネージャークラスです。
    /// </summary>
    public class PostDrawManager : GameComponent
    {
        class Context
        {
            public GameComponent GameComponent { get; set; }
            public AlphaBlendContext AlphaBlendContext { get; set; }
            public int Depth { get; set; }
        }

        Queue<Context> contexts;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="device"></param>
        public PostDrawManager(PPDDevice device) : base(device)
        {
            contexts = new Queue<Context>();
        }

        /// <summary>
        /// 追加します。
        /// </summary>
        /// <param name="gameComponent"></param>
        /// <param name="alphaBlendContext"></param>
        /// <param name="depth"></param>
        public void Add(GameComponent gameComponent, AlphaBlendContext alphaBlendContext, int depth)
        {
            contexts.Enqueue(new Context
            {
                GameComponent = gameComponent,
                AlphaBlendContext = device.GetModule<AlphaBlendContextCache>().Clone(alphaBlendContext),
                Depth = depth
            });
        }

        /// <summary>
        /// 描画します。
        /// </summary>
        /// <param name="alphaBlendContext"></param>
        protected override void DrawImpl(AlphaBlendContext alphaBlendContext)
        {
            while (contexts.Count > 0)
            {
                var context = contexts.Dequeue();
                context.GameComponent.DrawInternal(context.AlphaBlendContext, context.Depth);
            }
        }
    }
}
