using PPDFramework.Sprites;
using PPDPack;
using System;
using System.IO;

namespace PPDCoreModel
{
    class PPDPackSpriteManager : PPDPackSpriteManagerBase
    {
        DateTime lastWriteTime;

        public PPDPackSpriteManager(PackReader packReader, string[] imageNames, string spriteDirName, DateTime lastWriteTime)
            : base(packReader, imageNames, spriteDirName)
        {
            this.lastWriteTime = lastWriteTime;
        }

        protected override bool CheckUpdate(ImageInfo[] infos)
        {
            return Directory.GetLastWriteTime(ImageDir) == lastWriteTime;
        }

        protected override void OnPacked()
        {
            base.OnPacked();
            Directory.SetLastWriteTime(ImageDir, lastWriteTime);
        }
    }
}
