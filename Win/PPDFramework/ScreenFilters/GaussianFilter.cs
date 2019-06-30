using System;

namespace PPDFramework.ScreenFilters
{
    /// <summary>
    /// ガウシアンフィルタクラスです。
    /// </summary>
    public class GaussianFilter : ScreenFilterBase
    {
        /// <summary>
        /// 最小の分散
        /// </summary>
        public const float MinAvailableDisperson = 0.0f;

        /// <summary>
        /// 最大の分散
        /// </summary>
        public const float MaxAvailableDisperson = float.MaxValue;

        /// <summary>
        /// 最大のウェイト数
        /// </summary>
        public const int MaxWeightCount = 32;

        float[] weights;
        float disperson;

        /// <summary>
        /// 分散を取得、設定します
        /// </summary>
        public float Disperson
        {
            get { return disperson; }
            set
            {
                if (value < MinAvailableDisperson)
                {
                    value = MinAvailableDisperson;
                }
                if (value > MaxAvailableDisperson)
                {
                    value = MaxAvailableDisperson;
                }
                if (disperson != value)
                {
                    disperson = value;
                    UpdateWeights();
                }
            }
        }

        /// <summary>
        /// コンストラクタです
        /// </summary>
        public GaussianFilter()
        {
            weights = new float[MaxWeightCount];
            UpdateWeights();
        }

        /// <summary>
        /// フィルター処理です
        /// </summary>
        /// <param name="device"></param>
        public override void Filter(PPDDevice device)
        {
            device.GetModule<Impl.GaussianFilter>().Draw(device, weights);
        }

        private void UpdateWeights()
        {
            if (disperson < 1)
            {
                Array.Clear(weights, 0, weights.Length);
                weights[0] = 1;
            }
            else
            {
                float total = 0.0f;
                for (int i = 0; i < MaxWeightCount; i++)
                {
                    weights[i] = (float)(Math.Exp(-0.5 * i * i / disperson) / Math.Sqrt(2 * Math.PI * disperson));
                    total += i == 0 ? weights[i] : (weights[i] * 2);
                }
                if (total == 0)
                {
                    weights[0] = 1f;
                    total = 1;
                }
                for (int i = 0; i < MaxWeightCount; i++)
                {
                    weights[i] /= total;
                }
            }
        }
    }
}
