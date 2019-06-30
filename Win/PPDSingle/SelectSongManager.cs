using PPDFramework;
using PPDFramework.Web;
using System;
using System.Linq;

namespace PPDSingle
{
    /// <summary>
    /// 選択した譜面を管理するクラス
    /// </summary>
    class SelectSongManager : UpdatableGameComponent
    {
        public event EventHandler ModeChanged;
        class SelectInfo
        {
            public int Index;

            private static SelectInfo zero = new SelectInfo();
            public static SelectInfo Zero
            {
                get
                {
                    return zero;
                }
            }
        }

        public enum Mode
        {
            SongInfo = 0,
            LogicFolder = 1,
            ActiveScore = 2,
            Contest = 3,
            List = 4,
        }

        public enum SongChangeMode
        {
            Reset,
            Up,
            Down
        }

        Mode mode = Mode.SongInfo;

        public event Action<SongChangeMode> SongChanged;
        public event EventHandler DirectoryChanged;

        SelectInfo songSelect;
        SelectInfo logicSelect;
        SelectInfo contestSelect;
        SelectInfo activeScoreSelect;
        SelectInfo listInfoSelect;
        SelectedSongInfo[] songSelects;
        LogicSelectedSongInfo[] logicSelects;
        ContestSelectedSongInfo[] contestSelects;
        ActiveScoreSelectedSongInfo[] activeScoreSelects;
        ListInfoSelectedSongInfo[] listInfoSelects;

        SongInformation currentRoot;
        LogicFolderInfomation currentLogicRoot;
        ListInfo currentList;

        ContestInfo contestInfo;
        WebSongInformation[] activeScores;
        ListInfo[] listInfos;

        bool updateFlag;

        public SelectSongManager(ContestInfo contestInfo, WebSongInformation[] activeScores, ListInfo[] listInfos)
        {
            this.contestInfo = contestInfo;
            this.activeScores = activeScores;
            this.listInfos = listInfos;
            Filter = new SongSelectFilter();
            songSelect = new SelectInfo();
            logicSelect = new SelectInfo();
            contestSelect = new SelectInfo();
            activeScoreSelect = new SelectInfo();
            listInfoSelect = new SelectInfo();
            GenerateSelectContests();
            GenerateSelectActiveScores();
            GenerateSelectLists();
            SongInformation.Updated += SongInformation_Updated;
            LogicFolderInfomation.StaticAfterAdd += LogicFolderInfomation_StaticAfterAdd;
            LogicFolderInfomation.StaticBeforeRemove += LogicFolderInfomation_StaticBeforeRemove;
            LogicFolderInfomation.StaticBeforeChangeIndex += LogicFolderInfomation_StaticBeforeChangeIndex;
        }

        void SongInformation_Updated(object sender, EventArgs e)
        {
            songSelect = new SelectInfo();
            logicSelect = new SelectInfo();
            contestSelect = new SelectInfo();
            activeScoreSelect = new SelectInfo();
            listInfoSelect = new SelectInfo();

            currentRoot = SongInformation.Root;
            currentLogicRoot = LogicFolderInfomation.Root;
            GenerateSelectSongs();
            GenerateSelectLogics();
            GenerateSelectContests();
            GenerateSelectActiveScores();
            GenerateSelectLists();

            if (contestSelects != null && contestSelects.Length > 0)
            {
                contestSelects[0].UpdateSongInfo();
            }
            if (activeScores != null && activeScores.Length > 0)
            {
                foreach (var score in activeScoreSelects)
                {
                    score.UpdateSongInfo();
                }
            }
            if (listInfoSelects != null && listInfoSelects.Length > 0)
            {
                foreach (var select in listInfoSelects)
                {
                    select.UpdateSongInfo();
                }
            }
            OnDirectoryChanged();
            OnSongChanged(SongChangeMode.Reset);
        }

        void LogicFolderInfomation_StaticAfterAdd(object sender, EventArgs e)
        {
            var info = sender as LogicFolderInfomation;
            if (info.Parent == CurrentLogicRoot)
            {
                updateFlag = true;
            }
        }

        void LogicFolderInfomation_StaticBeforeChangeIndex(object sender, EventArgs e)
        {
            var info = sender as LogicFolderInfomation;
            if (logicSelects.FindIndex(link => link.LogicFolderInfomation == info) >= 0)
            {
                updateFlag = true;
            }
        }

        void LogicFolderInfomation_StaticBeforeRemove(object sender, EventArgs e)
        {
            var info = sender as LogicFolderInfomation;
            if (info.ContainAsChildrenOrSelf(CurrentLogicRoot))
            {
                if (info.Parent.Parent != null)
                {
                    CurrentLogicRoot = info.Parent.Parent;
                }
                else
                {
                    CurrentLogicRoot = info.Parent;
                }
                updateFlag |= mode == Mode.LogicFolder;
            }
            else if (logicSelects != null)
            {
                for (int i = 0; i < logicSelects.Length; i++)
                {
                    if (logicSelects[i].LogicFolderInfomation == info)
                    {
                        if (logicSelect.Index >= i)
                        {
                            logicSelect.Index--;
                            if (logicSelect.Index < 0) logicSelect.Index = 0;
                        }
                        else if (logicSelect.Index < i)
                        {
                            // nothing
                        }
                        updateFlag = true;
                        break;
                    }
                }
            }
        }

        public override void Update()
        {
            if (disposed) return;
            if (updateFlag)
            {
                updateFlag = false;
                GenerateSelectLogics();
                OnDirectoryChanged();
                OnSongChanged(SongChangeMode.Reset);
            }
        }

        protected override void DisposeResource()
        {
            LogicFolderInfomation.StaticAfterAdd -= LogicFolderInfomation_StaticAfterAdd;
            LogicFolderInfomation.StaticBeforeRemove -= LogicFolderInfomation_StaticBeforeRemove;
            LogicFolderInfomation.StaticBeforeChangeIndex -= LogicFolderInfomation_StaticBeforeChangeIndex;
            SongInformation.Updated -= SongInformation_Updated;
        }

        private SelectInfo CurrentSelect
        {
            get
            {
                switch (mode)
                {
                    case Mode.SongInfo:
                        return songSelect;
                    case Mode.LogicFolder:
                        return logicSelect;
                    case Mode.Contest:
                        return contestSelect;
                    case Mode.ActiveScore:
                        return activeScoreSelect;
                    case Mode.List:
                        return listInfoSelect;
                }
                return SelectInfo.Zero;
            }
        }

        private void OnSongChanged(SongChangeMode songChangeMode)
        {
            if (SongChanged != null)
            {
                SongChanged.Invoke(songChangeMode);
            }
        }
        private void OnDirectoryChanged()
        {
            if (DirectoryChanged != null) DirectoryChanged.Invoke(this, EventArgs.Empty);
        }
        private void OnModeChanged()
        {
            if (ModeChanged != null) ModeChanged.Invoke(this, EventArgs.Empty);
        }

        public SongInformation CurrentRoot
        {
            get
            {
                return currentRoot;
            }
            set
            {
                currentRoot = value;
                GenerateSelectSongs();
                OnDirectoryChanged();
            }
        }

        public LogicFolderInfomation CurrentLogicRoot
        {
            get
            {
                return currentLogicRoot;
            }
            set
            {
                currentLogicRoot = value;
                GenerateSelectLogics();
                OnDirectoryChanged();
            }
        }

        public ListInfo CurrentList
        {
            get { return currentList; }
            set
            {
                currentList = value;
                GenerateSelectLists();
                OnDirectoryChanged();
            }
        }

        public void ChangeMode(bool forward)
        {
            if (forward)
            {
                mode++;
                if (mode > Mode.List)
                {
                    mode = Mode.SongInfo;
                }
            }
            else
            {
                mode--;
                if (mode < 0)
                {
                    mode = Mode.List;
                }
            }
            OnDirectoryChanged();
            OnSongChanged(SongChangeMode.Reset);
            OnModeChanged();
        }

        public void ChangeMode(Mode mode)
        {
            if (this.mode != mode)
            {
                this.mode = mode;
                OnDirectoryChanged();
                OnSongChanged(SongChangeMode.Reset);
                OnModeChanged();
            }
        }

        /// <summary>
        /// 譜面リスト
        /// </summary>
        public SelectedSongInfo[] SongInformations
        {
            get
            {
                switch (mode)
                {
                    case Mode.SongInfo:
                        return songSelects;
                    case Mode.LogicFolder:
                        return logicSelects;
                    case Mode.Contest:
                        return contestSelects;
                    case Mode.ActiveScore:
                        return activeScoreSelects;
                    case Mode.List:
                        return listInfoSelects;
                }
                return null;
            }
        }

        public SongSelectFilter Filter
        {
            get;
            private set;
        }

        public int Count
        {
            get
            {
                switch (mode)
                {
                    case Mode.SongInfo:
                        return songSelects.Length;
                    case Mode.LogicFolder:
                        return logicSelects.Length;
                    case Mode.Contest:
                        return contestSelects.Length;
                    case Mode.ActiveScore:
                        return activeScoreSelects.Length;
                    case Mode.List:
                        return listInfoSelects.Length;
                }
                return 0;
            }
        }

        public Mode CurrentMode
        {
            get
            {
                return mode;
            }
        }

        /// <summary>
        /// 選択した譜面のインデックス
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                return CurrentSelect.Index;
            }
            set
            {
                SelectInfo si = CurrentSelect;
                var prev = si.Index;
                si.Index = value;
                OnSongChanged(si.Index >= prev ? SongChangeMode.Down : SongChangeMode.Up);
            }
        }

        public SelectedSongInfo SelectedSongInformation
        {
            get
            {
                switch (mode)
                {
                    case Mode.SongInfo:
                        if (songSelects.Length == 0)
                        {
                            return null;
                        }
                        else
                        {
                            return songSelects[songSelect.Index];
                        }
                    case Mode.LogicFolder:
                        if (logicSelects.Length == 0)
                        {
                            return null;
                        }
                        else
                        {
                            return logicSelects[logicSelect.Index];
                        }
                    case Mode.Contest:
                        if (contestSelects.Length == 0)
                        {
                            return null;
                        }
                        else
                        {
                            return contestSelects[contestSelect.Index];
                        }
                    case Mode.ActiveScore:
                        if (activeScoreSelects.Length == 0)
                        {
                            return null;
                        }
                        else
                        {
                            return activeScoreSelects[activeScoreSelect.Index];
                        }
                    case Mode.List:
                        if (listInfoSelects.Length == 0)
                        {
                            return null;
                        }
                        else
                        {
                            return listInfoSelects[listInfoSelect.Index];
                        }
                }
                return null;
            }
        }

        public void GenerateSelectSongs()
        {
            songSelects = Filter.GetFiltered(CurrentRoot.Children).Select(info => new SelectedSongInfo(info)).ToArray();
        }

        public void GenerateSelectLogics()
        {
            logicSelects = new LogicSelectedSongInfo[CurrentLogicRoot.ChildrenCount];
            int iter = 0;
            foreach (LogicFolderInfomation child in CurrentLogicRoot.Children)
            {
                logicSelects[iter] = new LogicSelectedSongInfo(child);
                iter++;
            }
        }

        public void GenerateSelectContests()
        {
            if (contestInfo != null)
            {
                contestSelects = new ContestSelectedSongInfo[1];
                contestSelects[0] = new ContestSelectedSongInfo(contestInfo);
            }
            else
            {
                contestSelects = new ContestSelectedSongInfo[0];
            }
        }

        public void GenerateSelectActiveScores()
        {
            if (activeScores != null)
            {
                activeScoreSelects = activeScores.Select(a => new ActiveScoreSelectedSongInfo(a)).ToArray();
            }
            else
            {
                activeScoreSelects = new ActiveScoreSelectedSongInfo[0];
            }
        }

        public void GenerateSelectLists()
        {
            if (CurrentList != null)
            {
                listInfoSelects = CurrentList.Scores.Select(s => new ListInfoSelectedSongInfo(s)).ToArray();
            }
            else
            {
                listInfoSelects = listInfos.Select(l => new ListInfoSelectedSongInfo(l)).ToArray();
            }
            listInfoSelects = new SelectedSongInfoFilter(Filter).GetFiltered<ListInfoSelectedSongInfo>(listInfoSelects);
        }

        public void Regenerate()
        {
            GoTop();
            switch (mode)
            {
                case Mode.SongInfo:
                    songSelect.Index = 0;
                    GenerateSelectSongs();
                    break;
                case Mode.LogicFolder:
                    songSelect.Index = 0;
                    GenerateSelectLogics();
                    break;
                case Mode.Contest:
                    songSelect.Index = 0;
                    GenerateSelectContests();
                    break;
                case Mode.ActiveScore:
                    songSelect.Index = 0;
                    GenerateSelectActiveScores();
                    break;
                case Mode.List:
                    listInfoSelect.Index = 0;
                    GenerateSelectLists();
                    break;
            }
            OnDirectoryChanged();
            OnSongChanged(SongChangeMode.Reset);
        }

        /// <summary>
        /// 次の譜面
        /// </summary>
        public void NextSong()
        {
            SelectInfo si = CurrentSelect;
            si.Index++;
            if (si.Index >= Count) si.Index = 0;
            OnSongChanged(SongChangeMode.Down);
        }

        /// <summary>
        /// 前の譜面
        /// </summary>
        public void PreviousSong()
        {
            SelectInfo si = CurrentSelect;
            si.Index--;
            if (si.Index < 0) si.Index = Count - 1;
            OnSongChanged(SongChangeMode.Up);
        }

        /// <summary>
        /// 選択譜面を指定した数だけずらす
        /// </summary>
        /// <param name="move"></param>
        public void SeekSong(int move)
        {
            if (Count == 0)
            {
                return;
            }
            SelectInfo si = CurrentSelect;
            si.Index += move;
            while (si.Index < 0)
            {
                si.Index += Count;
            }
            while (si.Index >= Count)
            {
                si.Index -= Count;
            }
            OnSongChanged(move >= 0 ? SongChangeMode.Down : SongChangeMode.Up);
        }

        /// <summary>
        /// 下のディレクトリに進む
        /// </summary>
        public void GoDownDirectory()
        {
            if (!CanGoDown) return;
            switch (mode)
            {
                case Mode.SongInfo:
                    currentRoot = SelectedSongInformation.SongInfo;
                    songSelect.Index = 0;
                    GenerateSelectSongs();
                    break;
                case Mode.LogicFolder:
                    currentLogicRoot = (SelectedSongInformation as LogicSelectedSongInfo).LogicFolderInfomation;
                    logicSelect.Index = 0;
                    GenerateSelectLogics();
                    break;
                case Mode.Contest:
                case Mode.ActiveScore:
                    break;
                case Mode.List:
                    currentList = (SelectedSongInformation as ListInfoSelectedSongInfo).ListInfo;
                    listInfoSelect.Index = 0;
                    GenerateSelectLists();
                    break;
            }
            OnDirectoryChanged();
            OnSongChanged(SongChangeMode.Reset);
        }

        /// <summary>
        /// 上のディレクトリに戻る
        /// </summary>
        public void GoUpDirectory()
        {
            if (!CanGoUp) return;
            switch (mode)
            {
                case Mode.SongInfo:
                    SongInformation tempSongRoot = currentRoot;
                    currentRoot = currentRoot.Parent;
                    GenerateSelectSongs();
                    songSelect.Index = songSelects.FindIndex(info => info.SongInfo == tempSongRoot);
                    break;
                case Mode.LogicFolder:
                    LogicFolderInfomation tempLogicRoot = currentLogicRoot;
                    CurrentLogicRoot = CurrentLogicRoot.Parent;
                    GenerateSelectLogics();
                    logicSelect.Index = logicSelects.FindIndex(info => info.LogicFolderInfomation == tempLogicRoot);
                    break;
                case Mode.Contest:
                case Mode.ActiveScore:
                    break;
                case Mode.List:
                    var prevList = currentList;
                    currentList = null;
                    GenerateSelectLists();
                    listInfoSelect.Index = listInfos.FindIndex(info => info == prevList);
                    break;
            }
            OnDirectoryChanged();
            OnSongChanged(SongChangeMode.Reset);
        }

        public void GoTop()
        {
            while (CanGoUp)
            {
                GoUpDirectory();
            }
        }

        public void RandomSelect(RandomSelectType randomSelectType)
        {
            if (CurrentMode == Mode.SongInfo)
            {
                var songs = randomSelectType == RandomSelectType.InAll ? SongInformation.All : CurrentRoot.Children.Where(s => s.IsPPDSong).ToArray();
                var linkAll = Filter.GetFiltered(songs).Select(info => new SelectedSongInfo(info)).ToArray();
                if (linkAll.Length == 0)
                {
                    return;
                }

                var rand = new Random();
                SongInformation target = linkAll[rand.Next(linkAll.Length)].SongInfo;
                CurrentRoot = target.Parent;
                GenerateSelectSongs();
                CurrentSelect.Index = songSelects.FindIndex(link => link.SongInfo == target);
                OnDirectoryChanged();
                OnSongChanged(SongChangeMode.Reset);
            }
            else if (CurrentMode == Mode.LogicFolder)
            {
                var links = randomSelectType == RandomSelectType.InAll ? LogicFolderInfomation.All : CurrentLogicRoot.Children.Where(l => !l.IsFolder).ToArray();
                var linkAll = links.Select(info => new LogicSelectedSongInfo(info)).ToArray();
                if (linkAll.Length == 0)
                {
                    return;
                }

                var rand = new Random();
                LogicFolderInfomation target = linkAll[rand.Next(linkAll.Length)].LogicFolderInfomation;
                CurrentLogicRoot = target.Parent;
                GenerateSelectLogics();
                logicSelect.Index = logicSelects.FindIndex(link => link.LogicFolderInfomation == target);
                OnDirectoryChanged();
                OnSongChanged(SongChangeMode.Reset);
            }
            else if (CurrentMode == Mode.ActiveScore)
            {
                var rand = new Random();
                activeScoreSelect.Index = rand.Next(activeScores.Length);
                OnDirectoryChanged();
                OnSongChanged(SongChangeMode.Reset);
            }
            else if (CurrentMode == Mode.List)
            {
                var rand = new Random();
                switch (randomSelectType)
                {
                    case RandomSelectType.InCurrentFolder:
                        listInfoSelect.Index = rand.Next(listInfoSelects.Length);
                        OnSongChanged(SongChangeMode.Reset);
                        break;
                    case RandomSelectType.InAll:
                        var listScores = listInfos.SelectMany(l => l.Scores).ToArray();
                        var target = listScores[rand.Next(listScores.Length)];
                        currentList = target.Parent;
                        listInfoSelect.Index = currentList.Scores.FindIndex(s => s == target);
                        GenerateSelectLists();
                        OnDirectoryChanged();
                        OnSongChanged(SongChangeMode.Reset);
                        break;
                }
            }
        }

        public bool CanSelect
        {
            get
            {
                switch (mode)
                {
                    case Mode.SongInfo:
                        return songSelects.Length > 0;
                    case Mode.LogicFolder:
                        if (SelectedSongInformation == null)
                        {
                            return false;
                        }
                        return !SelectedSongInformation.IsFolder || (SelectedSongInformation as LogicSelectedSongInfo).LogicFolderInfomation.ChildrenCount > 0;
                    case Mode.Contest:
                        if (SelectedSongInformation == null)
                        {
                            return false;
                        }
                        return SelectedSongInformation.SongInfo != null;
                    case Mode.ActiveScore:
                        if (SelectedSongInformation == null)
                        {
                            return false;
                        }
                        return SelectedSongInformation.SongInfo != null;
                    case Mode.List:
                        if (SelectedSongInformation == null)
                        {
                            return false;
                        }
                        return SelectedSongInformation.SongInfo != null || (SelectedSongInformation as ListInfoSelectedSongInfo).IsFolder;
                }
                return false;
            }
        }

        public bool CanGoDown
        {
            get
            {
                return SelectedSongInformation.IsFolder;
            }
        }

        public bool CanGoUp
        {
            get
            {
                switch (mode)
                {
                    case Mode.SongInfo:
                        return CurrentRoot.Parent != null;
                    case Mode.LogicFolder:
                        return CurrentLogicRoot.Parent != null;
                    case Mode.Contest:
                    case Mode.ActiveScore:
                        return false;
                    case Mode.List:
                        return CurrentList != null;
                }
                return false;
            }
        }
    }
}
