using FlowScriptDrawControl.Control;

namespace FlowScriptDrawControl.Command
{
    class AddSourceCommand : CommandBase
    {
        private FlowAreaControl areaControl;

        public SourceControl SourceControl
        {
            get;
            private set;
        }

        public AddSourceCommand(FlowAreaControl areaControl, SourceControl sourceControl)
        {
            this.areaControl = areaControl;
            SourceControl = sourceControl;
        }

        public override void Execute()
        {
            this.areaControl.controlCanvas.Children.Add(SourceControl);
        }

        public override void Undo()
        {
            this.areaControl.controlCanvas.Children.Remove(SourceControl);
        }
    }
}
