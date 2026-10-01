using PPDPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace PPDCore
{
    class PPDPlayDecorder
    {
        private readonly List<PPDPlayInput> inputs;
        private int iter;

        public bool IsEnd
        {
            get
            {
                return iter >= inputs.Count;
            }
        }

        public int Position
        {
            get { return iter; }
        }

        public int Length
        {
            get { return inputs.Count; }
        }

        private PPDPlayDecorder()
        {
            inputs = [];
        }

        public void Reset()
        {
            iter = 0;
        }

        public static PPDPlayDecorder FromInputs(PPDPlayInput[] inputs)
        {
            var ret = new PPDPlayDecorder();
            ret.inputs.AddRange(inputs);
            return ret;
        }

        public static PPDPlayDecorder FromRecorder(PPDPlayRecorder recorder)
        {
            var ret = new PPDPlayDecorder();
            ret.inputs.AddRange(recorder.Inputs);
            return ret;
        }

        public static PPDPlayDecorder FromBytes(byte[] bytes)
        {
            int version = 0;
            PPDPlayDecorder ret = null;
            using (MemoryStream stream = new(bytes))
            using (PackReader reader = new(stream))
            {
                foreach (string name in reader.FileList)
                {
                    var streamReader = reader.Read(name);
                    switch (name)
                    {
                        case "Version":
                            version = streamReader.ReadByte();
                            break;
                        case "Data":
                            ret = FromStream(streamReader);
                            break;
                    }
                }
            }
            if (version != 2)
            {
                throw new Exception(String.Format("Not Supported Replay Data:{0}", version));
            }
            return ret;
        }

        public static PPDPlayDecorder FromFile(string filePath)
        {
            using (FileStream fs = File.Open(filePath, FileMode.Open))
            {
                return FromStream(fs);
            }
        }

        /// <summary>
        /// count バイト読み込むか、ストリームの終わりに達するまで読み込みます。
        /// .NET では GZipStream.Read が要求より少ないバイト数を返すことがあるため（.NET Framework では要求したバイト数まで読み込んでいた）。
        /// </summary>
        private static int ReadFully(Stream stream, byte[] buffer, int count)
        {
            var total = 0;
            while (total < count)
            {
                var read = stream.Read(buffer, total, count - total);
                if (read == 0)
                {
                    break;
                }
                total += read;
            }
            return total;
        }

        public static PPDPlayDecorder FromStream(Stream stream)
        {
            var ret = new PPDPlayDecorder();
            using (GZipStream compStream = new(stream, CompressionMode.Decompress))
            {
                int readSize = 0;
                byte[] bytes2 = new byte[2];
                byte[] bytes4 = new byte[4];
                byte[] bytes8 = new byte[8];
                while (true)
                {
                    UInt16[] pressCount = new UInt16[10];
                    byte[] pressed = new byte[2];
                    byte[] released = new byte[2];
                    readSize = ReadFully(compStream, bytes4, bytes4.Length);
                    if (readSize == 0)
                    {
                        break;
                    }
                    var time = BitConverter.ToSingle(bytes4, 0);
                    readSize = ReadFully(compStream, bytes8, bytes8.Length);
                    if (readSize == 0)
                    {
                        break;
                    }
                    var currentTime = BitConverter.ToInt64(bytes8, 0);
                    readSize = ReadFully(compStream, bytes2, bytes2.Length);
                    if (readSize == 0)
                    {
                        break;
                    }
                    var bits = new BitArray(bytes2);
                    for (int i = 0; i < pressCount.Length; i++)
                    {
                        if (bits.Get(i))
                        {
                            readSize = ReadFully(compStream, bytes2, bytes2.Length);
                            if (readSize == 0)
                            {
                                break;
                            }
                            pressCount[i] = BitConverter.ToUInt16(bytes2, 0);
                        }
                    }
                    readSize = ReadFully(compStream, pressed, pressed.Length);
                    if (readSize == 0)
                    {
                        break;
                    }
                    readSize = ReadFully(compStream, released, released.Length);
                    if (readSize == 0)
                    {
                        break;
                    }
                    ret.inputs.Add(new PPDPlayInput(time, currentTime, pressCount, pressed, released));
                }
            }
            return ret;
        }

        public bool Update(float time, int[] presscount, bool[] pressed, bool[] released, out float newTime, out long newCurrentTime)
        {
            newTime = time;
            newCurrentTime = 0;
            if (iter >= inputs.Count)
            {
                return false;
            }

            if (inputs[iter].Time <= time + 1 / 120f)
            {
                newTime = inputs[iter].Time;
                newCurrentTime = inputs[iter].CurrentTime;
                Array.Copy(inputs[iter].PressCount, presscount, inputs[iter].PressCount.Length);
                Array.Copy(inputs[iter].Pressed, pressed, inputs[iter].Pressed.Length);
                Array.Copy(inputs[iter].Released, released, inputs[iter].Released.Length);
                iter++;
                return true;
            }

            return false;
        }
    }
}
