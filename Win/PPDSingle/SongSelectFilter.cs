using PPDFramework;
using System;
using System.Globalization;
using System.Linq;

namespace PPDSingle
{
    class SongSelectFilter
    {
        public enum SortField
        {
            Name,
            Time,
            UpdateDate,
            BPM,
            Author,
        }

        [Flags]
        public enum ScoreType
        {
            None = 0,
            Normal = 1,
            AC = 2,
            ACFT = 4,
        }

        public SortField Field
        {
            get;
            set;
        }

        public bool Desc
        {
            get;
            set;
        }

        public PPDFramework.SongInformation.AvailableDifficulty Difficulty
        {
            get;
            set;
        }

        public ScoreType Type
        {
            get;
            set;
        }

        public SongSelectFilter()
        {
            Field = SortField.Name;
            Desc = false;
            Difficulty = SongInformation.AvailableDifficulty.Easy | SongInformation.AvailableDifficulty.Normal |
                SongInformation.AvailableDifficulty.Hard | SongInformation.AvailableDifficulty.Extreme;
            Type = ScoreType.Normal | ScoreType.AC | ScoreType.ACFT;
        }

        public bool Filter(SongInformation info)
        {
            if (!info.IsPPDSong)
            {
                return true;
            }

            if (Type == ScoreType.None)
            {
                return false;
            }

            bool ok = (Type.HasFlag(ScoreType.ACFT) && info.HasACFT);
            ok |= (Type.HasFlag(ScoreType.AC) && info.HasAC);
            ok |= (Type.HasFlag(ScoreType.Normal) && info.HasNormal);

            if (!ok)
            {
                return false;
            }

            return ok |= (Difficulty.HasFlag(PPDFramework.SongInformation.AvailableDifficulty.Easy) && info.Difficulty.HasFlag(PPDFramework.SongInformation.AvailableDifficulty.Easy)) ||
                (Difficulty.HasFlag(PPDFramework.SongInformation.AvailableDifficulty.Normal) && info.Difficulty.HasFlag(PPDFramework.SongInformation.AvailableDifficulty.Normal)) ||
                (Difficulty.HasFlag(PPDFramework.SongInformation.AvailableDifficulty.Hard) && info.Difficulty.HasFlag(PPDFramework.SongInformation.AvailableDifficulty.Hard)) ||
                (Difficulty.HasFlag(PPDFramework.SongInformation.AvailableDifficulty.Extreme) && info.Difficulty.HasFlag(PPDFramework.SongInformation.AvailableDifficulty.Extreme));
        }

        public int Compare(SongInformation info1, SongInformation info2)
        {
            int ret;
            if (!info1.IsPPDSong && !info2.IsPPDSong)
            {
                ret = StringComparer.InvariantCulture.Compare(info1.DirectoryName, info2.DirectoryName);
            }
            else if (!info1.IsPPDSong)
            {
                ret = -1;
            }
            else if (!info2.IsPPDSong)
            {
                ret = 1;
            }
            else
            {
                switch (Field)
                {
                    case SortField.Name:
                        ret = StringComparer.InvariantCulture.Compare(info1.DirectoryName, info2.DirectoryName);
                        break;
                    case SortField.Time:
                        ret = Math.Sign(info1.EndTime - info1.StartTime - info2.EndTime + info2.StartTime);
                        break;
                    case SortField.UpdateDate:
                        DateTime date1 = DateTime.Parse(info1.UpdateDate, CultureInfo.InvariantCulture),
                            date2 = DateTime.Parse(info2.UpdateDate, CultureInfo.InvariantCulture);
                        ret = DateTime.Compare(date1, date2);
                        break;
                    case SortField.BPM:
                        ret = Math.Sign(info1.BPM - info2.BPM);
                        break;
                    case SortField.Author:
                        ret = StringComparer.InvariantCulture.Compare(info1.AuthorName, info2.AuthorName);
                        break;
                    default:
                        ret = 0;
                        break;
                }
                if (ret == 0)
                {
                    ret = StringComparer.InvariantCulture.Compare(info1.DirectoryName, info2.DirectoryName);
                }
            }
            return (Desc ? -1 : 1) * ret;
        }

        public SongInformation[] GetFiltered(SongInformation[] infos)
        {
            infos = infos.Where(Filter).ToArray();
            Array.Sort(infos, Compare);
            return infos;
        }
    }
}
