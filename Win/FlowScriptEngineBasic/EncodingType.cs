using System.Text;

namespace FlowScriptEngineBasic
{
    public enum EncodingType
    {
        ASCII = 0,
        UTF_8,
        UTF_16,
        UTF_32,
        SHIFT_JIS,
        EUC_JP
    }

    class EncodingUtility
    {
        public static Encoding GetEncoding(EncodingType encodingType)
        {
            return encodingType switch
            {
                EncodingType.ASCII => Encoding.ASCII,
                EncodingType.UTF_8 => Encoding.UTF8,
                EncodingType.UTF_16 => Encoding.Unicode,
                EncodingType.UTF_32 => Encoding.UTF32,
                EncodingType.SHIFT_JIS => Encoding.GetEncoding(932),
                EncodingType.EUC_JP => Encoding.GetEncoding(20932),
                _ => Encoding.ASCII,
            };
        }
    }
}
