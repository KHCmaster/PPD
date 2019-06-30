using System.Collections.Generic;

namespace PPDEditorCommon.Dialog.Model
{
    public class ScriptManagerInfo
    {
        List<ScriptInfo> scripts;

        public ScriptInfo[] Scripts
        {
            get
            {
                return scripts.ToArray();
            }
        }

        public ScriptManagerInfo()
        {
            scripts = new List<ScriptInfo>();
        }

        public void Add(ScriptInfo scriptInfo)
        {
            scripts.Add(scriptInfo);
        }
    }

    public class ScriptInfo
    {
        public string Src
        {
            get;
            private set;
        }

        public string Dest
        {
            get;
            private set;
        }

        public ScriptInfo(string src, string dest)
        {
            Src = src;
            Dest = dest;
        }
    }
}
