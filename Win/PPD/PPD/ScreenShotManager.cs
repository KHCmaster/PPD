using PPDFramework;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace PPD
{
    class ScreenShotManager
    {
        const int baseWidth = 800;
        const int baseHeight = 450;

        private PPDDevice device;
        private List<ScreenShotInfo> infos;
        int updateCallCount;
        List<ScreenShotInfo> removeInfos;

        public ScreenShotManager(PPDDevice device)
        {
            this.device = device;
            infos = new List<ScreenShotInfo>();
            removeInfos = new List<ScreenShotInfo>();
        }

        public void Add(string filePath, Action<string> action)
        {
            infos.Add(new ScreenShotInfo(filePath, action, updateCallCount + 2));
        }

        public void Update()
        {
            foreach (var info in infos)
            {
                if (info.UpdateCount > updateCallCount)
                {
                    continue;
                }

                var fileName = Path.GetTempFileName();
                device.BackBufferToFile(fileName);
                var size = GetSize(fileName);
                if (size.Width != baseWidth || size.Height != baseHeight)
                {
                    using (Bitmap src = new Bitmap(fileName))
                    using (Bitmap bitmap = new Bitmap(baseWidth, baseHeight))
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                        Vector2 scale, offset;
                        if (size.Width / size.Height > 16f / 9)
                        {
                            scale = new Vector2((float)size.Height / baseHeight);
                            offset = new Vector2((float)(size.Width - scale.X * baseWidth) / 2, 0);
                        }
                        else
                        {
                            scale = new Vector2((float)size.Width / baseWidth);
                            offset = new Vector2(0, (float)(size.Height - scale.X * baseHeight) / 2);
                        }

                        g.DrawImage(src, new System.Drawing.RectangleF(0, 0, baseWidth, baseHeight), new System.Drawing.RectangleF(offset.X, offset.Y, scale.X * baseWidth, scale.Y * baseHeight), GraphicsUnit.Pixel);
                        fileName = Path.GetTempFileName();
                        bitmap.Save(fileName);
                    }
                }
                try
                {
                    File.Delete(info.FilePath);
                    File.Move(fileName, info.FilePath);

                    info.Action?.Invoke(info.FilePath);
                }
                catch (Exception)
                {
                }
                removeInfos.Add(info);
            }

            foreach (var removeInfo in removeInfos)
            {
                infos.Remove(removeInfo);
            }
            removeInfos.Clear();
            updateCallCount++;
        }

        private Size GetSize(string filePath)
        {
            using (Bitmap bitmap = new Bitmap(filePath))
            {
                return new Size(bitmap.Width, bitmap.Height);
            }
        }

        class ScreenShotInfo
        {
            public string FilePath
            {
                get;
                private set;
            }

            public Action<string> Action
            {
                get;
                private set;
            }

            public int UpdateCount
            {
                get;
                private set;
            }

            public ScreenShotInfo(string filePath, Action<string> action, int updateCount)
            {
                FilePath = filePath;
                Action = action;
                UpdateCount = updateCount;
            }

        }
    }
}
