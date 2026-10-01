using System;
using System.Collections.Generic;
using System.IO;

namespace PPDFramework.PPDStructure
{
    /// <summary>
    /// SCDリーダークラス
    /// </summary>
    public static class SCDReader
    {
        /// <summary>
        /// 読み取る
        /// </summary>
        /// <param name="path">scdパス</param>
        /// <returns></returns>
        public static SCDData[] Read(string path)
        {
            SCDData[] ret = [];
            if (File.Exists(path))
            {
                using FileStream fs = File.Open(path, FileMode.Open);
                return Read(fs);
            }
            return ret;
        }

        /// <summary>
        /// 読み取る
        /// </summary>
        /// <param name="stream">ストリーム</param>
        /// <returns></returns>
        public static SCDData[] Read(Stream stream)
        {
            var list = new List<SCDData>();
            byte buttontype;
            byte[] sb = new byte[2];
            byte[] tb = new byte[4];
            while (stream.Position < stream.Length)
            {
                stream.ReadAtLeast(tb, tb.Length, throwOnEndOfStream: false);
                buttontype = (byte)stream.ReadByte();
                stream.ReadAtLeast(sb, sb.Length, throwOnEndOfStream: false);
                var data = new SCDData(BitConverter.ToSingle(tb, 0), (ButtonType)buttontype, BitConverter.ToUInt16(sb, 0));
                list.Add(data);
            }
            return [.. list];
        }
    }
}
