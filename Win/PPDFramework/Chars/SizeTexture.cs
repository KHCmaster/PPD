using PPDFramework.Texture;
using System.Numerics;
using PPDMath;
using Point = Vortice.Mathematics.Int2;
using Size2 = Vortice.Mathematics.SizeI;
using System;
using System.Collections.Generic;

namespace PPDFramework.Chars
{
    class SizeTexture : DisposableComponent
    {
        const int Size = 1024;
        readonly PPDDevice device;
        readonly List<AvailableSpace> availableSpaces;
        readonly List<AvailableSpace> onceUsedSpaces;

        public static Vector2 HalfPixel
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
            Texture = TextureFactoryManager.Factory.Create(device, Size, Size, 1, true);
            availableSpaces =
            [
                new(this, Point.Zero, new Size2(Size, Size))
            ];
            onceUsedSpaces = [];
        }

        public bool Write(TextureBase texture, int width, int height, out Vector2 uv, out Vector2 uvSize, out AvailableSpace usingAvailableSpace)
        {
            return Write(position => device.StretchRectangle(texture, null, Texture, position), width, height, out uv, out uvSize, out usingAvailableSpace);
        }

        /// <summary>
        /// 画素（BGRA、幅 × 4 バイトの行）を書き込みます。書き込むのは srcWidth × srcHeight で、width × height の領域を確保します。
        /// </summary>
        public bool WritePixels(byte[] pixels, int srcWidth, int srcHeight, int width, int height, out Vector2 uv, out Vector2 uvSize, out AvailableSpace usingAvailableSpace)
        {
            return Write(position => ((PPDFramework.Texture.DX11.Texture)Texture).WritePixels(pixels, srcWidth, srcHeight, position), width, height, out uv, out uvSize, out usingAvailableSpace);
        }

        // 文字のテクスチャは、読み込みのスレッドや文字を作るスレッドからも書き込まれるので、空き領域の操作はロックする
        private bool Write(Action<Point> copy, int width, int height, out Vector2 uv, out Vector2 uvSize, out AvailableSpace usingAvailableSpace)
        {
            lock (availableSpaces)
            {
                uv = Vector2.Zero;
                uvSize = Vector2.Zero;
                usingAvailableSpace = null;
                AvailableSpace usedAvailableSpace = null;
                foreach (var availableSpace in onceUsedSpaces)
                {
                    if (width == availableSpace.Size.Width && height == availableSpace.Size.Height)
                    {
                        copy(new Point((int)availableSpace.Position.X, (int)availableSpace.Position.Y));
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
                foreach (var availableSpace in availableSpaces)
                {
                    if (width <= availableSpace.Size.Width && height <= availableSpace.Size.Height)
                    {
                        copy(new Point((int)availableSpace.Position.X, (int)availableSpace.Position.Y));
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
                return false;
            }
        }

        public void Return(AvailableSpace availableSpace)
        {
            lock (availableSpaces)
            {
                onceUsedSpaces.Add(availableSpace);
            }
        }

        private int AvailableSpaceComparer(AvailableSpace a1, AvailableSpace a2)
        {
            return a1.Size.Width * a1.Size.Height - a2.Size.Width * a2.Size.Height;
        }

        protected override void DisposeResource()
        {
            base.DisposeResource();
            Texture?.Dispose();
            Texture = null;
        }
    }
}
