using System.Collections.Generic;
using System.Linq;

namespace PPDEditorCommon.Dialog.Model
{
    public class SoundManagerInfo
    {
        readonly List<string> sounds;
        readonly SortedList<float, ushort[]> changes;

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
            sounds = [];
            changes = [];
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
