using Effect2D;
namespace Effect2DEditor.Command
{
    class ResetPathCommand(EffectManager manager, string exp, SelectedManager SManager) : CommandBase(manager, exp)
    {
        BezierCaliculator.BezierAnalyzer lastba;
        readonly SelectedManager SManager = SManager;

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
