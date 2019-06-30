using System.Reflection;

namespace FlowScriptEngine
{
    public class CustomMemberInfo<T> where T : MemberInfo
    {
        public CustomMemberInfo(T memberInfo, string toolTipText, string toolTipTextKey, string[] replacedNames)
        {
            MemberInfo = memberInfo;
            ToolTipText = toolTipText;
            ToolTipTextKey = toolTipTextKey;
            ReplacedNames = replacedNames;
        }

        public T MemberInfo
        {
            get;
            private set;
        }

        public string ToolTipText
        {
            get;
            private set;
        }

        public string ToolTipTextKey
        {
            get;
            private set;
        }

        public string[] ReplacedNames
        {
            get;
            private set;
        }
    }
}
