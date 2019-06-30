using Effect2D;

namespace Effect2DEditor.Command
{
    class SetRatioMakerChangeCommand : CommandBase
    {
        SelectedManager SManager;
        RatioType type;
        IRatioMaker previousmaker;
        IRatioMaker maker;
        public SetRatioMakerChangeCommand(EffectManager manager, string exp, SelectedManager SManager, RatioType type, IRatioMaker maker)
            : base(manager, exp)
        {
            this.SManager = SManager;
            this.maker = maker;
            this.type = type;
        }

        public override void Undo()
        {
            SManager.Set[type] = previousmaker;
        }

        public override void Execute()
        {
            previousmaker = SManager.Set[type];
            SManager.Set[type] = maker;
        }
    }
}
