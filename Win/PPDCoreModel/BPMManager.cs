using System;

namespace PPDCoreModel
{
    public class BPMManager
    {
        public float CurrentBPM
        {
            get;
            set;
        }

        public float TargetBPM
        {
            get;
            set;
        }

        public void Step()
        {
            if (CurrentBPM != TargetBPM)
            {
                CurrentBPM += Math.Sign(TargetBPM - CurrentBPM);
                if (Math.Abs(TargetBPM - CurrentBPM) < 1)
                {
                    CurrentBPM = TargetBPM;
                }
            }
        }
    }
}
