using System;
using System.Drawing;
using System.Globalization;
using System.Text;

namespace BezierCaliculator
{
    /// <summary>
    /// ベジエ曲線頂点クラス
    /// </summary>
    public class BezierControlPoint
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BezierControlPoint()
        {
            ValidFirst = false;
            ValidSecond = true;
            ValidThird = false;
        }
        /// <summary>
        /// 一つ目のハンドルの向き
        /// </summary>
        public PointF FirstDirection
        {
            get;
            set;
        }
        /// <summary>
        /// 一つ目のハンドルの長さ
        /// </summary>
        public float FirstLength
        {
            get;
            set;
        }
        /// <summary>
        /// アンカー
        /// </summary>
        public PointF Second
        {
            get;
            set;
        }
        /// <summary>
        /// 二つ目のハンドルの向き
        /// </summary>
        public PointF ThirdDirection
        {
            get;
            set;
        }
        /// <summary>
        /// 二つ目のハンドルの長さ
        /// </summary>
        public float ThirdLength
        {
            get;
            set;
        }
        /// <summary>
        /// 一つ目のハンドルの位置
        /// </summary>
        public PointF First
        {
            get
            {
                return new PointF(Second.X + FirstDirection.X * FirstLength, Second.Y + FirstDirection.Y * FirstLength);
            }
            set
            {
                FirstDirection = GetNormalizePoint(new PointF(value.X - Second.X, value.Y - Second.Y));
                FirstLength = GetLength(new PointF(value.X - Second.X, value.Y - Second.Y));
            }
        }
        /// <summary>
        /// 二つ目のハンドルの位置
        /// </summary>
        public PointF Third
        {
            get
            {
                return new PointF(Second.X + ThirdDirection.X * ThirdLength, Second.Y + ThirdDirection.Y * ThirdLength);
            }
            set
            {
                ThirdDirection = GetNormalizePoint(new PointF(value.X - Second.X, value.Y - Second.Y));
                ThirdLength = GetLength(new PointF(value.X - Second.X, value.Y - Second.Y));
            }
        }
        /// <summary>
        /// 一つ目のハンドルが有効かどうか
        /// </summary>
        public bool ValidFirst
        {
            get;
            set;
        }
        /// <summary>
        /// アンカーが有効かどうか
        /// </summary>
        public bool ValidSecond
        {
            get;
            set;
        }
        /// <summary>
        /// 二つ目のハンドルが有効かどうか
        /// </summary>
        public bool ValidThird
        {
            get;
            set;
        }
        /// <summary>
        /// クローンメソッド
        /// </summary>
        /// <returns></returns>
        public BezierControlPoint Clone()
        {
            var bcp = new BezierControlPoint
            {
                FirstDirection = FirstDirection,
                FirstLength = FirstLength,
                Second = Second,
                ThirdDirection = ThirdDirection,
                ThirdLength = ThirdLength,
                ValidFirst = ValidFirst,
                ValidSecond = ValidSecond,
                ValidThird = ValidThird
            };
            return bcp;
        }
        private PointF GetNormalizePoint(PointF p1)
        {
            var length = (float)(Math.Sqrt(p1.X * p1.X + p1.Y * p1.Y));
            if (length == 0)
            {
                return new PointF(0, 0);
            }
            else
            {
                return new PointF(p1.X / length, p1.Y / length);
            }
        }
        private float GetLength(PointF p1)
        {
            var length = (float)(Math.Sqrt(p1.X * p1.X + p1.Y * p1.Y));
            return length;
        }
        /// <summary>
        /// シリアライズメソッド
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            var sb = new StringBuilder();
            sb.Append("VF=");
            sb.Append(ValidFirst ? "1" : "0");
            sb.Append(" ");
            sb.Append("VS=");
            sb.Append(ValidSecond ? "1" : "0");
            sb.Append(" ");
            sb.Append("VT=");
            sb.Append(ValidThird ? "1" : "0");
            sb.Append(" ");
            sb.Append("FDX=");
            sb.Append(FirstDirection.X.ToString(CultureInfo.InvariantCulture));
            sb.Append(" ");
            sb.Append("FDY=");
            sb.Append(FirstDirection.Y.ToString(CultureInfo.InvariantCulture));
            sb.Append(" ");
            sb.Append("SX=");
            sb.Append(Second.X.ToString(CultureInfo.InvariantCulture));
            sb.Append(" ");
            sb.Append("SY=");
            sb.Append(Second.Y.ToString(CultureInfo.InvariantCulture));
            sb.Append(" ");
            sb.Append("TDX=");
            sb.Append(ThirdDirection.X.ToString(CultureInfo.InvariantCulture));
            sb.Append(" ");
            sb.Append("TDY=");
            sb.Append(ThirdDirection.Y.ToString(CultureInfo.InvariantCulture));
            sb.Append(" ");
            sb.Append("FL=");
            sb.Append(FirstLength.ToString(CultureInfo.InvariantCulture));
            sb.Append(" ");
            sb.Append("TL=");
            sb.Append(ThirdLength.ToString(CultureInfo.InvariantCulture));
            return sb.ToString();
        }
        /// <summary>
        /// デシリアライズメソッド
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static BezierControlPoint Deserialize(string str)
        {
            var ret = new BezierControlPoint();
            var fd = new PointF(1, 0);
            var sec = new PointF(0, 0);
            var td = new PointF(1, 0);
            foreach (string st in str.Split(' '))
            {
                var content = st.Split('=');
                if (content.Length != 2)
                    throw new ArgumentException("Incorrect Format:" + st);
                switch (content[0])
                {
                    case "VF":
                        ret.ValidFirst = content[1] == "1";
                        break;
                    case "VS":
                        ret.ValidSecond = content[1] == "1";
                        break;
                    case "VT":
                        ret.ValidThird = content[1] == "1";
                        break;
                    case "FDX":
                        fd.X = float.Parse(content[1], CultureInfo.InvariantCulture);
                        break;
                    case "FDY":
                        fd.Y = float.Parse(content[1], CultureInfo.InvariantCulture);
                        break;
                    case "SX":
                        sec.X = float.Parse(content[1], CultureInfo.InvariantCulture);
                        break;
                    case "SY":
                        sec.Y = float.Parse(content[1], CultureInfo.InvariantCulture);
                        break;
                    case "TDX":
                        td.X = float.Parse(content[1], CultureInfo.InvariantCulture);
                        break;
                    case "TDY":
                        td.Y = float.Parse(content[1], CultureInfo.InvariantCulture);
                        break;
                    case "FL":
                        ret.FirstLength = float.Parse(content[1], CultureInfo.InvariantCulture);
                        break;
                    case "TL":
                        ret.ThirdLength = float.Parse(content[1], CultureInfo.InvariantCulture);
                        break;
                    default:
                        throw new ArgumentException("Incorrect Argument:" + content[0]);
                }
            }
            ret.FirstDirection = fd;
            ret.Second = sec;
            ret.ThirdDirection = td;
            return ret;
        }
    }
}
