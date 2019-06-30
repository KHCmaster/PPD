using Effect2D;

namespace Effect2DEditor.Command
{
    class DeleteStateCommand : CommandBase
    {
        int itemindex;
        int keyindex;
        int insertindex;
        EffectStateRatioSet previousset1;
        EffectStateRatioSet previousset2;
        public DeleteStateCommand(EffectManager manager, string exp, int itemindex, int keyindex)
            : base(manager, exp)
        {
            this.itemindex = itemindex;
            this.keyindex = keyindex;
        }
        public override void Undo()
        {
            IEffect effect = manager.Effects[itemindex];
            if (previousset1 != null && previousset2 != null)
            {
                effect.Sets.RemoveAt(insertindex);
                effect.Sets.Add(previousset1.StartFrame, previousset1);
                effect.Sets.Add(previousset2.StartFrame, previousset2);
            }
            else if (previousset1 != null)
            {
                manager.Sets.Add(previousset1.StartFrame, previousset1);
            }
            else if (previousset2 != null)
            {
                manager.Sets.Add(previousset1.StartFrame, previousset1);
            }
        }

        public override void Execute()
        {
            IEffect effect = manager.Effects[itemindex];
            for (int i = 0; i < effect.Sets.Count; i++)
            {
                EffectStateRatioSet set = effect.Sets.Values[i];
                if (set.StartFrame == keyindex)
                {
                    previousset1 = (EffectStateRatioSet)set.CloneExceptState();
                    effect.Sets.RemoveAt(i);
                    break;
                }
                else if (set.EndFrame == keyindex)
                {
                    if (i == effect.Sets.Count - 1)
                    {
                        previousset1 = (EffectStateRatioSet)set.CloneExceptState();
                        effect.Sets.RemoveAt(i);
                    }
                    else
                    {
                        EffectStateRatioSet nextset = effect.Sets.Values[i + 1];
                        previousset1 = (EffectStateRatioSet)set.CloneExceptState();
                        previousset2 = (EffectStateRatioSet)nextset.CloneExceptState();
                        effect.Sets.RemoveAt(i);
                        effect.Sets.RemoveAt(i);
                        nextset.StartFrame = set.StartFrame;
                        nextset.StartState = set.StartState;
                        effect.Sets.Add(nextset.StartFrame, nextset);
                        insertindex = i;
                    }
                    break;
                }
            }
        }
    }
}
