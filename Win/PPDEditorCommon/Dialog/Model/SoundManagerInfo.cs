using System.Collections.Generic;
using System.Linq;

namespace PPDEditorCommon.Dialog.Model
{
    public class SoundManagerInfo
    {
        List<string> sounds;
        SortedList<float, ushort[]> changes;

        public string[] Sounds
        {
            get
            {
                return sounds.ToArray();
            }
        }

        public KeyValuePair<float, ushort[]>[] Changes
        {
            get
            {
                return changes.ToArray();
            }
        }

        public SoundManagerInfo()
        {
            sounds = new List<string>();
            changes = new SortedList<float, ushort[]>();
        }

        public void Add(string filePath)
        {
            sounds.Add(filePath);
        }

        public void AddChange(float time, ushort[] indexes)
        {
            if (!changes.ContainsKey(time))
            {
                changes[time] = indexes;
            }
        }
    }
}
