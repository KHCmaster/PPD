using PPDFramework;
using System.Collections.Generic;

namespace PPD
{
    class Loading(PPDDevice device) : LoadingBase(device)
    {
        public override void EnterLoading()
        {
        }

        public override bool Load()
        {
            return true;
        }

        public override void SendToLoading(Dictionary<string, object> parameters)
        {
        }
    }
}
