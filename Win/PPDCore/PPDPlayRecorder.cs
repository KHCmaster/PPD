using PPDPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace PPDCore
{
    class PPDPlayRecorder
    {
        private List<PPDPlayInput> inputs;

        public PPDPlayInput[] Inputs
        {
            get
            {
                return inputs.ToArray();
            }
        }

        public PPDPlayRecorder()
        {
            inputs = new List<PPDPlayInput>();
        }

        public void Update(float time, long currentTime, int[] presscount, bool[] pressed, bool[] released)
        {
            UInt16[] temp = new UInt16[presscount.Length];
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] = (UInt16)presscount[i];
            }

            var input = new PPDPlayInput(time, currentTime, temp, pressed, released);
            inputs.Add(input);
        }

        public void ToFile(string filePath)
        {
            using (FileStream fs = File.Open(filePath, FileMode.Create))
            {
                GetData(fs);
            }
        }

        public byte[] GetBytes()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (PackWriter writer = new PackWriter(memoryStream))
            {
                foreach (PPDPackStreamWriter streamWriter in writer.Write(new string[] { "Version", "Data" }))
                {
                    switch (streamWriter.Filename)
                    {
                        case "Version":
                            streamWriter.WriteByte(2);
                            break;
                        case "Data":
                            var data = GetBytesImpl();
                            streamWriter.Write(data, 0, data.Length);
                            break;
                    }
                }
                return memoryStream.ToArray();
            }
        }

        private byte[] GetBytesImpl()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                GetData(stream);
                return stream.ToArray();
            }
        }

        private void GetData(Stream stream)
        {
            using (GZipStream compStream = new GZipStream(stream, CompressionMode.Compress))
            {
                byte[] bytes;
                foreach (PPDPlayInput input in inputs)
                {
                    bytes = BitConverter.GetBytes(input.Time);
                    compStream.Write(bytes, 0, bytes.Length);
                    bytes = BitConverter.GetBytes(input.CurrentTime);
                    compStream.Write(bytes, 0, bytes.Length);
                    bytes = input.PressCountCompressed;
                    compStream.Write(bytes, 0, bytes.Length);
                    bytes = input.PressedCompressed;
                    compStream.Write(bytes, 0, bytes.Length);
                    bytes = input.ReleasedCompressed;
                    compStream.Write(bytes, 0, bytes.Length);
                }
            }
        }
    }
}
