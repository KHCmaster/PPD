using FlowScriptEngine;
using System;

namespace FlowScriptEnginePPD.FlowSourceObjects.Mod.ModInfo
{
    [ToolTipText("Mod_ModInfo_Value_Summary")]
    public partial class ValueFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "PPD.Mod.ModInfo.Value"; }
        }

        [ToolTipText("Mod_ModInfo_Value_Value")]
        public PPDFramework.Mod.ModInfo Value
        {
            private get;
            set;
        }

        [ToolTipText("Mod_ModInfo_Value_DisplayName")]
        public String DisplayName
        {
            get
            {
                SetValue(nameof(Value));
                if (Value != null)
                {
                    return Value.DisplayName;
                }
                return "";
            }
        }

        [ToolTipText("Mod_ModInfo_Value_AuthorName")]
        public string AuthorName
        {
            get
            {
                SetValue(nameof(Value));
                if (Value != null)
                {
                    return Value.AuthorName;
                }
                return "";
            }
        }

        [ToolTipText("Mod_ModInfo_Value_FileName")]
        public string FileName
        {
            get
            {
                SetValue(nameof(Value));
                if (Value != null)
                {
                    return Value.FileName;
                }
                return "";
            }
        }
    }
}
