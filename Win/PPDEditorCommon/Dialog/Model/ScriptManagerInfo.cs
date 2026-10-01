using System.Collections.Generic;

namespace PPDEditorCommon.Dialog.Model
{
    public class ScriptManagerInfo
    {
        readonly List<ScriptInfo> scripts;

        public ScriptInfo[] Scripts
        {
            get
            {
                return [.. scripts];
            }
        }

        public ScriptManagerInfo()
        {
            scripts = [];
        }

        public void Add(ScriptInfo scriptInfo)
        {
            scripts.Add(scriptInfo);
        }
    }

    public class ScriptInfo(string src, string dest)
    {
        public string Src
        {
            get;
            private set;
        } = src;

        public string Dest
        {
            get;
            private set;
        } = dest;
    }
}
