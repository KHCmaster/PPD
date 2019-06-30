using FlowScriptControl.Classes;
using PPDConfiguration;

namespace PPDEditor
{
    class FlowScriptLanguageProvider : ILanguageProvider
    {
        private SettingReader flowLang;

        public FlowScriptLanguageProvider(SettingReader flowLang)
        {
            this.flowLang = flowLang;
        }

        #region ILanguageProvider メンバ

        public string GetFlowSourceToolTipText(string key)
        {
            return flowLang.ReadString(key);
        }

        public string FormatError
        {
            get { return Utility.Language["FormatError"]; }
        }

        public string AddNode
        {
            get { return Utility.Language["AddNode"]; }
        }

        public string AddComment
        {
            get { return Utility.Language["AddComment"]; }
        }

        public string Cut
        {
            get { return Utility.Language["Cut"]; }
        }

        public string Copy
        {
            get { return Utility.Language["Copy"]; }
        }

        public string Paste
        {
            get { return Utility.Language["Paste"]; }
        }

        public string PasteWithLinks
        {
            get { return Utility.Language["PasteWithLinks"]; }
        }

        public string Delete
        {
            get { return Utility.Language["Delete"]; }
        }

        public string FitView
        {
            get { return Utility.Language["FitView"]; }
        }

        public string AddBoundComment
        {
            get { return Utility.Language["AddBoundComment"]; }
        }

        public string RemoveBoundComment
        {
            get { return Utility.Language["RemoveBoundComment"]; }
        }

        public string AddScope
        {
            get { return Utility.Language["AddScope"]; }
        }

        public string RemoveScope
        {
            get { return Utility.Language["RemoveScope"]; }
        }

        public string SelectInScope
        {
            get { return Utility.Language["SelectInScope"]; }
        }

        public string SelectAllInScope
        {
            get { return Utility.Language["SelectAllInScope"]; }
        }

        public string AddBreakPoint
        {
            get { return Utility.Language["AddBreakPoint"]; }
        }

        public string RemoveBreakPoint
        {
            get { return Utility.Language["RemoveBreakPoint"]; }
        }

        public string FindLinkedNode
        {
            get { return Utility.Language["FindLinkedNode"]; }
        }

        public string CopyNodeName
        {
            get { return Utility.Language["CopyNodeName"]; }
        }

        public string CopyPropertyName
        {
            get { return Utility.Language["CopyPropertyName"]; }
        }

        #endregion
    }
}
