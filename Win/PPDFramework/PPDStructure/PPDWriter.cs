using PPDFramework.PPDStructure.PPDData;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace PPDFramework.PPDStructure
{
    /// <summary>
    /// PPDデータを書き込むクラスです
    /// </summary>
    public static class PPDWriter
    {
        /// <summary>
        /// 書き込む
        /// </summary>
        /// <param name="path"></param>
        /// <param name="markData"></param>
        /// <returns></returns>
        public static void Write(string path, MarkData[] markData)
        {
            using (Stream stream = File.Create(path))
            {
                Write(stream, markData);
            }
        }

        /// <summary>
        /// 書き込む
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="markData"></param>
        /// <returns></returns>
        public static void Write(Stream stream, MarkData[] markData)
        {
            var sign = new byte[] { (byte)'P', (byte)'P', (byte)'D' };
            byte[] fdata;
            stream.Write(sign, 0, sign.Length);
            var hasParameters = new List<MarkData>();
            for (int i = 0; i < markData.Length; i++)
            {
                fdata = System.BitConverter.GetBytes(markData[i].Time);
                stream.Write(fdata, 0, fdata.Length);
                fdata = System.BitConverter.GetBytes(markData[i].X);
                stream.Write(fdata, 0, fdata.Length);
                fdata = System.BitConverter.GetBytes(markData[i].Y);
                stream.Write(fdata, 0, fdata.Length);
                fdata = System.BitConverter.GetBytes(markData[i].Angle);
                stream.Write(fdata, 0, fdata.Length);
                var offset = (byte)(markData[i].ID != 0 ? 40 : 0);
                var exmkData = markData[i] as ExMarkData;
                if (exmkData == null)
                {
                    stream.WriteByte((byte)(markData[i].ButtonType + offset));
                }
                else
                {
                    stream.WriteByte((byte)(markData[i].ButtonType + 20 + offset));
                    fdata = System.BitConverter.GetBytes(exmkData.EndTime);
                    stream.Write(fdata, 0, fdata.Length);
                }
                if (offset != 0)
                {
                    fdata = System.BitConverter.GetBytes(markData[i].ID);
                    stream.Write(fdata, 0, fdata.Length);
                }
                if (markData[i].ParameterCount == 0 | markData[i].ID == 0)
                {
                    continue;
                }
                hasParameters.Add(markData[i]);
            }
            if (hasParameters.Count == 0)
            {
                return;
            }
            fdata = System.BitConverter.GetBytes(float.NaN);
            stream.Write(fdata, 0, fdata.Length);
            fdata = System.BitConverter.GetBytes(hasParameters.Count);
            stream.Write(fdata, 0, fdata.Length);
            foreach (var mark in hasParameters)
            {
                fdata = System.BitConverter.GetBytes(mark.ID);
                stream.Write(fdata, 0, fdata.Length);
                var document = new XDocument(new XElement("Root"));
                foreach (var para in mark.Parameters)
                {
                    var elem = new XElement("Parameter", new XAttribute("Key", para.Key), new XAttribute("Value", para.Value));
                    document.Root.Add(elem);
                }
                var str = document.ToString();
                var bytes = Encoding.UTF8.GetBytes(str);
                fdata = System.BitConverter.GetBytes(str.Length);
                stream.Write(fdata, 0, fdata.Length);
                stream.Write(bytes, 0, bytes.Length);
            }
        }
    }
}
