using Effect2D;

namespace Effect2DEditor.Models
{
    /// <summary>
    /// 並べた画像を 1 枚ずつ順に表示するアニメーションを作ります（今のステートは消す）。
    /// </summary>
    public static class AnimationBuilder
    {
        /// <summary>
        /// アニメーションを作ります。
        /// </summary>
        /// <param name="framecount">1 枚の画像を表示するフレーム数。</param>
        /// <param name="withreverse">最後まで表示したあと、逆の順にも表示するかどうか。</param>
        public static void Build(EffectManager effectManager, int framecount, bool withreverse)
        {
            var effects = effectManager.Effects;
            bool first = true;
            if (!withreverse)
            {
                for (int i = 0; i < effects.Count; i++)
                {
                    IEffect effect = effects[effects.Count - 1 - i];
                    effect.Sets.Clear();
                    var ess1 = new EffectStateStructure();
                    var ess2 = new EffectStateStructure();
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        ess1.Alpha = 0;
                        AddSet(effect, 0, i * framecount, ess1, ess2);
                    }
                    var ess3 = new EffectStateStructure
                    {
                        Alpha = 0
                    };
                    AddSet(effect, i * framecount, (i + 1) * framecount, ess2, ess3);
                    if (i != effects.Count - 1)
                    {
                        var ess4 = new EffectStateStructure
                        {
                            Alpha = 0
                        };
                        AddSet(effect, (i + 1) * framecount, effects.Count * framecount, ess3, ess4);
                    }
                }
            }
            else
            {
                for (int i = 0; i < effects.Count; i++)
                {
                    IEffect effect = effects[effects.Count - 1 - i];
                    effect.Sets.Clear();
                    var ess1 = new EffectStateStructure();
                    var ess2 = new EffectStateStructure();
                    if (!first)
                    {
                        ess1.Alpha = 0;
                        AddSet(effect, 0, i * framecount, ess1, ess2);
                    }
                    var ess3 = new EffectStateStructure
                    {
                        Alpha = 0
                    };
                    AddSet(effect, i * framecount, (i + 1) * framecount, ess2, ess3);
                    if (i != effects.Count - 1)
                    {
                        var ess4 = new EffectStateStructure
                        {
                            Alpha = 1
                        };
                        AddSet(effect, (i + 1) * framecount, ((effects.Count - 1) * 2 - i) * framecount, ess3, ess4);
                        ess3 = ess4;
                    }
                    var ess5 = new EffectStateStructure
                    {
                        Alpha = 0
                    };
                    var set4StartFrame = ((effects.Count - 1) * 2 - i) * framecount;
                    var set4EndFrame = set4StartFrame + framecount;
                    if (i != effects.Count - 1)
                    {
                        AddSet(effect, set4StartFrame, set4EndFrame, ess3, ess5);
                    }
                    if (!first)
                    {
                        var ess6 = new EffectStateStructure
                        {
                            Alpha = 0
                        };
                        AddSet(effect, set4EndFrame, framecount * (2 * effects.Count - 1), i == effects.Count - 1 ? ess3 : ess5, ess6);
                    }
                    first = false;
                }
            }
        }

        /// <summary>
        /// 変化しない（一定の割合の）ステートの組をエフェクトに加えます。
        /// </summary>
        private static void AddSet(IEffect effect, int startFrame, int endFrame, EffectStateStructure startState, EffectStateStructure endState)
        {
            var set = new EffectStateRatioSet();
            foreach (RatioType type in Effect2D.Utility.RatioTypeArray)
            {
                set[type] = new ConstantRatioMaker();
            }
            set.StartFrame = startFrame;
            set.EndFrame = endFrame;
            set.StartState = startState;
            set.EndState = endState;
            effect.Sets.Add(set.StartFrame, set);
        }
    }
}
