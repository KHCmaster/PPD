using FlowScriptEngine;
using PPDCoreModel;
using PPDFramework;
using SharpDX;
using System.Collections.Generic;

namespace FlowScriptEnginePPD.FlowSourceObjects.Graphics.Number
{
    [ToolTipText("Number_Value_Summary")]
    public partial class ValueFlowSourceObject : ExecutableFlowSourceObject
    {
        private ScriptResourceManager resourceManager;

        public override string Name
        {
            get { return "PPD.Graphics.Number.Value"; }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (this.Manager.Items.ContainsKey("ResourceManager"))
            {
                resourceManager = this.Manager.Items["ResourceManager"] as ScriptResourceManager;
            }
        }

        [ToolTipText("Number_Value_Object")]
        public NumberPictureObject Object
        {
            get;
            private set;
        }

        [ToolTipText("Number_Value_Path")]
        public string Path
        {
            private get;
            set;
        }

        [ToolTipText("Number_Value_Position")]
        public Vector2 Position
        {
            private get;
            set;
        }

        [ToolTipText("Number_Value_Alignment")]
        public PPDFramework.Alignment Alignment
        {
            private get;
            set;
        }

        [ToolTipText("Number_Value_MaxDigit")]
        public int MaxDigit
        {
            private get;
            set;
        }

        public override void In(FlowEventArgs e)
        {
            SetValue(nameof(Path));
            SetValue(nameof(Position));
            SetValue(nameof(Alignment));
            SetValue(nameof(MaxDigit));

            if (resourceManager != null)
            {
                Object = (NumberPictureObject)resourceManager.GetResource(Path, PPDCoreModel.Data.ResourceKind.Number, new Dictionary<string, object>
                {
                    {"Position",Position},
                    {"Alignment",Alignment},
                    {"MaxDigit",MaxDigit}
                });
                OnSuccess();
            }
            else
            {
                OnFailed();
            }
        }
    }
}
