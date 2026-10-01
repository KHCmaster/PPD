using FlowScriptDrawControl.Control;

namespace FlowScriptDrawControl.Command
{
    class AddSourceCommand(FlowAreaControl areaControl, SourceControl sourceControl) : CommandBase
    {
        private readonly FlowAreaControl areaControl = areaControl;

        public SourceControl SourceControl
        {
            get;
            private set;
        } = sourceControl;

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
