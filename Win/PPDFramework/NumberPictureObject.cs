using PPDFramework.Resource;
using PPDFramework.Shaders;
using PPDFramework.Vertex;
using SharpDX;
using System.Text;

namespace PPDFramework
{
    /// <summary>
    /// 数字描画クラス
    /// 0123456789のように等間隔で並んだ画像を使う必要があります
    /// </summary>
    public class NumberPictureObject : GameComponent
    {
        ImageResourceBase imageResource;
        Alignment alignment;
        int maxDigit;
        uint numValue;
        VertexInfo[] numberVertices;

        /// <summary>
        /// アライメント
        /// </summary>
        public Alignment Alignment
        {
            get { return alignment; }
            set
            {
                if (alignment != value)
                {
                    alignment = value;
                    UpdateResource();
                }
            }
        }

        /// <summary>
        /// 値
        /// </summary>
        public uint Value
        {
            get
            {
                return numValue;
            }
            set
            {
                if (numValue != value)
                {
                    numValue = value;
                    UpdateResource();
                }
            }
        }

        /// <summary>
        /// 一桁の幅
        /// </summary>
        public float DigitWidth
        {
            get
            {
                return (float)imageResource.Width / 10;
            }
        }

        /// <summary>
        /// 最大桁数。負なら制限なし
        /// </summary>
        public int MaxDigit
        {
            get
            {
                return maxDigit;
            }
            set
            {
                if (maxDigit != value)
                {
                    maxDigit = value;
                    UpdateResource();
                }
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="device"></param>
        /// <param name="filename">ファイルパス</param>
        /// <param name="resourceManager">リソースクラス</param>
        public NumberPictureObject(PPDDevice device, Resource.ResourceManager resourceManager, PathObject filename) : base(device)
        {
            imageResource = resourceManager.GetResource<ImageResourceBase>(filename);
            if (imageResource == null)
            {
                imageResource = (ImageResourceBase)resourceManager.Add(filename, ImageResourceFactoryManager.Factory.Create(device, filename, false));
            }
            UpdateResource();
        }

        /// <summary>
        /// 描画の処理
        /// </summary>
        /// <param name="alphaBlendContext"></param>
        protected override void DrawImpl(AlphaBlendContext alphaBlendContext)
        {
            alphaBlendContext.Texture = imageResource.Texture;
            foreach (var vertices in numberVertices)
            {
                alphaBlendContext.Vertex = vertices;
                device.GetModule<AlphaBlend>().Draw(device, alphaBlendContext);
            }
        }

        /// <summary>
        /// 描画するかどうかを返します
        /// </summary>
        /// <param name="alphaBlendContext"></param>
        /// <param name="depth"></param>
        /// <param name="childIndex"></param>
        /// <returns></returns>
        protected override bool OnCanDraw(AlphaBlendContext alphaBlendContext, int depth, int childIndex)
        {
            return numberVertices != null;
        }

        private float GetLeft(int length)
        {
            switch (Alignment)
            {
                case Alignment.Left:
                    return 0;
                case Alignment.Right:
                    return -DigitWidth * length;
                case Alignment.Center:
                    return -DigitWidth * length / 2;
            }
            return 0;
        }

        private void UpdateResource()
        {
            var valueString = Value.ToString();
            var verticesCount = 0;
            if (maxDigit < 0)
            {
                verticesCount = valueString.Length;
            }
            else if (valueString.Length < maxDigit)
            {
                var sb = new StringBuilder();
                sb.Append('0', maxDigit - valueString.Length);
                sb.Append(valueString);
                valueString = sb.ToString();
                verticesCount = valueString.Length;
            }
            else if (valueString.Length >= maxDigit)
            {
                valueString = valueString.Substring(valueString.Length - maxDigit);
                verticesCount = valueString.Length;
            }
            if (numberVertices == null || numberVertices.Length != verticesCount)
            {
                if (numberVertices != null)
                {
                    foreach (var vertices in numberVertices)
                    {
                        if (vertices != null)
                        {
                            vertices.Dispose();
                        }
                    }
                }
                numberVertices = new VertexInfo[verticesCount];
                for (var i = 0; i < verticesCount; i++)
                {
                    numberVertices[i] = device.GetModule<ShaderCommon>().CreateVertex(4);
                }
            }
            var left = GetLeft(valueString.Length);
            for (var i = 0; i < verticesCount; i++)
            {
                var textureSize = new Size2F(imageResource.Width, imageResource.Height);
                var offset = valueString[i] - '0';
                var rect = imageResource.GetActualUVRectangle(
                    (offset * DigitWidth) / textureSize.Width,
                    0,
                    ((offset + 1) * DigitWidth) / textureSize.Width,
                    imageResource.Height / textureSize.Height);
                var pos1X = i * DigitWidth + left;
                var pos2X = pos1X + DigitWidth;
                numberVertices[i].Write(new[] {
                    new ColoredTexturedVertex(new Vector3(pos1X, 0, 0.5f), rect.TopLeft),
                    new ColoredTexturedVertex(new Vector3(pos2X, 0, 0.5f), rect.TopRight),
                    new ColoredTexturedVertex(new Vector3(pos1X, imageResource.Height, 0.5f), rect.BottomLeft),
                    new ColoredTexturedVertex(new Vector3(pos2X, imageResource.Height, 0.5f), rect.BottomRight)
                });
            }
        }

        /// <summary>
        /// 破棄します
        /// </summary>
        protected override void DisposeResource()
        {
            base.DisposeResource();

            if (numberVertices != null)
            {
                foreach (var vertices in numberVertices)
                {
                    if (vertices != null)
                    {
                        vertices.Dispose();
                    }
                }
                numberVertices = null;
            }
        }
    }
}
