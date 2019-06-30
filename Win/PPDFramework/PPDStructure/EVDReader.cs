using PPDFramework.PPDStructure.EVDData;
using System;
using System.Collections.Generic;
using System.IO;

namespace PPDFramework.PPDStructure
{
    /// <summary>
    /// EVDリーダークラス
    /// </summary>
    public static class EVDReader
    {
        /// <summary>
        /// 読み取る
        /// </summary>
        /// <param name="path">evdパス</param>
        /// <returns></returns>
        public static IEVDData[] Read(string path)
        {
            IEVDData[] ret = new IEVDData[0];
            if (File.Exists(path))
            {
                var fs = File.Open(path, FileMode.Open);
                ret = Read(fs);
                fs.Close();
            }
            return ret;
        }

        /// <summary>
        /// 読み取る
        /// </summary>
        /// <param name="stream">ストリーム</param>
        /// <returns></returns>
        public static IEVDData[] Read(Stream stream)
        {
            var data = new List<IEVDData>();
            byte[] tb = new byte[4];
            float time;
            byte mode;
            while (stream.Position < stream.Length)
            {
                stream.Read(tb, 0, tb.Length);
                time = BitConverter.ToSingle(tb, 0);
                mode = (byte)stream.ReadByte();

                IEVDData evddata = null;
                switch (mode)
                {
                    case 0:
                        var channel = (byte)stream.ReadByte();
                        var volpercent = (byte)stream.ReadByte();
                        evddata = new ChangeVolumeEvent(time, volpercent, channel);
                        break;
                    case 1:
                        stream.Read(tb, 0, tb.Length);
                        evddata = new ChangeBPMEvent(time, BitConverter.ToSingle(tb, 0));
                        if ((evddata as ChangeBPMEvent).BPM < 0)
                        {
                            evddata = null;
                        }
                        break;
                    case 2:
                        stream.Read(tb, 0, tb.Length);
                        byte[] rapid = new byte[1];
                        stream.Read(rapid, 0, rapid.Length);
                        evddata = new RapidChangeBPMEvent(time, BitConverter.ToSingle(tb, 0), BitConverter.ToBoolean(rapid, 0));
                        if ((evddata as RapidChangeBPMEvent).BPM < 0)
                        {
                            evddata = null;
                        }
                        break;
                    case 3:
                        channel = (byte)stream.ReadByte();
                        var keepplaying = (byte)stream.ReadByte();
                        evddata = new ChangeSoundPlayModeEvent(time, keepplaying == 1, channel);
                        break;
                    case 4:
                        var dstate = (DisplayState)stream.ReadByte();
                        evddata = new ChangeDisplayStateEvent(time, dstate);
                        break;
                    case 5:
                        var mstate = (MoveState)stream.ReadByte();
                        evddata = new ChangeMoveStateEvent(time, mstate);
                        break;
                    case 6:
                        channel = (byte)stream.ReadByte();
                        var releasesound = (byte)stream.ReadByte();
                        evddata = new ChangeReleaseSoundEvent(time, releasesound == 1, channel);
                        break;
                    case 7:
                        var noteType = (byte)stream.ReadByte();
                        evddata = new ChangeNoteTypeEvent(time, (NoteType)noteType);
                        break;
                    case 8:
                        byte[] table = new byte[10];
                        ButtonType[] convertedTable = new ButtonType[10];
                        stream.Read(table, 0, table.Length);
                        for (int i = 0; i < 10; i++)
                        {
                            convertedTable[i] = (ButtonType)table[i];
                        }
                        evddata = new ChangeInitializeOrderEvent(time, convertedTable);
                        break;
                    case 9:
                        stream.Read(tb, 0, tb.Length);
                        var slideScale = BitConverter.ToSingle(tb, 0);
                        evddata = new ChangeSlideScaleEvent(time, slideScale);
                        break;
                }
                if (evddata != null) data.Add(evddata);
            }
            return data.ToArray();
        }
    }
}
