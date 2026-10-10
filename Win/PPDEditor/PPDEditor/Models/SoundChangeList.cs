using PPDFramework;
using PPDFramework.PPDStructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PPDEditor.Models
{
    /// <summary>
    /// ボタンを押したときに鳴らす音の一覧と、時間ごとの音の割り当て（ボタンごとの音の番号）です。
    /// </summary>
    /// <remarks>
    /// 音の番号の 0 は無音です。<see cref="Paths"/> の 0 番目は無音の場所で、ファイルではありません。
    /// </remarks>
    public class SoundChangeList
    {
        public const int ButtonCount = 10;

        readonly List<string> paths = [""];
        readonly SortedList<float, ushort[]> changes = [];

        /// <summary>
        /// 音のファイルのパスです（0 番目は無音）。
        /// </summary>
        public IReadOnlyList<string> Paths
        {
            get { return paths; }
        }

        public int Count
        {
            get { return changes.Count; }
        }

        public IList<float> Times
        {
            get { return changes.Keys; }
        }

        public IList<ushort[]> Changes
        {
            get { return changes.Values; }
        }

        public bool Contains(float time)
        {
            return changes.ContainsKey(time);
        }

        public int IndexOf(float time)
        {
            return changes.IndexOfKey(time);
        }

        /// <summary>
        /// 音のファイルを足します。
        /// </summary>
        /// <returns>新しく読み込む必要があるか（同じファイルがまだないか）。</returns>
        public bool AddSound(string path)
        {
            var isNew = !paths.Contains(path);
            paths.Add(path);
            return isNew;
        }

        /// <summary>
        /// 音を消します。その音を使っている割り当ては無音にし、後ろの音の番号は詰めます。
        /// </summary>
        /// <returns>消した音のファイルがもう使われていないか（読み込んだ音を消してよいか）。</returns>
        public bool RemoveSound(int index)
        {
            var path = paths[index];
            paths.RemoveAt(index);
            foreach (var change in changes.Values)
            {
                for (int j = 0; j < change.Length; j++)
                {
                    change[j] = ShiftIndex(change[j], index);
                }
            }
            return !paths.Contains(path);
        }

        /// <summary>
        /// 音を消したときの、割り当ての番号です。
        /// </summary>
        public static ushort ShiftIndex(ushort soundIndex, int removedIndex)
        {
            if (soundIndex == removedIndex)
            {
                return 0;
            }
            return soundIndex > removedIndex ? (ushort)(soundIndex - 1) : soundIndex;
        }

        /// <summary>
        /// ほかの難易度の割り当てのファイル（.scd）からも、消した音を外します。
        /// </summary>
        public static void RemoveSoundFromFile(string scdPath, int removedIndex)
        {
            if (!File.Exists(scdPath))
            {
                return;
            }
            var data = SCDReader.Read(scdPath);
            SCDWriter.Write(scdPath, [.. data.Select(d => new SCDData(d.Time, d.ButtonType, ShiftIndex(d.SoundIndex, removedIndex)))]);
        }

        /// <summary>
        /// 音のファイルを入れ替えます。
        /// </summary>
        public void ReplaceSound(int index, string path)
        {
            paths[index] = path;
        }

        /// <summary>
        /// 割り当てを足します。同じ時間に割り当てがあるときは足さずに false を返します。
        /// </summary>
        public bool AddChange(float time, ushort[] change)
        {
            if (changes.ContainsKey(time))
            {
                return false;
            }
            changes.Add(time, change);
            return true;
        }

        public void SetChange(int index, ushort[] change)
        {
            changes[changes.Keys[index]] = change;
        }

        public void RemoveChangeAt(int index)
        {
            changes.RemoveAt(index);
        }

        /// <summary>
        /// 割り当ての時間を変えます。前の時間に割り当てがないときや、新しい時間に割り当てがあるときは変えずに false を返します。
        /// </summary>
        public bool MoveChange(float previousTime, float newTime)
        {
            if (!changes.TryGetValue(previousTime, out ushort[] change) || changes.ContainsKey(newTime))
            {
                return false;
            }
            changes.Remove(previousTime);
            changes.Add(newTime, change);
            return true;
        }

        /// <summary>
        /// 最初の割り当て以外を消します。
        /// </summary>
        public void ClearChanges()
        {
            while (changes.Count > 1)
            {
                changes.RemoveAt(changes.Count - 1);
            }
        }

        /// <summary>
        /// 音も割り当ても（最初の割り当て以外）消します。
        /// </summary>
        public void Clear()
        {
            paths.RemoveRange(1, paths.Count - 1);
            ClearChanges();
        }

        /// <summary>
        /// その時間に有効な割り当ての番号です（最初の割り当てより前でも 0）。
        /// </summary>
        public int GetIndexAt(float time)
        {
            int i;
            for (i = 0; i < changes.Count; i++)
            {
                if (changes.Keys[i] > time)
                {
                    break;
                }
            }
            return Math.Max(i - 1, 0);
        }

        public float[] GetTimesWithin(float startTime, float endTime)
        {
            var ret = new List<float>();
            foreach (float time in changes.Keys)
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

        /// <summary>
        /// 音のファイル名の一覧（プロジェクトに保存するもの）です。
        /// </summary>
        public string SerializeSounds()
        {
            return String.Join(Environment.NewLine, paths.Skip(1).Select(Path.GetFileName));
        }

        /// <summary>
        /// 譜面の音の割り当てのデータ（scd）にします。前の割り当てから変わったボタンだけを書きます。
        /// </summary>
        public SCDData[] ToSCDData()
        {
            var ret = new List<SCDData>();
            var last = changes.Values[0];
            for (int i = 1; i < changes.Count; i++)
            {
                var now = changes.Values[i];
                for (int j = 0; j < now.Length; j++)
                {
                    if (last[j] != now[j])
                    {
                        ret.Add(new SCDData(changes.Keys[i], (ButtonType)j, now[j]));
                    }
                }
                last = now;
            }
            return [.. ret];
        }
    }
}
