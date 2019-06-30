using Effect2D;
using PPDFramework.Texture;
using PPDFramework.Vertex;
using SharpDX;
using System;

namespace PPDFramework.Shaders
{
    /// <summary>
    /// アルファブレンドのコンテキストクラスです。
    /// </summary>
    public class AlphaBlendContext
    {
        /// <summary>
        /// SRTの最大数です。
        /// </summary>
        public const int MaxSRTCount = 32;

        /// <summary>
        /// フィルターの最大数です。
        /// </summary>
        public const int MaxFilterCount = 16;

        /// <summary>
        /// テクスチャのオーバーレイカラーを取得、設定します。
        /// </summary>
        public Color4 Overlay
        {
            get;
            set;
        }

        /// <summary>
        /// テクスチャを取得、設定します。
        /// </summary>
        public TextureBase Texture
        {
            get;
            set;
        }

        /// <summary>
        /// マスクのテクスチャを取得、設定します。
        /// </summary>
        public TextureBase MaskTexture
        {
            get;
            set;
        }

        /// <summary>
        /// SRT行列を取得、設定します。
        /// </summary>
        public Matrix[] SRTS
        {
            get;
            private set;
        }

        /// <summary>
        /// 深さを取得します。
        /// </summary>
        public int SRTDepth
        {
            get;
            set;
        }

        /// <summary>
        /// 頂点データを取得します。
        /// </summary>
        public VertexInfo Vertex
        {
            get;
            set;
        }

        /// <summary>
        /// アルファを取得、設定します。
        /// </summary>
        public float Alpha
        {
            get;
            set;
        }

        /// <summary>
        /// ブレンドモードを取得、設定します。
        /// </summary>
        public BlendMode BlendMode
        {
            get;
            set;
        }

        /// <summary>
        /// フィルターの一覧を取得します。
        /// </summary>
        public ColorFilterBase[] Filters
        {
            get;
            private set;
        }

        /// <summary>
        /// フィルターの個数を取得します。
        /// </summary>
        public int FilterCount
        {
            get;
            internal set;
        }

        /// <summary>
        /// 透明のコンテキストがどうかを取得します。
        /// </summary>
        public bool Transparent
        {
            get;
            internal set;
        }

        /// <summary>
        /// World行列を無効化するかどうかを取得します。
        /// </summary>
        public bool WorldDisabled
        {
            get;
            internal set;
        }

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        internal AlphaBlendContext()
        {
            SRTS = new Matrix[MaxSRTCount];
            Filters = new ColorFilterBase[MaxFilterCount];
            Overlay = PPDColors.Transparent;
        }

        /// <summary>
        /// SRTを設定します。
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="depth"></param>
        public void SetSRT(Matrix matrix, int depth)
        {
            if (depth >= 0 && depth < MaxSRTCount)
            {
                SRTS[depth] = matrix;
            }
        }

        /// <summary>
        /// フィルターを設定します。
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="depth"></param>
        public void SetFilter(ColorFilterBase filter, int depth)
        {
            if (depth >= 0 && depth < MaxFilterCount)
            {
                Filters[depth] = filter;
            }
        }

        /// <summary>
        /// クローンします。
        /// </summary>
        /// <returns></returns>
        internal void Clone(AlphaBlendContext context)
        {
            context.Alpha = Alpha;
            context.BlendMode = BlendMode;
            context.FilterCount = FilterCount;
            context.SRTDepth = SRTDepth;
            context.Texture = Texture;
            context.MaskTexture = MaskTexture;
            context.Vertex = Vertex;
            context.Overlay = Overlay;
            context.Transparent = Transparent;
            context.WorldDisabled = WorldDisabled;
            Array.Copy(SRTS, context.SRTS, SRTDepth + 1);
            Array.Copy(Filters, context.Filters, FilterCount);
        }

        /// <summary>
        /// 初期化します。
        /// </summary>
        internal void Initialize()
        {
            Alpha = 0;
            BlendMode = BlendMode.None;
            FilterCount = 0;
            SRTDepth = 0;
            Texture = null;
            MaskTexture = null;
            Vertex = null;
            Overlay = PPDColors.Transparent;
            Transparent = false;
            WorldDisabled = false;
        }
    }
}