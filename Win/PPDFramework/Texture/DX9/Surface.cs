namespace PPDFramework.Texture.DX9
{
    class Surface : SurfaceBase
    {
        public SharpDX.Direct3D9.Surface _Surface
        {
            get;
            private set;
        }

        public Surface(SharpDX.Direct3D9.Surface surface)
        {
            _Surface = surface;
        }

        protected override void DisposeResource()
        {
            if (_Surface != null)
            {
                _Surface.Dispose();
                _Surface = null;
            }
            base.DisposeResource();
        }
    }
}
