using BezierCaliculator;
using Effect2D;
using System.Drawing;
using System.Drawing.Drawing2D;


namespace Effect2DEditor.Command
{
    class AddStateCommand : CommandBase
    {
        bool iskey;
        int itemindex;
        int keyindex;
        bool islast;
        bool twoframeinserted;
        EffectStateRatioSet previousset;
        int setindex;
        public AddStateCommand(EffectManager manager, string exp, bool IsKey, int itemindex, int keyindex)
            : base(manager, exp)
        {
            iskey = IsKey;
            this.itemindex = itemindex;
            this.keyindex = keyindex;
        }
        public override void Undo()
        {
            IEffect effect = manager.Effects[itemindex];
            if (islast)
            {
                effect.Sets.RemoveAt(effect.Sets.Count - 1);
            }
            else
            {
                if (iskey)
                {
                    if (twoframeinserted) effect.Sets.RemoveAt(setindex);
                    effect.Sets.RemoveAt(setindex);
                    effect.Sets.RemoveAt(setindex);
                    effect.Sets.Add(previousset.StartFrame, previousset);
                }
                else
                {
                    effect.Sets.RemoveAt(setindex);
                    effect.Sets.RemoveAt(setindex);
                    effect.Sets.Add(previousset.StartFrame, previousset);
                }
            }
        }

        public override void Execute()
        {
            if (iskey) AddKeyState();
            else AddNormalState();
        }
        private void AddKeyState()
        {
            EffectStateRatioSet lastset = manager.Effects[itemindex].Sets.Values[manager.Effects[itemindex].Sets.Count - 1];
            int lastframe = lastset.EndFrame;
            if (lastframe < keyindex)
            {
                var newset = new EffectStateRatioSet
                {
                    StartState = lastset.EndState,
                    StartFrame = lastset.EndFrame,
                    EndState = (EffectStateStructure)lastset.EndState.Clone(),
                    EndFrame = keyindex
                };
                foreach (RatioType type in Effect2D.Utility.RatioTypeArray)
                {
                    newset[type] = new ConstantRatioMaker();
                }
                manager.Effects[itemindex].Sets.Add(newset.StartFrame, newset);
                islast = true;
            }
            else
            {
                IEffect effect = manager.Effects[itemindex];
                var set = GetSetFromFrameIndex(effect, keyindex);
                setindex = effect.Sets.IndexOfValue(set);
                previousset = (EffectStateRatioSet)set.CloneExceptState();
                twoframeinserted = set.StartFrame + 1 != keyindex;
                if (set != null)
                {
                    effect.Update(keyindex - 1, new EffectStateStructure());
                    var temp1 = (EffectStateStructure)effect.CurrentState.Clone();
                    effect.Update(keyindex, new EffectStateStructure());
                    var temp2 = (EffectStateStructure)effect.CurrentState.Clone();
                    var newset1 = new EffectStateRatioSet();
                    var newset2 = new EffectStateRatioSet();
                    foreach (RatioType type in Effect2D.Utility.RatioTypeArray)
                    {
                        if (set[type] is LinearRatioMaker)
                        {
                            newset2[type] = new LinearRatioMaker();
                        }
                        else if (set[type] is ConstantRatioMaker)
                        {
                            newset2[type] = new ConstantRatioMaker();
                        }
                        else if (set[type] is BezierRatioMaker)
                        {
                            var brm = set[type] as BezierRatioMaker;
                            GetDevidedBezierFromX(brm.Analyzer.BCPS, (float)(keyindex - 1 - set.StartFrame) / (set.EndFrame - set.StartFrame), out BezierControlPoint[] bcps1, out BezierControlPoint[] bcps2);
                            set[type] = new BezierRatioMaker(bcps1[0], bcps1[1]);
                            GetDevidedBezierFromX(bcps2, (float)1 / (set.EndFrame - keyindex + 1), out bcps1, out BezierControlPoint[] bcps3);
                            newset2[type] = new BezierRatioMaker(bcps3[0], bcps3[1]);
                        }
                        newset1[type] = new ConstantRatioMaker();
                    }
                    if (set.IsBezierPosition)
                    {
                        BezierControlPoint[] bcps1, bcps2;
                        if (set.StartFrame + 1 != keyindex)
                        {
                            set.BAnalyzer.GetDevidedBeziers(BezierAnalyzer.MaxRatio * (float)(keyindex - 1 - set.StartFrame) / (set.EndFrame - set.StartFrame), out bcps1, out bcps2);
                            set.BAnalyzer = new BezierAnalyzer(bcps1);
                            newset1.BAnalyzer = new BezierAnalyzer(bcps2);
                        }
                        else
                        {
                            newset1.BAnalyzer = set.BAnalyzer;
                        }
                        newset1.BAnalyzer.GetDevidedBeziers(BezierAnalyzer.MaxRatio * (float)1 / (set.EndFrame - keyindex + 1), out bcps1, out bcps2);
                        newset1.BAnalyzer = new BezierAnalyzer(bcps1);
                        newset2.BAnalyzer = new BezierAnalyzer(bcps2);
                    }
                    newset2.EndFrame = set.EndFrame;
                    newset2.EndState = set.EndState;
                    set.EndFrame = keyindex - 1;
                    set.EndState = temp1;
                    newset1.StartFrame = keyindex - 1;
                    newset1.StartState = set.EndState;
                    newset1.EndFrame = keyindex;
                    newset1.EndState = temp2;
                    newset2.StartFrame = newset1.EndFrame;
                    newset2.StartState = newset1.EndState;
                    if (set.StartFrame + 1 == keyindex) effect.Sets.Remove(set.StartFrame);
                    effect.Sets.Add(newset1.StartFrame, newset1);
                    effect.Sets.Add(newset2.StartFrame, newset2);
                }
                islast = false;
            }
        }
        private void AddNormalState()
        {
            EffectStateRatioSet lastset = manager.Effects[itemindex].Sets.Values[manager.Effects[itemindex].Sets.Count - 1];
            int lastframe = lastset.EndFrame;
            if (lastframe < keyindex)
            {
                var set = new EffectStateRatioSet
                {
                    StartState = lastset.EndState,
                    StartFrame = lastset.EndFrame,
                    EndState = (EffectStateStructure)lastset.EndState.Clone(),
                    EndFrame = keyindex
                };
                set.SetDefaultMaker();
                manager.Effects[itemindex].Sets.Add(set.StartFrame, set);
                islast = true;
            }
            else
            {
                IEffect effect = manager.Effects[itemindex];
                var set = GetSetFromFrameIndex(effect, keyindex);
                setindex = effect.Sets.IndexOfValue(set);
                previousset = (EffectStateRatioSet)set.CloneExceptState();
                if (set != null)
                {
                    var newset = new EffectStateRatioSet();
                    var bratio = set.GetRatio(RatioType.BezierPosition, keyindex);
                    foreach (RatioType type in Effect2D.Utility.RatioTypeArray)
                    {
                        if (set[type] is LinearRatioMaker)
                        {
                            newset[type] = new LinearRatioMaker();
                        }
                        else if (set[type] is ConstantRatioMaker)
                        {
                            newset[type] = new ConstantRatioMaker();
                        }
                        else if (set[type] is BezierRatioMaker)
                        {
                            var brm = set[type] as BezierRatioMaker;
                            GetDevidedBezierFromX(brm.Analyzer.BCPS, (float)(keyindex - set.StartFrame) / (set.EndFrame - set.StartFrame), out BezierControlPoint[] bcps1, out BezierControlPoint[] bcps2);
                            newset[type] = new BezierRatioMaker(bcps1[0], bcps1[1]);
                            set[type] = new BezierRatioMaker(bcps2[0], bcps2[1]);
                        }
                    }
                    effect.Update(keyindex, new EffectStateStructure());
                    if (set.IsBezierPosition)
                    {
                        //set.BAnalyzer.GetDevidedBeziers(BezierAnalyzer.MaxRatio * (float)(keyindex - set.StartFrame) / (set.EndFrame - set.StartFrame), out bcps1, out bcps2);
                        set.BAnalyzer.GetDevidedBeziers(BezierAnalyzer.MaxRatio * (1 - bratio), out BezierControlPoint[] bcps1, out BezierControlPoint[] bcps2);
                        newset.BAnalyzer = new BezierAnalyzer(bcps1);
                        set.BAnalyzer = new BezierAnalyzer(bcps2);
                    }
                    effect.Sets.RemoveAt(setindex);
                    newset.StartFrame = set.StartFrame;
                    newset.StartState = set.StartState;
                    newset.EndFrame = keyindex;
                    newset.EndState = (EffectStateStructure)effect.CurrentState.Clone();
                    set.StartState = newset.EndState;
                    set.StartFrame = keyindex;
                    effect.Sets.Add(newset.StartFrame, newset);
                    effect.Sets.Add(set.StartFrame, set);
                }
                islast = false;
            }
        }
        private void GetDevidedBezierFromX(BezierControlPoint[] bcps, float ratio, out BezierControlPoint[] out1, out BezierControlPoint[] out2)
        {
            var ba = new BezierAnalyzer(bcps);
            float x = BezierRatioMaker.MaxLength * ratio;
            var y = ba.GetYFromX(x);
            out1 = null;
            out2 = null;
            if (BezierCaliculate.OnBezeier(bcps[0].Second, bcps[0].Third, bcps[1].First, bcps[1].Second, new PointF(x, y), out float outratio))
            {
                BezierCaliculate.GetDevidedBeziers(bcps[0], bcps[1], outratio, out BezierControlPoint bcp1, out BezierControlPoint bcp2, out BezierControlPoint bcp3);
                var bcp22 = bcp2.Clone();
                bcp2.ValidThird = false;
                bcp22.ValidFirst = false;
                ScaleAt(new PointF(0, BezierRatioMaker.MaxLength), new PointF(BezierRatioMaker.MaxLength / bcp2.Second.X, BezierRatioMaker.MaxLength / (BezierRatioMaker.MaxLength - bcp2.Second.Y)), bcp1);
                ScaleAt(new PointF(0, BezierRatioMaker.MaxLength), new PointF(BezierRatioMaker.MaxLength / bcp2.Second.X, BezierRatioMaker.MaxLength / (BezierRatioMaker.MaxLength - bcp2.Second.Y)), bcp2);
                bcp2.Second = new PointF(128, 0);
                ScaleAt(new PointF(BezierRatioMaker.MaxLength, 0), new PointF(BezierRatioMaker.MaxLength / (BezierRatioMaker.MaxLength - bcp22.Second.X), BezierRatioMaker.MaxLength / bcp22.Second.Y), bcp3);
                ScaleAt(new PointF(BezierRatioMaker.MaxLength, 0), new PointF(BezierRatioMaker.MaxLength / (BezierRatioMaker.MaxLength - bcp22.Second.X), BezierRatioMaker.MaxLength / bcp22.Second.Y), bcp22);
                bcp22.Second = new PointF(0, 128);
                out1 = new BezierControlPoint[] { bcp1, bcp2 };
                out2 = new BezierControlPoint[] { bcp22, bcp3 };
            }
        }
        private void ScaleAt(PointF basepoint, PointF Scale, BezierControlPoint bcp)
        {
            var mat = new Matrix();
            mat.Translate(basepoint.X, basepoint.Y);
            mat.Scale(Scale.X, Scale.Y);
            mat.Translate(-basepoint.X, -basepoint.Y);
            var temp = new PointF[] { bcp.First, bcp.Second, bcp.Third };
            mat.TransformPoints(temp);
            bcp.Second = temp[1];
            if (bcp.ValidFirst) bcp.First = temp[0];
            if (bcp.ValidThird) bcp.Third = temp[2];
        }
        private EffectStateRatioSet GetSetFromFrameIndex(IEffect effect, int frameindex)
        {
            if (effect.StartFrame > frameindex || frameindex > effect.StartFrame + effect.FrameLength) return null;
            float num = frameindex - effect.StartFrame;
            var index = BinaryFinder.FindNearest(effect.Sets.Keys, ref num);
            return effect.Sets.Values[index];
        }
    }
}
