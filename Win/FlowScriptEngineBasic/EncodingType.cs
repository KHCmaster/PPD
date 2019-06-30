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
            switch (encodingType)
            {
                case EncodingType.ASCII:
                    return Encoding.ASCII;
                case EncodingType.UTF_8:
                    return Encoding.UTF8;
                case EncodingType.UTF_16:
                    return Encoding.Unicode;
                case EncodingType.UTF_32:
                    return Encoding.UTF32;
                case EncodingType.SHIFT_JIS:
                    return Encoding.GetEncoding(932);
                case EncodingType.EUC_JP:
                    return Encoding.GetEncoding(20932);
            }

            return Encoding.ASCII;
        }
    }
}
