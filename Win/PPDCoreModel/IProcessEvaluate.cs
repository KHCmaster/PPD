using PPDCoreModel.Data;
using System.Numerics;
using PPDMath;

namespace PPDCoreModel
{
    public interface IProcessEvaluate : IEvaluate
    {
        void ProcessEvaluate(IMarkInfo markInfo, EffectType effectType, bool missPress, bool release, Vector2 position);
    }
}
