using System;
using System.Linq;

namespace PPDSingle
{
    class SelectedSongInfoFilter
    {
        private SongSelectFilter filter;

        public SelectedSongInfoFilter(SongSelectFilter filter)
        {
            this.filter = filter;
        }

        public int Compare(SelectedSongInfo info1, SelectedSongInfo info2)
        {
            var desc = filter.Desc ? -1 : 1;
            if (info1.SongInfo == null)
            {
                if (info2.SongInfo == null)
                {
                    return desc * String.Compare(info1.Text, info2.Text);
                }
                else
                {
                    return -desc;
                }
            }
            else
            {
                if (info2.SongInfo == null)
                {
                    return desc;
                }
                else
                {
                    return filter.Compare(info1.SongInfo, info2.SongInfo);
                }
            }
        }

        public T[] GetFiltered<T>(T[] infos) where T : SelectedSongInfo
        {
            infos = infos.Where(i => i.SongInfo == null || filter.Filter(i.SongInfo)).ToArray();
            Array.Sort(infos, Compare);
            return infos;

        }
    }
}
