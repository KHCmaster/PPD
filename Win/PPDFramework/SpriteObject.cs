using System.Collections.Generic;

namespace PPDFramework
{
    /// <summary>
    /// 何も描画しない透明のスプライトです
    /// </summary>
    public class SpriteObject : GameComponent
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SpriteObject(PPDDevice device) : base(device)
        {
        }

        /// <summary>
        /// コンストラクターです。
        /// </summary>
        public SpriteObject(PPDDevice device, params GameComponent[] children)
            : this(device, (IEnumerable<GameComponent>)children)
        {

        }

        /// <summary>
        /// コンストラクターです。
        /// </summary>
        public SpriteObject(PPDDevice device, IEnumerable<GameComponent> children) : base(device)
        {
            foreach (GameComponent child in children)
            {
                AddChild(child);
            }
        }
    }

    /// <summary>
    /// 何も描画しない透明のスプライトです
    /// </summary>
    public class SpriteObject<T> : GameComponent where T : GameComponent
    {
        /// <summary>
        /// 子要素を取得します。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public new T this[int index]
        {
            get { return (T)base[index]; }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SpriteObject(PPDDevice device) : base(device)
        {
        }

        /// <summary>
        /// コンストラクターです。
        /// </summary>
        public SpriteObject(PPDDevice device, params T[] children)
            : this(device, (IEnumerable<T>)children)
        {

        }

        /// <summary>
        /// コンストラクターです。
        /// </summary>
        public SpriteObject(PPDDevice device, IEnumerable<T> children) : base(device)
        {
            foreach (T child in children)
            {
                AddChild(child);
            }
        }
    }
}
