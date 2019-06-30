using System.Collections.Generic;

namespace PPDFramework
{
    /// <summary>
    /// スタック上に表示オブジェクトを配置するオブジェクトです。
    /// </summary>
    public class StackObject : SpriteObject
    {
        /// <summary>
        /// 水平に並べるかどうかを取得、設定します。
        /// </summary>
        public bool IsHorizontal
        {
            get;
            set;
        }

        /// <summary>
        /// コンストラクターです。
        /// </summary>
        public StackObject(PPDDevice device)
            : base(device)
        {
        }

        /// <summary>
        /// コンストラクターです。
        /// </summary>
        public StackObject(PPDDevice device, params GameComponent[] children)
            : base(device, children)
        {
        }

        /// <summary>
        /// コンストラクターです。
        /// </summary>
        public StackObject(PPDDevice device, IEnumerable<GameComponent> children) :
            base(device, children)
        {
        }

        /// <summary>
        /// 更新処理を行います。
        /// </summary>
        public override void Update()
        {
            base.Update();

            if (IsHorizontal)
            {
                LayoutHorizontal();
            }
            else
            {
                LayoutVertical();
            }

            base.Update();
        }

        private void LayoutHorizontal()
        {
            float sum = 0;
            foreach (GameComponent child in Children)
            {
                child.Position = new SharpDX.Vector2(sum, 0);
                sum += child.Width;
            }
        }

        private void LayoutVertical()
        {
            float sum = 0;
            foreach (GameComponent child in Children)
            {
                child.Position = new SharpDX.Vector2(0, sum);
                sum += child.Height;
            }
        }
    }
}
