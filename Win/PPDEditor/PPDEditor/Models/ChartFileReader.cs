using PPDConfiguration;
using PPDEditorCommon;
using PPDFramework;
using PPDFramework.PPDStructure;
using PPDFramework.PPDStructure.EVDData;
using PPDPack;
using EventType = PPDFramework.PPDStructure.EVDData.EventType;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace PPDEditor.Models
{
    /// <summary>
    /// data.ini の中身です。
    /// </summary>
    /// <param name="Times">サムネイルの開始と終了、曲の開始と終了の時間。</param>
    /// <param name="Difficulties">Easy から Extreme までの難易度の表示。</param>
    public sealed record DataIniContent(float[] Times, string[] Difficulties, float Bpm, string BpmString, MovieTrimmingData Trimming, string Guid);

    /// <summary>
    /// 譜面に付くファイル（*.scd、*.evd、data.ini）を読み、エディターで使う形にします。
    /// </summary>
    public static class ChartFileReader
    {
        /// <summary>
        /// *.scd（音の切り替え）を、時間ごとのすべてのボタンの音の番号にします。
        /// </summary>
        /// <returns>切り替えがないときは null。</returns>
        public static (float[] Times, ushort[][] Changes)? ReadSoundChanges(string filename)
        {
            var scdData = SCDReader.Read(filename);
            if (scdData.Length == 0)
            {
                return null;
            }
            float lastTime = scdData[0].Time;
            ushort[] changes = new ushort[10];
            var times = new List<float>();
            var soundChanges = new List<ushort[]>();
            if (lastTime > 0)
            {
                times.Add(0);
                soundChanges.Add((ushort[])changes.Clone());
            }
            foreach (SCDData scd in scdData)
            {
                if (lastTime != scd.Time)
                {
                    times.Add(lastTime);
                    soundChanges.Add((ushort[])changes.Clone());
                }
                lastTime = scd.Time;
                changes[(int)scd.ButtonType] = scd.SoundIndex;
            }
            times.Add(lastTime);
            soundChanges.Add((ushort[])changes.Clone());
            return ([.. times], [.. soundChanges]);
        }

        /// <summary>
        /// *.evd（イベント）を、時間ごとのすべての状態にします。
        /// </summary>
        /// <param name="bpm">最初の BPM（BPM を変えるイベントがないとき）。</param>
        public static SortedList<float, EventData> ReadEvents(Stream stream, float bpm)
        {
            var ret = new SortedList<float, EventData>();
            var evdData = EVDReader.Read(stream);
            if (evdData.Length == 0)
            {
                return ret;
            }
            float lasttime = evdData[0].Time;
            var evdata = new EventData
            {
                BPM = bpm
            };
            foreach (IEVDData eventData in evdData)
            {
                if (lasttime != eventData.Time)
                {
                    ret.Add(lasttime, evdata);
                    evdata = evdata.Clone();
                }
                lasttime = eventData.Time;
                switch (eventData.EventType)
                {
                    case EventType.ChangeVolume:
                        evdata.SetVolume(((ChangeVolumeEvent)eventData).Channel, ((ChangeVolumeEvent)eventData).Volume);
                        break;
                    case EventType.ChangeBPM:
                        evdata.BPM = ((ChangeBPMEvent)eventData).BPM;
                        evdata.BPMRapidChange = false;
                        break;
                    case EventType.RapidChangeBPM:
                        evdata.BPM = ((RapidChangeBPMEvent)eventData).BPM;
                        evdata.BPMRapidChange = ((RapidChangeBPMEvent)eventData).Rapid;
                        break;
                    case EventType.ChangeSoundPlayMode:
                        evdata.SetKeepPlaying(((ChangeSoundPlayModeEvent)eventData).Channel, ((ChangeSoundPlayModeEvent)eventData).KeepPlaying);
                        break;
                    case EventType.ChangeDisplayState:
                        evdata.DisplayState = ((ChangeDisplayStateEvent)eventData).DisplayState;
                        break;
                    case EventType.ChangeMoveState:
                        evdata.MoveState = ((ChangeMoveStateEvent)eventData).MoveState;
                        break;
                    case EventType.ChangeReleaseSound:
                        evdata.SetReleaseSound(((ChangeReleaseSoundEvent)eventData).Channel, ((ChangeReleaseSoundEvent)eventData).ReleaseSound);
                        break;
                    case EventType.ChangeNoteType:
                        evdata.NoteType = ((ChangeNoteTypeEvent)eventData).NoteType;
                        break;
                    case EventType.ChangeSlideScale:
                        evdata.SlideScale = ((ChangeSlideScaleEvent)eventData).SlideScale;
                        break;
                    case EventType.ChangeInitializeOrder:
                        evdata.InitializeOrder = new ButtonType[10];
                        for (int i = 0; i < evdata.InitializeOrder.Length; i++)
                        {
                            evdata.InitializeOrder[i] = ((ChangeInitializeOrderEvent)eventData).InitializeOrder[i];
                        }
                        break;
                }
            }
            ret.Add(lasttime, evdata);
            return ret;
        }

        /// <summary>
        /// 譜面のパック（*.ppd）の中の evd か、evd のファイルを読みます。
        /// </summary>
        public static SortedList<float, EventData> ReadEventsFromFile(string path, float bpm)
        {
            try
            {
                using PackReader reader = new(path);
                return ReadEvents(reader.Read("evd"), bpm);
            }
            catch
            {
                using FileStream stream = File.Open(path, FileMode.Open);
                return ReadEvents(stream, bpm);
            }
        }

        /// <summary>
        /// data.ini を読みます。
        /// </summary>
        public static DataIniContent ReadDataIni(string filename)
        {
            var setting = new SettingReader(File.ReadAllText(filename));
            float[] times = new float[4];
            string[] diffs = new string[4];
            float.TryParse(setting.ReadString("thumbtimestart"), NumberStyles.Float, CultureInfo.InvariantCulture, out times[0]);
            float.TryParse(setting.ReadString("thumbtimeend"), NumberStyles.Float, CultureInfo.InvariantCulture, out times[1]);
            float.TryParse(setting.ReadString("start"), NumberStyles.Float, CultureInfo.InvariantCulture, out times[2]);
            float.TryParse(setting.ReadString("end"), NumberStyles.Float, CultureInfo.InvariantCulture, out times[3]);
            float.TryParse(setting.ReadString("bpm"), NumberStyles.Float, CultureInfo.InvariantCulture, out float bpm);
            diffs[0] = setting.ReadString("difficulty easy");
            diffs[1] = setting.ReadString("difficulty normal");
            diffs[2] = setting.ReadString("difficulty hard");
            diffs[3] = setting.ReadString("difficulty extreme");
            float.TryParse(setting.ReadString("moviecutleft"), NumberStyles.Float, CultureInfo.InvariantCulture, out float left);
            float.TryParse(setting.ReadString("moviecutright"), NumberStyles.Float, CultureInfo.InvariantCulture, out float right);
            float.TryParse(setting.ReadString("moviecuttop"), NumberStyles.Float, CultureInfo.InvariantCulture, out float top);
            float.TryParse(setting.ReadString("moviecutbottom"), NumberStyles.Float, CultureInfo.InvariantCulture, out float bottom);
            return new DataIniContent(times, diffs, bpm, setting.ReadString("bpmstring"),
                new MovieTrimmingData(top, left, right, bottom), setting.ReadString("guid"));
        }

        /// <summary>
        /// soundset.txt（音のファイルの名前の一覧）を読み、sound のフォルダの中のパスにします。
        /// </summary>
        /// <returns>空のときは空の配列。</returns>
        public static string[] ReadSoundSet(string filename)
        {
            var insidedirectory = Path.GetDirectoryName(filename);
            var s = File.ReadAllText(filename).Replace("\r\n", "\r").Replace("\r", "\n");
            if (s.Length == 0) return [];
            if (s[^1] == '\n')
            {
                s = s[..^1];
            }
            var pathes = s.Split('\n');
            for (int i = 0; i < pathes.Length; i++)
            {
                pathes[i] = insidedirectory + "\\sound\\" + pathes[i];
            }
            return pathes;
        }
    }
}
