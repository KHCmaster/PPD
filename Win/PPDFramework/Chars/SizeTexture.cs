using PPDFramework.Texture;
using SharpDX;
using System.Collections.Generic;

namespace PPDFramework.Chars
{
    class SizeTexture : DisposableComponent
    {
        const int Size = 1024;
        PPDDevice device;
        List<AvailableSpace> availableSpaces;
        List<AvailableSpace> onceUsedSpaces;

        public Vector2 HalfPixel
        {
            get { return new Vector2(0.5f / Size); }
        }

        public TextureBase Texture
        {
            get;
            private set;
        }

        public SizeTexture(PPDDevice device)
        {
            this.device = device;
            Texture = ((Texture.DX9.TextureFactory)TextureFactoryManager.Factory).Create(device, Size, Size, 1, SharpDX.Direct3D9.Pool.Default, true);
            availableSpaces = new List<AvailableSpace>
            {
                new AvailableSpace(this, Point.Zero, new Size2(Size, Size))
            };
            onceUsedSpaces = new List<AvailableSpace>();
        }

        public bool Write(TextureBase texture, int width, int height, out Vector2 uv, out Vector2 uvSize, out AvailableSpace usingAvailableSpace)
        {
            uv = Vector2.Zero;
            uvSize = Vector2.Zero;
            usingAvailableSpace = null;
            AvailableSpace usedAvailableSpace = null;
            foreach (var availableSpace in onceUsedSpaces)
            {
                if (width == availableSpace.Size.Width && height == availableSpace.Size.Height)
                {
                    device.StretchRectangle(texture,
                           null,
                           Texture,
                           new SharpDX.Mathematics.Interop.RawPoint((int)availableSpace.Position.X, (int)availableSpace.Position.Y));
                    uv = new Vector2(availableSpace.Position.X / (float)Size, availableSpace.Position.Y / (float)Size);
                    uvSize = new Vector2(width / (float)Size, height / (float)Size);
                    usedAvailableSpace = availableSpace;
                    usingAvailableSpace = new AvailableSpace(this, availableSpace.Position, new Size2(width, height));
                    break;
                }
            }
            if (usedAvailableSpace != null)
            {
                onceUsedSpaces.Remove(usedAvailableSpace);
                onceUsedSpaces.Sort(AvailableSpaceComparer);
                return true;
            }
            else
            {
                foreach (var availableSpace in availableSpaces)
                {
                    if (width <= availableSpace.Size.Width && height <= availableSpace.Size.Height)
                    {
                        device.StretchRectangle(texture,
                            null,
                            Texture,
                            new SharpDX.Mathematics.Interop.RawPoint((int)availableSpace.Position.X, (int)availableSpace.Position.Y));
                        if (availableSpace.Size.Width - width > 0)
                        {
                            availableSpaces.Add(new AvailableSpace(this,
                                new Point(availableSpace.Position.X + width, availableSpace.Position.Y),
                                new Size2(availableSpace.Size.Width - width, height)));
                        }
                        if (availableSpace.Size.Height - height > 0)
                        {
                            availableSpaces.Add(new AvailableSpace(this,
                                new Point(availableSpace.Position.X, availableSpace.Position.Y + height),
                                new Size2(availableSpace.Size.Width, availableSpace.Size.Height - height)));
                        }
                        uv = new Vector2(availableSpace.Position.X / (float)Size, availableSpace.Position.Y / (float)Size);
                        uvSize = new Vector2(width / (float)Size, height / (float)Size);
                        usedAvailableSpace = availableSpace;
                        usingAvailableSpace = new AvailableSpace(this, availableSpace.Position, new Size2(width, height));
                        break;
                    }
                }
                if (usedAvailableSpace != null)
                {
                    availableSpaces.Remove(usedAvailableSpace);
                    availableSpaces.Sort(AvailableSpaceComparer);
                    return true;
                }
            }
            return false;
        }

        public void Return(AvailableSpace availableSpace)
        {
            onceUsedSpaces.Add(availableSpace);
        }

        private int AvailableSpaceComparer(AvailableSpace a1, AvailableSpace a2)
        {
            return a1.Size.Width * a1.Size.Height - a2.Size.Width * a2.Size.Height;
        }
    }
}
