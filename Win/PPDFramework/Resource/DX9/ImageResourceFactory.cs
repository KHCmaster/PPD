using System.IO;

namespace PPDFramework.Resource.DX9
{
    class ImageResourceFactory : IImageResourceFactory
    {
        public ImageResourceBase Create(PPDDevice device, string filename, bool pa)
        {
            return new ImageResource(device, filename, pa);
        }

        public ImageResourceBase Create(PPDDevice device, Stream stream, bool pa)
        {
            return new ImageResource(device, stream, pa);
        }
    }
}
