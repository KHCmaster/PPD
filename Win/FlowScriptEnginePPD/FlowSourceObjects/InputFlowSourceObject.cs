using FlowScriptEngine;

namespace FlowScriptEnginePPD.FlowSourceObjects
{
    [ToolTipText("Input_Summary")]
    public partial class InputFlowSourceObject : FlowSourceObjectBase
    {
        [ToolTipText("Input_Out")]
        public event FlowEventHandler Out;

        public override string Name
        {
            get { return "PPD.Input"; }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            Manager.RegisterCallBack("PPD.Input", CallBack);
        }

        private void CallBack(object[] values)
        {
            SetValue(nameof(HandleAny));
            SetValue(nameof(Button));

            if (HandleAny || Button == (PPDCoreModel.Data.MarkType)values[0])
            {
                Button = (PPDCoreModel.Data.MarkType)values[0];
                PressCount = (int)values[1];
                Pressed = (bool)values[2];
                Released = (bool)values[3];

                FireEvent(Out);
            }
        }

        [ToolTipText("Input_Button")]
        public PPDCoreModel.Data.MarkType Button
        {
            get;
            set;
        }

        [ToolTipText("Input_HandleAny")]
        public bool HandleAny
        {
            private get;
            set;
        }

        [ToolTipText("Input_Pressed")]
        public bool Pressed
        {
            get;
            private set;
        }

        [ToolTipText("Input_Released")]
        public bool Released
        {
            get;
            private set;
        }

        [ToolTipText("Input_PressCount")]
        public int PressCount
        {
            get;
            private set;
        }
    }
}
