using PPDFrameworkCore;
using System.Text;

namespace PPDExpansionCore
{
    public class ScoreInfo : PackableBase
    {
        public override DataType DataType
        {
            get { return DataType.ScoreInfo; }
        }

        public PlayType PlayType
        {
            get;
            set;
        }

        public byte[] ScoreHash
        {
            get;
            set;
        }

        public string ScoreName
        {
            get;
            set;
        }

        public Difficulty Difficulty
        {
            get;
            set;
        }

        [Ignore]
        public string ScoreHashAsString
        {
            get
            {
                var sb = new StringBuilder();
                foreach (var b in ScoreHash)
                {
                    sb.AppendFormat("{0:X2}", b);
                }
                return sb.ToString();
            }
        }

        public float StartTime
        {
            get;
            set;
        }

        public float EndTime
        {
            get;
            set;
        }

        public float Length
        {
            get
            {
                return EndTime - StartTime;
            }
        }

        public int UserHighScore
        {
            get;
            set;
        }

        public int WebHighScore
        {
            get;
            set;
        }

        public bool IsRegular
        {
            get;
            set;
        }
    }
}
