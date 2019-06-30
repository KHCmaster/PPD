using PPDFramework;
using PPDFramework.Resource;
using SharpDX;
using System;

namespace PPDCore
{
    class MarkConnectionCommon : GameComponent
    {
        public const int MaxWidth = 800;
        public const int SplitCount = 100;
        public const int ImageWidth = 32;
        public const float Diff = 1.5f;
        public const int ConnectionCount = 3;
        public const float BaseScale = 4;
        public const float RandMaxNum = 6;
        public const float RandWidth = 15;
        public const int WidthPerCount = MaxWidth / SplitCount * 2;

        Random r;
        int iter;

        public ImageResourceBase ImageResource
        {
            get;
            private set;
        }

        public ColoredTexturedVertex[][] Connects
        {
            get;
            private set;
        }

        public RectangleF ActualUVRectangle
        {
            get;
            private set;
        }

        public MarkConnectionCommon(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
        {
            var path = Utility.Path.Combine("connectpic.png");
            ImageResource = resourceManager.GetResource<ImageResourceBase>(path);
            ActualUVRectangle = ImageResource.GetActualUVRectangle(0, 0, 1, 1);
            r = new Random();
            Connects = new ColoredTexturedVertex[ConnectionCount][];
            for (int i = 0; i < ConnectionCount; i++)
            {
                Connects[i] = new ColoredTexturedVertex[SplitCount];
                for (int j = 0; j < SplitCount; j++)
                {
                    int x = -MaxWidth / 2 + MaxWidth / SplitCount * j;
                    Connects[i][j] = new ColoredTexturedVertex(new Vector3(x, 0, 0.5f));
                    j++;
                    Connects[i][j] = new ColoredTexturedVertex(new Vector3(x, 0, 0.5f));
                }
            }
            for (int i = 0; i < SplitCount + BaseScale; i++)
            {
                UpdateImpl();
            }
        }

        protected override void UpdateImpl()
        {
            for (var i = 0; i < Connects.Length; i++)
            {
                var vertexs = Connects[i];
                float basePos = (Connects.Length / 2 - i) * BaseScale;
                float zure = vertexs[0].Position.Y - basePos + ImageWidth / 2;
                basePos += Rand(zure);
                for (var j = SplitCount - 1; j >= 2; j--)
                {
                    vertexs[j].Position = new Vector3(vertexs[j].Position.X, vertexs[j - 2].Position.Y, 0.5f);
                    j--;
                    vertexs[j].Position = new Vector3(vertexs[j].Position.X, vertexs[j - 2].Position.Y, 0.5f);
                }
                vertexs[0].Position = new Vector3(vertexs[0].Position.X, basePos - ImageWidth / 2, 0.5f);
                vertexs[1].Position = new Vector3(vertexs[1].Position.X, basePos + ImageWidth / 2, 0.5f);
            }
        }

        protected override bool OnCanUpdate()
        {
            iter++;
            return iter % 4 == 0;
        }

        private float Rand(float totalZure)
        {
            var val = (float)(r.NextDouble() * RandMaxNum);
            if (val >= RandMaxNum * (RandWidth - 1) / RandWidth + totalZure)
            {
                return Diff;
            }
            else if (val >= RandMaxNum / RandWidth + totalZure)
            {
                return 0;
            }
            else
            {
                return -Diff;
            }
        }
    }
}