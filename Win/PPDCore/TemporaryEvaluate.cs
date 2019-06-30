using PPDFramework;
using SharpDX;

namespace PPDCore
{
    /// <summary>
    /// 現状の評価表示UI
    /// </summary>
    class TemporaryEvaluate : GameComponent
    {
        PictureObject[] gages;
        PictureObject movieposdraw;

        int currentdraw = 4;

        public float MoviePosDrawX
        {
            set
            {
                movieposdraw.Position = new SharpDX.Vector2(value - movieposdraw.Width / 2, movieposdraw.Position.Y);
            }
        }

        public TemporaryEvaluate(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
        {
            movieposdraw = new PictureObject(device, resourceManager, Utility.Path.Combine("movieseeker", "movieseeker.png"))
            {
                Position = new SharpDX.Vector2(0, -1)
            };
            this.AddChild(movieposdraw);

            gages = new PictureObject[5];
            var filenames = new string[]{
                "cheapgage.png",
                "standardgage.png",
                "greatgage.png",
                "excellentgage.png",
                "perfectgage.png"
            };
            int iter = 0;
            foreach (string filename in filenames)
            {
                gages[iter] = new PictureObject(device, resourceManager, Utility.Path.Combine("movieseeker", filename))
                {
                    Hidden = true
                };
                this.AddChild(gages[iter]);
                iter++;
            }
            gages[4].Hidden = false;
            InitializeComponentPosition();
        }

        private void InitializeComponentPosition()
        {
            this.Position = new SharpDX.Vector2(0, 41);
        }

        /// <summary>
        /// 評価変更
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ratio"></param>
        public void Change(ResultEvaluateType type, float ratio)
        {
            gages[currentdraw].Hidden = true;
            currentdraw = (int)type - 1;
            gages[currentdraw].Rectangle = new RectangleF(0, 0, 800 * ratio, gages[currentdraw].Height);
            gages[currentdraw].Hidden = false;
        }

        public void Retry()
        {
            MoviePosDrawX = 0;
            gages[currentdraw].Hidden = true;
            currentdraw = 4;
            gages[currentdraw].Rectangle = new RectangleF(0, 0, 800, gages[currentdraw].Height);
            gages[currentdraw].Hidden = false;
            SetDefault();
            InitializeComponentPosition();
        }
    }
}
