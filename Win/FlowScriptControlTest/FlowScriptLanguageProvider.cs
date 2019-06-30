using FlowScriptControl.Classes;
using PPDConfiguration;

namespace FlowScriptControlTest
{
    class FlowScriptLanguageProvider : ILanguageProvider
    {
        private SettingReader lang;
        private SettingReader flowLang;

        public FlowScriptLanguageProvider(SettingReader lang, SettingReader flowLang)
        {
            this.lang = lang;
            this.flowLang = flowLang;
        }

        #region ILanguageProvider メンバ

        public string GetFlowSourceToolTipText(string key)
        {
            return flowLang.ReadString(key);
        }

        public string FormatError
        {
            get { return lang.ReadString("FormatError"); }
        }

        public string AddNode
        {
            get { return lang.ReadString("AddNode"); }
        }

        public string AddComment
        {
            get { return lang.ReadString("AddComment"); }
        }

        public string Cut
        {
            get { return lang.ReadString("Cut"); }
        }

        public string Copy
        {
            get { return lang.ReadString("Copy"); }
        }

        public string Paste
        {
            get { return lang.ReadString("Paste"); }
        }

        public string PasteWithLinks
        {
            get { return lang.ReadString("PasteWithLinks"); }
        }

        public string Delete
        {
            get { return lang.ReadString("Delete"); }
        }

        public string FitView
        {
            get { return lang.ReadString("FitView"); }
        }

        public string AddBoundComment
        {
            get { return lang.ReadString("AddBoundComment"); }
        }

        public string RemoveBoundComment
        {
            get { return lang.ReadString("RemoveBoundComment"); }
        }

        public string AddScope
        {
            get { return lang.ReadString("AddScope"); }
        }

        public string RemoveScope
        {
            get { return lang.ReadString("RemoveScope"); }
        }

        public string SelectInScope
        {
            get { return lang.ReadString("SelectInScope"); }
        }

        public string SelectAllInScope
        {
            get { return lang.ReadString("SelectAllInScope"); }
        }

        public string AddBreakPoint
        {
            get { return lang.ReadString("AddBreakPoint"); }
        }

        public string RemoveBreakPoint
        {
            get { return lang.ReadString("RemoveBreakPoint"); }
        }

        public string FindLinkedNode
        {
            get { return lang.ReadString("FindLinkedNode"); }
        }

        public string CopyNodeName
        {
            get { return lang.ReadString("CopyNodeName"); }
        }

        public string CopyPropertyName
        {
            get { return lang.ReadString("CopyPropertyName"); }
        }

        #endregion
    }
}
