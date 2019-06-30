using BezierCaliculator;
using Effect2D;
using System.Drawing;

namespace Effect2DEditor.Command
{
    class BezierPositionChangeCommand : CommandBase
    {
        PointF laststartpoint;
        PointF lastendpoint;
        bool lastbcpsavailable;
        BezierControlPoint[] lastbcps;
        BezierControlPoint[] bcps;
        SelectedManager SManager;
        public BezierPositionChangeCommand(EffectManager manager, string exp, SelectedManager SManager, BezierControlPoint[] bcps)
            : base(manager, exp)
        {
            this.SManager = SManager;
            this.bcps = bcps;
        }
        public BezierControlPoint[] BCPS
        {
            get
            {
                return bcps;
            }
            set
            {
                bcps = value;
            }
        }
        public override void Undo()
        {
            if (lastbcpsavailable)
            {
                SManager.Set.BAnalyzer = new BezierAnalyzer(lastbcps);
            }
            else
            {
                SManager.Set.BAnalyzer = null;
            }
            SManager.Set.StartState.X = laststartpoint.X;
            SManager.Set.StartState.Y = laststartpoint.Y;
            SManager.Set.EndState.X = lastendpoint.X;
            SManager.Set.EndState.Y = lastendpoint.Y;
        }

        public override void Execute()
        {
            lastbcpsavailable = SManager.Set.IsBezierPosition;
            laststartpoint = new PointF(SManager.Set.StartState.X, SManager.Set.StartState.Y);
            lastendpoint = new PointF(SManager.Set.EndState.X, SManager.Set.EndState.Y);
            if (SManager.Set.IsBezierPosition)
            {
                lastbcps = SManager.Set.BAnalyzer.BCPS;
            }
            SManager.Set.BAnalyzer = new BezierAnalyzer(bcps);
            SManager.Set.StartState.X = bcps[0].Second.X;
            SManager.Set.StartState.Y = bcps[0].Second.Y;
            SManager.Set.EndState.X = bcps[bcps.Length - 1].Second.X;
            SManager.Set.EndState.Y = bcps[bcps.Length - 1].Second.Y;
        }
        public void SpecialExecute()
        {
            SManager.Set.BAnalyzer = new BezierAnalyzer(bcps);
            SManager.Set.StartState.X = bcps[0].Second.X;
            SManager.Set.StartState.Y = bcps[0].Second.Y;
            SManager.Set.EndState.X = bcps[bcps.Length - 1].Second.X;
            SManager.Set.EndState.Y = bcps[bcps.Length - 1].Second.Y;
        }
    }
}
