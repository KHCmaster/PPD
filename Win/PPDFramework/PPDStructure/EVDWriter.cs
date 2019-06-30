using PPDFramework.PPDStructure.EVDData;
using System.IO;

namespace PPDFramework.PPDStructure
{
    /// <summary>
    /// EVDデータを書き込むクラスです
    /// </summary>
    public static class EVDWriter
    {
        /// <summary>
        /// 書き込む
        /// </summary>
        /// <param name="path"></param>
        /// <param name="eventData"></param>
        /// <returns></returns>
        public static void Write(string path, IEVDData[] eventData)
        {
            using (Stream stream = File.Create(path))
            {
                Write(stream, eventData);
            }
        }

        /// <summary>
        /// 書き込む
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="eventData"></param>
        /// <returns></returns>
        public static void Write(Stream stream, IEVDData[] eventData)
        {
            foreach (IEVDData evdData in eventData)
            {
                switch (evdData.EventType)
                {
                    case EventType.ChangeVolume:
                        WriteChangeVolumeEvent(stream, evdData as ChangeVolumeEvent);
                        break;
                    case EventType.ChangeBPM:
                        WriteChangeBPMEvent(stream, evdData as ChangeBPMEvent);
                        break;
                    case EventType.RapidChangeBPM:
                        WriteRapidChangeBPMEvent(stream, evdData as RapidChangeBPMEvent);
                        break;
                    case EventType.ChangeSoundPlayMode:
                        WriteChangeSoundPlayModeEvent(stream, evdData as ChangeSoundPlayModeEvent);
                        break;
                    case EventType.ChangeDisplayState:
                        WriteChangeDisplayStateEvent(stream, evdData as ChangeDisplayStateEvent);
                        break;
                    case EventType.ChangeMoveState:
                        WriteChangeMoveStateEvent(stream, evdData as ChangeMoveStateEvent);
                        break;
                    case EventType.ChangeReleaseSound:
                        WriteChangeReleaseSoundEvent(stream, evdData as ChangeReleaseSoundEvent);
                        break;
                    case EventType.ChangeNoteType:
                        WriteChangeNoteTypeEvent(stream, evdData as ChangeNoteTypeEvent);
                        break;
                    case EventType.ChangeInitializeOrder:
                        WriteChangeInitializeOrderEvent(stream, evdData as ChangeInitializeOrderEvent);
                        break;
                    case EventType.ChangeSlideScale:
                        WriteChangeSlideScaleEvent(stream, evdData as ChangeSlideScaleEvent);
                        break;
                }
            }
        }

        private static void WriteChangeBPMEvent(Stream stream, ChangeBPMEvent e)
        {
            var tbyte = System.BitConverter.GetBytes(e.Time);
            stream.Write(tbyte, 0, tbyte.Length);
            stream.WriteByte(1);
            tbyte = System.BitConverter.GetBytes(e.BPM);
            stream.Write(tbyte, 0, tbyte.Length);
        }

        private static void WriteRapidChangeBPMEvent(Stream stream, RapidChangeBPMEvent e)
        {
            var tbyte = System.BitConverter.GetBytes(e.Time);
            stream.Write(tbyte, 0, tbyte.Length);
            stream.WriteByte(2);
            tbyte = System.BitConverter.GetBytes(e.BPM);
            stream.Write(tbyte, 0, tbyte.Length);
            tbyte = System.BitConverter.GetBytes(e.Rapid);
            stream.Write(tbyte, 0, tbyte.Length);
        }

        private static void WriteChangeVolumeEvent(Stream stream, ChangeVolumeEvent e)
        {
            var tbyte = System.BitConverter.GetBytes(e.Time);
            stream.Write(tbyte, 0, tbyte.Length);
            stream.WriteByte(0);
            stream.WriteByte((byte)e.Channel);
            stream.WriteByte((byte)e.Volume);
        }

        private static void WriteChangeSoundPlayModeEvent(Stream stream, ChangeSoundPlayModeEvent e)
        {
            var tbyte = System.BitConverter.GetBytes(e.Time);
            stream.Write(tbyte, 0, tbyte.Length);
            stream.WriteByte(3);
            stream.WriteByte((byte)e.Channel);
            tbyte = System.BitConverter.GetBytes(e.KeepPlaying);
            stream.Write(tbyte, 0, tbyte.Length);
        }

        private static void WriteChangeReleaseSoundEvent(Stream stream, ChangeReleaseSoundEvent e)
        {
            var tbyte = System.BitConverter.GetBytes(e.Time);
            stream.Write(tbyte, 0, tbyte.Length);
            stream.WriteByte(6);
            stream.WriteByte((byte)e.Channel);
            tbyte = System.BitConverter.GetBytes(e.ReleaseSound);
            stream.Write(tbyte, 0, tbyte.Length);
        }

        private static void WriteChangeDisplayStateEvent(Stream stream, ChangeDisplayStateEvent e)
        {
            var tbyte = System.BitConverter.GetBytes(e.Time);
            stream.Write(tbyte, 0, tbyte.Length);
            stream.WriteByte(4);
            stream.WriteByte((byte)e.DisplayState);
        }

        private static void WriteChangeMoveStateEvent(Stream stream, ChangeMoveStateEvent e)
        {
            var tbyte = System.BitConverter.GetBytes(e.Time);
            stream.Write(tbyte, 0, tbyte.Length);
            stream.WriteByte(5);
            stream.WriteByte((byte)e.MoveState);
        }

        private static void WriteChangeNoteTypeEvent(Stream stream, ChangeNoteTypeEvent e)
        {
            var tbyte = System.BitConverter.GetBytes(e.Time);
            stream.Write(tbyte, 0, tbyte.Length);
            stream.WriteByte(7);
            stream.WriteByte((byte)e.NoteType);
        }

        private static void WriteChangeInitializeOrderEvent(Stream stream, ChangeInitializeOrderEvent e)
        {
            var tbyte = System.BitConverter.GetBytes(e.Time);
            stream.Write(tbyte, 0, tbyte.Length);
            stream.WriteByte(8);
            for (int i = 0; i < e.InitializeOrder.Length; i++)
            {
                stream.WriteByte((byte)e.InitializeOrder[i]);
            }
        }

        private static void WriteChangeSlideScaleEvent(Stream stream, ChangeSlideScaleEvent e)
        {
            var tbyte = System.BitConverter.GetBytes(e.Time);
            stream.Write(tbyte, 0, tbyte.Length);
            stream.WriteByte(9);
            tbyte = System.BitConverter.GetBytes(e.SlideScale);
            stream.Write(tbyte, 0, tbyte.Length);
        }
    }
}
