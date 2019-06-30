using FlowScriptEngine;
using PPDCoreModel;
using PPDFramework;

namespace FlowScriptEnginePPD.FlowSourceObjects.Song
{
    [ToolTipText("Song_Info_Summary")]
    public partial class InfoFlowSourceObject : FlowSourceObjectBase
    {
        PPDGameUtility gameUtility;
        IMarkManager markManager;
        ScoreDifficultyMeasureResult result;

        public override string Name
        {
            get { return "PPD.Song.Info"; }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (this.Manager.Items.ContainsKey("PPDGameUtility"))
            {
                gameUtility = this.Manager.Items["PPDGameUtility"] as PPDGameUtility;
            }

            if (this.Manager.Items.ContainsKey("MarkManager"))
            {
                markManager = this.Manager.Items["MarkManager"] as IMarkManager;
            }
        }

        [ToolTipText("Song_Info_IsAuto")]
        public bool IsAuto
        {
            get
            {
                return gameUtility != null && gameUtility.AutoMode != AutoMode.None;
            }
        }

        [ToolTipText("Song_Info_IsConnect")]
        public bool IsConnect
        {
            get
            {
                return gameUtility != null && gameUtility.Connect;
            }
        }

        [ToolTipText("Song_Info_DifficultyString")]
        public string DifficultyString
        {
            get
            {
                return gameUtility != null ? gameUtility.DifficultString : "";
            }
        }

        [ToolTipText("Song_Info_Difficulty")]
        public PPDFrameworkCore.Difficulty Difficulty
        {
            get
            {
                return gameUtility != null ? gameUtility.Difficulty : PPDFrameworkCore.Difficulty.Other;
            }
        }

        [ToolTipText("Song_Info_IsGodMode")]
        public bool IsGodMode
        {
            get
            {
                return gameUtility != null && gameUtility.GodMode;
            }
        }

        [ToolTipText("Song_Info_IsDebug")]
        public bool IsDebug
        {
            get
            {
                return gameUtility != null && gameUtility.IsDebug;
            }
        }

        [ToolTipText("Song_Info_IsRegular")]
        public bool IsRegular
        {
            get
            {
                return gameUtility != null && gameUtility.IsRegular;
            }
        }

        [ToolTipText("Song_Info_IsMuteSE")]
        public bool IsMuteSE
        {
            get
            {
                return gameUtility != null && gameUtility.MuteSE;
            }
        }

        [ToolTipText("Song_Info_IsRandom")]
        public bool IsRandom
        {
            get
            {
                return gameUtility != null && gameUtility.Random;
            }
        }

        [ToolTipText("Song_Info_SpeedScale")]
        public float SpeedScale
        {
            get
            {
                return gameUtility != null ? gameUtility.SpeedScale : 1;
            }
        }

        [ToolTipText("Song_Info_AuthorName")]
        public string AuthorName
        {
            get
            {
                return gameUtility != null ? gameUtility.SongInformation.AuthorName : "";
            }
        }

        [ToolTipText("Song_Info_BPM")]
        public float BPM
        {
            get
            {
                return gameUtility != null ? gameUtility.SongInformation.BPM : 0;
            }
        }

        [ToolTipText("Song_Info_StartTime")]
        public float StartTime
        {
            get
            {
                return gameUtility != null ? gameUtility.SongInformation.StartTime : 0;
            }
        }

        [ToolTipText("Song_Info_EndTime")]
        public float EndTime
        {
            get
            {
                return gameUtility != null ? gameUtility.SongInformation.EndTime : 0;
            }
        }

        [ToolTipText("Song_Info_SongName")]
        public string SongName
        {
            get
            {
                return gameUtility != null ? gameUtility.SongInformation.DirectoryName : "";
            }
        }

        [ToolTipText("Song_Info_AllMarkCount")]
        public int AllMarkCount
        {
            get
            {
                return markManager != null ? markManager.AllMarkCount : 0;
            }
        }

        [ToolTipText("Song_Info_AllLongMarkCount")]
        public int AllLongMarkCount
        {
            get
            {
                return markManager != null ? markManager.AllLongMarkCount : 0;
            }
        }

        [ToolTipText("Song_Info_OneMarkCount")]
        public int OneMarkCount
        {
            get
            {
                return markManager != null ? markManager.GetMarkCount(0) : 0;
            }
        }

        [ToolTipText("Song_Info_TwoMarkCount")]
        public int TwoMarkCount
        {
            get
            {
                return markManager != null ? markManager.GetMarkCount(1) : 0;
            }
        }

        [ToolTipText("Song_Info_ThreeMarkCount")]
        public int ThreeMarkCount
        {
            get
            {
                return markManager != null ? markManager.GetMarkCount(2) : 0;
            }
        }

        [ToolTipText("Song_Info_FourMarkCount")]
        public int FourMarkCount
        {
            get
            {
                return markManager != null ? markManager.GetMarkCount(3) : 0;
            }
        }

        [ToolTipText("Song_Info_FiveMarkCount")]
        public int FiveMarkCount
        {
            get
            {
                return markManager != null ? markManager.GetMarkCount(4) : 0;
            }
        }

        [ToolTipText("Song_Info_SixMarkCount")]
        public int SixMarkCount
        {
            get
            {
                return markManager != null ? markManager.GetMarkCount(5) : 0;
            }
        }

        [ToolTipText("Song_Info_DifficultyPointAverage")]
        public float DifficultyPointAverage
        {
            get
            {
                if (markManager == null)
                {
                    return 0;
                }
                if (result == null)
                {
                    result = markManager.ScoreDifficultyMeasureResult;
                }
                return result != null ? result.Average : 0;
            }
        }

        [ToolTipText("Song_Info_DifficultyPointPeak")]
        public float DifficultyPointPeak
        {
            get
            {
                if (markManager == null)
                {
                    return 0;
                }
                if (result == null)
                {
                    result = markManager.ScoreDifficultyMeasureResult;
                }
                return result != null ? result.Peak : 0;
            }
        }

        [ToolTipText("Song_Info_NoteType", "Song_Info_NoteType_Remark")]
        public PPDFramework.NoteType NoteType
        {
            get
            {
                if (gameUtility == null)
                {
                    return PPDFramework.NoteType.Normal;
                }
                return gameUtility.SongInformation.GetNoteType(gameUtility.Difficulty);
            }
        }

        [ToolTipText("Song_Info_Marks")]
        public object[] Marks
        {
            get
            {
                if (markManager == null)
                {
                    return null;
                }
                return markManager.Marks;
            }
        }
    }
}
