using System.IO;

namespace PPDFramework.PPDStructure
{
    /// <summary>
    /// SCDデータを書き込むクラスです
    /// </summary>
    public static class SCDWriter
    {
        /// <summary>
        /// 書き込む
        /// </summary>
        /// <param name="path"></param>
        /// <param name="scdData"></param>
        /// <returns></returns>
        public static void Write(string path, SCDData[] scdData)
        {
            using (Stream stream = File.Create(path))
            {
                Write(stream, scdData);
            }
        }

        /// <summary>
        /// 書き込む
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static void Write(Stream stream, SCDData[] data)
        {
            foreach (SCDData scdData in data)
            {
                var tbyte = System.BitConverter.GetBytes(scdData.Time);
                stream.Write(tbyte, 0, tbyte.Length);
                stream.WriteByte((byte)scdData.ButtonType);
                var cbyte = System.BitConverter.GetBytes(scdData.SoundIndex);
                stream.Write(cbyte, 0, cbyte.Length);
            }
        }
    }
}
