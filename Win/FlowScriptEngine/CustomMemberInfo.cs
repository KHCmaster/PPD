using System.Reflection;

namespace FlowScriptEngine
{
    public class CustomMemberInfo<T>(T memberInfo, string toolTipText, string toolTipTextKey, string[] replacedNames) where T : MemberInfo
    {
        public T MemberInfo
        {
            get;
            private set;
        } = memberInfo;

        public string ToolTipText
        {
            get;
            private set;
        } = toolTipText;

        public string ToolTipTextKey
        {
            get;
            private set;
        } = toolTipTextKey;

        public string[] ReplacedNames
        {
            get;
            private set;
        } = replacedNames;
    }
}
