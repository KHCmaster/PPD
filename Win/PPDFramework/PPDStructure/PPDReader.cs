using PPDFramework.PPDStructure.PPDData;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace PPDFramework.PPDStructure
{
    /// <summary>
    /// PPDリーダークラス
    /// </summary>
    public static class PPDReader
    {
        /// <summary>
        /// 読み取る
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static MarkDataBase[] Read(string path)
        {
            MarkDataBase[] ret = new MarkDataBase[0];
            if (File.Exists(path))
            {
                using (FileStream fs = File.Open(path, FileMode.Open))
                {
                    ret = Read(fs);
                }
            }
            return ret;
        }

        /// <summary>
        /// 読み取る
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static MarkDataBase[] Read(Stream stream)
        {
            bool b = false;
            return Read(stream, ref b);
        }

        /// <summary>
        /// 読み取る
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="signError"></param>
        /// <returns></returns>
        public static MarkDataBase[] Read(Stream stream, ref bool signError)
        {
            var data = new List<MarkDataBase>();
            var dict = new Dictionary<int, MarkDataBase>();
            var sign = new byte[] { (byte)'P', (byte)'P', (byte)'D' };
            byte[] headdata = new byte[3];
            stream.Read(headdata, 0, headdata.Length);
            if (sign[0] != headdata[0] || sign[1] != headdata[1] || sign[2] != headdata[2])
            {
                signError = true;
                return data.ToArray();
            }
            byte[] fdata = new byte[4];
            while (stream.Position < stream.Length)
            {
                stream.Read(fdata, 0, fdata.Length);
                var time = System.BitConverter.ToSingle(fdata, 0);
                if (float.IsNaN(time))
                {
                    stream.Read(fdata, 0, fdata.Length);
                    var count = System.BitConverter.ToInt32(fdata, 0);
                    int index = 0;
                    while (stream.Position < stream.Length)
                    {
                        uint markId;
                        stream.Read(fdata, 0, fdata.Length);
                        markId = System.BitConverter.ToUInt32(fdata, 0);
                        stream.Read(fdata, 0, fdata.Length);
                        var length = System.BitConverter.ToInt32(fdata, 0);
                        byte[] temp = new byte[length];
                        stream.Read(temp, 0, temp.Length);
                        if (!dict.TryGetValue((int)markId, out MarkDataBase ppdData))
                        {
                            continue;
                        }
                        var str = Encoding.UTF8.GetString(temp);
                        var document = XDocument.Parse(str);
                        foreach (var elem in document.Root.Elements("Parameter"))
                        {
                            ppdData.AddParameter(elem.Attribute("Key").Value, elem.Attribute("Value").Value);
                        }
                        index++;
                        if (index >= count)
                        {
                            break;
                        }
                    }
                    break;
                }
                stream.Read(fdata, 0, fdata.Length);
                var x = System.BitConverter.ToSingle(fdata, 0);
                stream.Read(fdata, 0, fdata.Length);
                var y = System.BitConverter.ToSingle(fdata, 0);
                stream.Read(fdata, 0, fdata.Length);
                var rotation = System.BitConverter.ToSingle(fdata, 0);
                var type = stream.ReadByte();
                bool withID = type >= 40;
                type = withID ? type - 40 : type;
                bool isEx = type >= 20;
                type = isEx ? type - 20 : type;
                float endTime = 0;
                if (isEx)
                {
                    stream.Read(fdata, 0, fdata.Length);
                    endTime = System.BitConverter.ToSingle(fdata, 0);
                }
                uint id = 0;
                if (withID)
                {
                    stream.Read(fdata, 0, fdata.Length);
                    id = System.BitConverter.ToUInt32(fdata, 0);
                }
                var markData = isEx ? new ExMarkData(x, y, rotation, time, endTime, (ButtonType)type, id) : new MarkData(x, y, rotation, time, (ButtonType)type, id);
                data.Add(markData);
                if (markData.ID != 0)
                {
                    dict[(int)markData.ID] = markData;
                }
            }
            return data.ToArray();
        }
    }
}
