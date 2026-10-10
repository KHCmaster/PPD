using PPDFramework.Sprites;
using PPDPack;
using System;
using System.IO;

namespace PPDCoreModel
{
    class PPDPackSpriteManager(PackReader packReader, string[] imageNames, string spriteDirName, DateTime lastWriteTime) : PPDPackSpriteManagerBase(packReader, imageNames, spriteDirName)
    {
        readonly DateTime lastWriteTime = lastWriteTime;

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
