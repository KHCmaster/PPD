using Effect2D;

namespace Effect2DEditor.Command
{
    class StateParameterChangeCommand(EffectManager manager, string exp, SelectedManager SManager, RatioType type, float value) : CommandBase(manager, exp)
    {
        readonly SelectedManager SManager = SManager;
        float previousvalue;
        readonly RatioType type = type;
        float value = value;

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
