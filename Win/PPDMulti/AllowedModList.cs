using PPDFramework.Web;
using System.Collections.Generic;

namespace PPDMulti
{
    class AllowedModList
    {
        private WebModInfo[] webMods;
        private string[] allowedModIds;
        public HashSet<string> allowedFileHashes;

        public WebModInfo[] WebMods
        {
            get { return webMods; }
            set
            {
                webMods = value;
                UpdateCache();
            }
        }

        public string[] AllowedModIds
        {
            get { return allowedModIds; }
            set
            {
                allowedModIds = value;
                UpdateCache();
            }
        }

        public bool IsAllowed(string fileHash)
        {
            if (allowedFileHashes == null)
            {
                return false;
            }
            return allowedFileHashes.Contains(fileHash);
        }

        private void UpdateCache()
        {
            if (webMods == null || allowedModIds == null)
            {
                return;
            }
            var tempAllowedMods = new HashSet<string>(allowedModIds);
            allowedFileHashes = new HashSet<string>();
            foreach (var webMod in webMods)
            {
                if (!tempAllowedMods.Contains(webMod.Id))
                {
                    continue;
                }
                foreach (var detail in webMod.Details)
                {
                    allowedFileHashes.Add(detail.Hash);
                }
            }
        }
    }
}
