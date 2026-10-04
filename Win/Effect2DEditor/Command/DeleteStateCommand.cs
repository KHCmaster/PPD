using Effect2D;

namespace Effect2DEditor.Command
{
    class DeleteStateCommand(EffectManager manager, string exp, int itemindex, int keyindex) : CommandBase(manager, exp)
    {
        readonly int itemindex = itemindex;
        readonly int keyindex = keyindex;
        int insertindex;
        EffectStateRatioSet previousset1;
        EffectStateRatioSet previousset2;

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
                // 最初か最後のステートを消したときは、消した組をエフェクトに戻す（以前はエフェクトでなく EffectManager に足していた）
                effect.Sets.Add(previousset1.StartFrame, previousset1);
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
