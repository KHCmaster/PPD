using Effect2D;
namespace Effect2DEditor.Command
{
    class ResetPathCommand : CommandBase
    {
        BezierCaliculator.BezierAnalyzer lastba;
        SelectedManager SManager;
        public ResetPathCommand(EffectManager manager, string exp, SelectedManager SManager)
            : base(manager, exp)
        {
            this.SManager = SManager;
        }

        public override void Undo()
        {
            SManager.Set.BAnalyzer = lastba;
        }

        public override void Execute()
        {
            lastba = SManager.Set.BAnalyzer;
            SManager.Set.BAnalyzer = null;
        }
    }
}
