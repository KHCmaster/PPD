using Effect2D;

namespace Effect2DEditor.Command
{
    class StateParameterChangeCommand : CommandBase
    {
        SelectedManager SManager;
        float previousvalue;
        RatioType type;
        float value;
        public StateParameterChangeCommand(EffectManager manager, string exp, SelectedManager SManager, RatioType type, float value)
            : base(manager, exp)
        {
            this.SManager = SManager;
            this.type = type;
            this.value = value;
        }
        public float Value
        {
            get { return value; }
            set { this.value = value; }
        }
        public RatioType Type
        {
            get { return type; }
        }


        public override void Undo()
        {
            EffectStateStructure state = SManager.State;
            state[type] = previousvalue;
        }

        public override void Execute()
        {
            EffectStateStructure state = SManager.State;
            previousvalue = state[type];
            state[type] = value;
        }
        public void SpecialExecute()
        {
            EffectStateStructure state = SManager.State;
            state[type] = value;
        }
    }
}
