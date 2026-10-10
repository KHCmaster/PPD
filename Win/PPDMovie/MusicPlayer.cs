using DirectShow;
using PPDFramework;
using PPDFramework.Resource;
using PPDFramework.Texture;
using System.IO;

namespace PPDMovie
{
    public class MusicPlayer(PPDDevice device, string filename) : MusicPlayerBase(device, filename)
    {
        private ImageResourceBase resource;

        public override int Initialize()
        {
            this.graphBuilder2 = (IFilterGraph2)new FilterGraph();
            if (base.Initialize() != 0)
            {
                return -1;
            }

            var dir = Path.GetDirectoryName(filename);
            foreach (string filepath in Directory.GetFiles(dir, "thumb.*", SearchOption.TopDirectoryOnly))
            {
                resource = ImageResourceFactoryManager.Factory.Create(device, filepath, false);
                maxu = maxv = 1;
                width = 800;
                height = 450;
                break;
            }

            return 0;
        }

        public override bool Rotated
        {
            get
            {
                return false;
            }
        }

        public override void ReleaseCOM()
        {
            if (!initialized)
            {
                return;
            }

            resource?.Dispose();
            resource = null;
            base.ReleaseCOM();
        }

        public override TextureBase Texture
        {
            get
            {
                return resource?.Texture;
            }
        }
    }
}
