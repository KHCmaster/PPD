using PPDEditorCommon;
using PPDFramework.PPDStructure.EVDData;
using System.Collections.Generic;

namespace PPDEditor.Models
{
    /// <summary>
    /// 時間ごとのイベント（音量、BPM、マークの表示や動きなど）の一覧です。
    /// </summary>
    public class EventList
    {
        SortedList<float, EventData> data = [];
        readonly List<float[]> stopList = [];

        public int Count
        {
            get { return data.Count; }
        }

        public IList<float> Times
        {
            get { return data.Keys; }
        }

        public IList<EventData> Events
        {
            get { return data.Values; }
        }

        public IEnumerable<KeyValuePair<float, EventData>> Items
        {
            get { return data; }
        }

        /// <summary>
        /// イベントの一覧のコピーです。設定すると、その一覧に入れ替えます。
        /// </summary>
        public SortedList<float, EventData> Data
        {
            get { return new SortedList<float, EventData>(data); }
            set
            {
                data = value;
                UpdateStopList();
            }
        }

        public bool Contains(float time)
        {
            return data.ContainsKey(time);
        }

        public int IndexOf(float time)
        {
            return data.IndexOfKey(time);
        }

        /// <summary>
        /// イベントを足します。同じ時間にイベントがあるときは足さずに false を返します。
        /// </summary>
        public bool Add(float time, EventData eventData)
        {
            if (data.ContainsKey(time))
            {
                return false;
            }
            data.Add(time, eventData);
            UpdateStopList();
            return true;
        }

        public void RemoveAt(int index)
        {
            data.RemoveAt(index);
            UpdateStopList();
        }

        public void ReplaceAt(int index, EventData eventData)
        {
            var time = data.Keys[index];
            data.RemoveAt(index);
            data.Add(time, eventData);
            UpdateStopList();
        }

        /// <summary>
        /// イベントの時間を変えます。前の時間にイベントがないときや、新しい時間にイベントがあるときは変えずに false を返します。
        /// </summary>
        public bool Move(float previousTime, float newTime)
        {
            if (!data.TryGetValue(previousTime, out EventData eventData) || data.ContainsKey(newTime))
            {
                return false;
            }
            data.Remove(previousTime);
            data.Add(newTime, eventData);
            UpdateStopList();
            return true;
        }

        public void Clear()
        {
            data.Clear();
            UpdateStopList();
        }

        /// <summary>
        /// 最初（0 秒）のイベントの BPM を変えます。
        /// </summary>
        public void SetInitialBpm(float bpm)
        {
            data[0].BPM = bpm;
        }

        /// <summary>
        /// 時間の範囲の中のイベントの時間です。
        /// </summary>
        public float[] GetTimesWithin(float startTime, float endTime)
        {
            var ret = new List<float>();
            foreach (float time in data.Keys)
            {
                if (time > endTime)
                {
                    break;
                }
                if (startTime <= time)
                {
                    ret.Add(time);
                }
            }
            return [.. ret];
        }

        public string GetFormattedContent(float time)
        {
            return data.TryGetValue(time, out EventData eventData) ? eventData.GetFormattedContent() : "";
        }

        /// <summary>
        /// その時間に有効なイベント（その時間までで最後のイベント）です。なければ null です。
        /// </summary>
        public EventData GetEvent(float time)
        {
            EventData last = null;
            foreach (var pair in data)
            {
                if (pair.Key > time)
                {
                    break;
                }
                last = pair.Value;
            }
            return last;
        }

        /// <summary>
        /// その時間に有効なイベントの番号です（その時間より前のイベントがなければ -1）。
        /// </summary>
        public int GetIndexAt(float time)
        {
            int i;
            for (i = 0; i < data.Count; i++)
            {
                if (data.Keys[i] > time)
                {
                    break;
                }
            }
            return i - 1;
        }

        /// <summary>
        /// マークが止まっている間を考えて、今の時間を直します（以前と同じ計算）。
        /// </summary>
        public float GetCorrectTime(float currentTime, float markTime)
        {
            float ret = currentTime;
            foreach (var stop in stopList)
            {
                if (markTime <= stop[1]) break;
                if (currentTime >= stop[1]) continue;
                if (currentTime >= stop[0] && currentTime <= stop[1])
                {
                    ret = stop[1];
                }
                if (currentTime < stop[0])
                {
                    ret += stop[1] - stop[0];
                }
            }
            return ret;
        }

        // マークが止まっている間（止まるイベントから、動くイベントまで）
        private void UpdateStopList()
        {
            stopList.Clear();
            float stopStartTime = -1;
            for (int i = 0; i < data.Count; i++)
            {
                if (stopStartTime < 0)
                {
                    if (data.Values[i].MoveState == MoveState.Stop)
                    {
                        stopStartTime = data.Keys[i];
                    }
                }
                else if (data.Values[i].MoveState == MoveState.Normal)
                {
                    stopList.Add([stopStartTime, data.Keys[i]]);
                    stopStartTime = -1;
                }
            }
        }

        /// <summary>
        /// 譜面のイベントのデータ（evd）にします。前のイベントから変わったものだけを書きます。
        /// </summary>
        /// <param name="initialBpm">最初の BPM（設定ファイルの BPM）です。</param>
        public IEVDData[] ToEvdData(float initialBpm)
        {
            var ret = new List<IEVDData>();
            var last = new EventData
            {
                BPM = initialBpm
            };
            for (int i = 0; i < data.Count; i++)
            {
                EventData now = data.Values[i];
                float time = data.Keys[i];
                int[] nowVolumes = [now.MovieVolumePercent, now.SquareVolumePercent, now.CrossVolumePercent, now.CircleVolumePercent, now.TriangleVolumePercent,
                    now.LeftVolumePercent, now.DownVolumePercent, now.RightVolumePercent, now.UpVolumePercent, now.RVolumePercent, now.LVolumePercent];
                int[] lastVolumes = [last.MovieVolumePercent, last.SquareVolumePercent, last.CrossVolumePercent, last.CircleVolumePercent, last.TriangleVolumePercent,
                    last.LeftVolumePercent, last.DownVolumePercent, last.RightVolumePercent, last.UpVolumePercent, last.RVolumePercent, last.LVolumePercent];
                for (int channel = 0; channel < nowVolumes.Length; channel++)
                {
                    if (nowVolumes[channel] != lastVolumes[channel])
                    {
                        ret.Add(new ChangeVolumeEvent(time, (byte)nowVolumes[channel], channel));
                    }
                }
                if (now.BPM != last.BPM)
                {
                    ret.Add(now.BPMRapidChange ? new RapidChangeBPMEvent(time, now.BPM, now.BPMRapidChange) : new ChangeBPMEvent(time, now.BPM));
                }

                bool[] nowKeep = [now.SquareKeepPlaying, now.CrossKeepPlaying, now.CircleKeepPlaying, now.TriangleKeepPlaying,
                    now.LeftKeepPlaying, now.DownKeepPlaying, now.RightKeepPlaying, now.UpKeepPlaying, now.RKeepPlaying, now.LKeepPlaying];
                bool[] lastKeep = [last.SquareKeepPlaying, last.CrossKeepPlaying, last.CircleKeepPlaying, last.TriangleKeepPlaying,
                    last.LeftKeepPlaying, last.DownKeepPlaying, last.RightKeepPlaying, last.UpKeepPlaying, last.RKeepPlaying, last.LKeepPlaying];
                for (int button = 0; button < nowKeep.Length; button++)
                {
                    if (nowKeep[button] != lastKeep[button])
                    {
                        // 以前から、レフトの変更には L の値を書いている（既存の譜面と同じ結果になるように、そのままにする）
                        var value = button == 4 ? now.LKeepPlaying : nowKeep[button];
                        ret.Add(new ChangeSoundPlayModeEvent(time, value, button + 1));
                    }
                }

                if (now.DisplayState != last.DisplayState)
                {
                    ret.Add(new ChangeDisplayStateEvent(time, now.DisplayState));
                }
                if (now.MoveState != last.MoveState)
                {
                    ret.Add(new ChangeMoveStateEvent(time, now.MoveState));
                }

                bool[] nowRelease = [now.SquareReleaseSound, now.CrossReleaseSound, now.CircleReleaseSound, now.TriangleReleaseSound,
                    now.LeftReleaseSound, now.DownReleaseSound, now.RightReleaseSound, now.UpReleaseSound, now.RReleaseSound, now.LReleaseSound];
                bool[] lastRelease = [last.SquareReleaseSound, last.CrossReleaseSound, last.CircleReleaseSound, last.TriangleReleaseSound,
                    last.LeftReleaseSound, last.DownReleaseSound, last.RightReleaseSound, last.UpReleaseSound, last.RReleaseSound, last.LReleaseSound];
                for (int button = 0; button < nowRelease.Length; button++)
                {
                    if (nowRelease[button] != lastRelease[button])
                    {
                        ret.Add(new ChangeReleaseSoundEvent(time, nowRelease[button], button + 1));
                    }
                }

                if (now.NoteType != last.NoteType)
                {
                    ret.Add(new ChangeNoteTypeEvent(time, now.NoteType));
                }
                if (now.SlideScale != last.SlideScale)
                {
                    ret.Add(new ChangeSlideScaleEvent(time, now.SlideScale));
                }
                if (!Utility.IsSameArray(now.InitializeOrder, last.InitializeOrder))
                {
                    ret.Add(new ChangeInitializeOrderEvent(time, now.InitializeOrder));
                }

                last = now;
            }
            return [.. ret];
        }
    }
}
