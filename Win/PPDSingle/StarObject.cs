using PPDFramework;

namespace PPDSingle
{
    class StarObject : GameComponent
    {
        PictureObject star;
        PictureObject disableStar;

        public bool Enabled
        {
            get
            {
                return !star.Hidden;
            }
            set
            {
                star.Hidden = !value;
                disableStar.Hidden = value;
            }
        }

        public StarObject(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
        {
            star = new PictureObject(device, resourceManager, Utility.Path.Combine("review_star.png"));
            disableStar = new PictureObject(device, resourceManager, Utility.Path.Combine("review_star_disable.png"));

            this.AddChild(star);
            this.AddChild(disableStar);

            Enabled = true;
        }
    }
}
