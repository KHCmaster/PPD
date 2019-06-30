using FlowScriptEngine;
using System;
using System.Text;

namespace FlowScriptEngineBasic.FlowSourceObjects.Bit
{
    [ToolTipText("Bit_ReadableString_Summary")]
    public partial class ReadableStringFlowSourceObject : FlowSourceObjectBase
    {
        public override string Name
        {
            get { return "Bit.ReadableString"; }
        }

        [ToolTipText("Bit_ReadableString_Value")]
        public int Value
        {
            private get;
            set;
        }

        [ToolTipText("Bit_ReadableString_WithZeroPadding")]
        public bool WithZeroPadding
        {
            private get;
            set;
        }

        [ToolTipText("Bit_ReadableString_String")]
        public string String
        {
            get
            {
                SetValue(nameof(Value));
                SetValue(nameof(WithZeroPadding));
                if (WithZeroPadding)
                {
                    var sb = new StringBuilder(BitUtility.Int32BitCount());
                    for (int i = BitUtility.Int32BitCount() - 1; i >= 0; i--)
                    {
                        sb.Append(BitUtility.HasFlag(Value, i) ? "1" : "0");
                    }
                    return sb.ToString();
                }
                else
                {
                    return Convert.ToString(Value, 2);
                }
            }
        }
    }
}
