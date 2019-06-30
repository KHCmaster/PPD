using SharpDX;
using System;
using System.Text;

namespace Effect2D
{
    /// <summary>
    /// エフェクトのデータ構造
    /// </summary>
    public class EffectStateStructure : ICloneable
    {
        /// <summary>
        /// ブレンドモード
        /// </summary>
        public BlendMode BlendMode
        {
            get;
            set;
        }

        /// <summary>
        /// X
        /// </summary>
        public float X
        {
            get { return data[(int)RatioType.X]; }
            set { data[(int)RatioType.X] = value; }
        }

        /// <summary>
        /// Y
        /// </summary>
        public float Y
        {
            get { return data[(int)RatioType.Y]; }
            set { data[(int)RatioType.Y] = value; }
        }

        /// <summary>
        /// アルファ
        /// </summary>
        public float Alpha
        {
            get { return data[(int)RatioType.Alpha]; }
            set { data[(int)RatioType.Alpha] = value; }
        }

        /// <summary>
        /// 回転角
        /// </summary>
        public float Rotation
        {
            get { return data[(int)RatioType.Rotation]; }
            set { data[(int)RatioType.Rotation] = value; }
        }

        /// <summary>
        /// スケールX
        /// </summary>
        public float ScaleX
        {
            get { return data[(int)RatioType.ScaleX]; }
            set { data[(int)RatioType.ScaleX] = value; }
        }

        /// <summary>
        /// スケールY
        /// </summary>
        public float ScaleY
        {
            get { return data[(int)RatioType.ScaleY]; }
            set { data[(int)RatioType.ScaleY] = value; }
        }

        float[] data = new float[Utility.RatioTypeArray.Length];

        /// <summary>
        /// 合成されたアルファを取得します。
        /// </summary>
        public float ComposedAlpha
        {
            get;
            private set;
        }

        /// <summary>
        /// 合成されたマトリックスを取得します。
        /// </summary>
        public Matrix[] ComposedMatrices
        {
            get;
            private set;
        }

        /// <summary>
        /// 合成されたブレンドモードを取得します。
        /// </summary>
        public BlendMode ComposedBlendMode
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EffectStateStructure()
        {
            Initialize();
        }

        private EffectStateStructure(bool init)
        {
            if (init)
            {
                Initialize();
            }
        }

        private void Initialize()
        {
            X = 0;
            Y = 0;
            Alpha = 1;
            Rotation = 0;
            ScaleX = 1;
            ScaleY = 1;
        }

        /// <summary>
        /// 途中の状態を得る
        /// </summary>
        /// <param name="ratio">比</param>
        /// <param name="state">状態</param>
        /// <returns></returns>
        public EffectStateStructure GetMixedState(float ratio, EffectStateStructure state)
        {
            var ret = new EffectStateStructure(false)
            {
                BlendMode = BlendMode
            };
            foreach (RatioType type in Utility.RatioTypeArray)
            {
                ret[type] = GetDivision(data[(int)type], state[type], ratio);
            }
            return ret;
        }

        /// <summary>
        /// 途中の状態を得る
        /// </summary>
        /// <param name="ratios">比</param>
        /// <param name="state">状態</param>
        /// <returns></returns>
        public EffectStateStructure GetMixedState(float[] ratios, EffectStateStructure state)
        {
            var ret = new EffectStateStructure(false)
            {
                BlendMode = BlendMode
            };
            int iter = 0;
            foreach (RatioType type in Utility.RatioTypeArray)
            {
                ret[type] = GetDivision(data[(int)type], state[type], ratios[iter]);
                iter++;
            }
            return ret;
        }

        /// <summary>
        /// 合成します。
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public void Compose(EffectStateStructure parent)
        {
            var matrix = GetMatrix();
            if (parent.ComposedMatrices != null)
            {
                ComposedMatrices = new Matrix[parent.ComposedMatrices.Length + 1];
                Array.Copy(parent.ComposedMatrices, ComposedMatrices, parent.ComposedMatrices.Length);
            }
            else
            {
                ComposedMatrices = new Matrix[1];
            }
            ComposedMatrices[ComposedMatrices.Length - 1] = matrix;
            ComposedAlpha = Alpha = Alpha * parent.Alpha;
            ComposedBlendMode = parent.BlendMode;
            if (BlendMode != BlendMode.None)
            {
                ComposedBlendMode = BlendMode;
            }
        }

        private Matrix GetMatrix()
        {
            return Matrix.Transformation2D(Vector2.Zero, 0, new Vector2(ScaleX, ScaleY), Vector2.Zero, (float)(Rotation / 180 * Math.PI), new Vector2(X, Y));
        }

        private float GetDivision(float a, float b, float ratio)
        {
            return a * ratio + b * (1 - ratio);
        }

        /// <summary>
        /// 比のタイプでプロパティにアクセスする
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public float this[RatioType type]
        {
            get
            {
                return data[(int)type];
            }
            set
            {
                data[(int)type] = value;
            }
        }

        /// <summary>
        /// 文字列化
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.Append("X:");
            sb.Append(X);
            sb.Append(" ");
            sb.Append("Y:");
            sb.Append(Y);
            sb.Append(" ");
            sb.Append("Alpha:");
            sb.Append(Alpha);
            sb.Append(" ");
            sb.Append("Rotation:");
            sb.Append(Rotation);
            sb.Append(" ");
            sb.Append("ScaleX:");
            sb.Append(ScaleX);
            sb.Append(" ");
            sb.Append("ScaleY:");
            sb.Append(ScaleY);
            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// クローンメソッド
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var ess = new EffectStateStructure(false);
            Array.Copy(data, ess.data, ess.data.Length);
            ess.BlendMode = BlendMode;
            return ess;
        }
    }
}
