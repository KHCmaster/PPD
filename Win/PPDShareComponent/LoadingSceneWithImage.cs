using PPDFramework;
using SharpDX;
using System;
using System.IO;
using System.Linq;

namespace PPDShareComponent
{
    public abstract class LoadingSceneWithImage : LoadingBase
    {
        string[] imageFiles;
        string[] allowedImageExtensions = { ".png", ".gif", ".jpg", ".jpeg" };
        PictureObject currentImage;

        protected LoadingSceneWithImage(PPDDevice device) : base(device)
        {
            if (Directory.Exists("img\\loading"))
            {
                imageFiles = Directory.GetFiles("img\\loading").Where(f => Array.IndexOf(allowedImageExtensions, Path.GetExtension(f)) >= 0).ToArray();
            }
        }

        public override void EnterLoading()
        {
            if (imageFiles != null && imageFiles.Length > 0)
            {
                if (currentImage != null)
                {
                    RemoveChild(currentImage);
                    currentImage.Dispose();
                    currentImage = null;
                }
                var index = new Random().Next(imageFiles.Length);
                currentImage = new PictureObject(device, ResourceManager, PathObject.Absolute(imageFiles[index]), true)
                {
                    Position = new Vector2(400, 225)
                };
                var aspect = currentImage.Width / currentImage.Height;
                var displayAspect = 16 / 9f;
                Vector2 scale;
                if (aspect > displayAspect)
                {
                    scale = new Vector2(800 / currentImage.Width);
                }
                else
                {
                    scale = new Vector2(450 / currentImage.Height);
                }
                currentImage.Scale = scale;
                AddChild(currentImage);
            }
        }
    }
}