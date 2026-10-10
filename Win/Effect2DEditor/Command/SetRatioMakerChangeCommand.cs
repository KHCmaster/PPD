using Effect2D;

namespace Effect2DEditor.Command
{
    class SetRatioMakerChangeCommand(EffectManager manager, string exp, SelectedManager SManager, RatioType type, IRatioMaker maker) : CommandBase(manager, exp)
    {
        readonly SelectedManager SManager = SManager;
        readonly RatioType type = type;
        IRatioMaker previousmaker;
        readonly IRatioMaker maker = maker;

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
