using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPDEditor.FlowScript;
using PPDEditor.ViewModels.Dialogs;
using PPDEditorCommon;
using PPDFramework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using Point = Vortice.Mathematics.Int2;

namespace PPDEditor.ViewModels
{
    // 動画、ゲーム（PPD）の起動、コマンド（FlowScript）の実行
    public partial class MainWindowViewModel : IMovieManager, IDisposable
    {
        long movielastime;
        bool moviefirstplayafterinitialized = true;
        Process currentProcess;
        Executor currentExecutor;
        Storage storage;

        public event Action ProcessStarted;
        public event Action ProcessExited;
        public event Action<string> CommandExecuting;
        public event Action<string> CommandExecuted;

        /// <summary>
        /// PPD を動かしているかどうか。
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StartGameCommand))]
        [NotifyCanExecuteChangedFor(nameof(StartGameFromMenuCommand))]
        [NotifyCanExecuteChangedFor(nameof(KillProcessCommand))]
        public partial bool IsProcessRunning { get; set; }

        /// <summary>
        /// コマンドを実行しているかどうか（その間はコマンドとゲームのメニューを使えない）。
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(StartGameCommand))]
        [NotifyCanExecuteChangedFor(nameof(StartGameFromMenuCommand))]
        [NotifyPropertyChangedFor(nameof(IsMenuOfCommandsEnabled))]
        public partial bool IsCommandRunning { get; set; }

        /// <summary>
        /// コマンドとゲームのメニューを使えるかどうか。
        /// </summary>
        public bool IsMenuOfCommandsEnabled
        {
            get { return !IsCommandRunning; }
        }

        /// <summary>
        /// PPD を起動するときに、ネットにつなぐかどうか。
        /// </summary>
        [ObservableProperty]
        public partial bool Connect { get; set; }

        /// <summary>
        /// PPD を起動するときに、Expansion を起動しないかどうか。
        /// </summary>
        [ObservableProperty]
        public partial bool DisableExpansion { get; set; }

        /// <summary>
        /// コマンドのメニューです（Commands のフォルダの FlowScript）。
        /// </summary>
        public ObservableCollection<CommandMenuItem> Commands { get; } = [];

        /// <summary>
        /// MOD のメニューです（mods のフォルダ）。
        /// </summary>
        public ObservableCollection<ModMenuItem> Mods { get; } = [];

        public MyGame MyGame { get; set; }

        public void Dispose()
        {
            currentProcess?.Dispose();
            currentProcess = null;
            GC.SuppressFinalize(this);
        }

        #region 動画

        public double CurrentTime
        {
            get { return MyGame.Movie != null && MyGame.Movie.Initialized ? MyGame.Movie.MoviePosition : 0; }
        }

        public double MovieLength
        {
            get { return MyGame.Movie?.Length ?? 0; }
        }

        public int MovieWidth
        {
            get { return MyGame.Movie?.MovieWidth ?? 0; }
        }

        public int MovieHeight
        {
            get { return MyGame.Movie?.MovieHeight ?? 0; }
        }

        public bool MovieInitialized
        {
            get { return MyGame.Movie?.Initialized ?? false; }
        }

        public double MoviePosition
        {
            get { return MyGame.Movie?.MoviePosition ?? 0; }
        }

        public string MovieFileName
        {
            get { return MyGame.Movie?.FileName ?? ""; }
        }

        public MovieTrimmingData MovieTrimmingData
        {
            get { return MyGame.Movie?.TrimmingData; }
            set { MyGame.Movie?.TrimmingData = value; }
        }

        public string CurrentKasi
        {
            get { return WindowUtility.KasiEditor.Askcurrentkasi(); }
        }

        private void ReadMovie(string filename)
        {
            if (!MyGame.InitializeMovie(filename))
            {
                return;
            }
            UpdateMovieStatus();
            WindowUtility.Seekmain.Length = MovieLength;
            Info.SetMovieInfo(MovieWidth.ToString(), MovieHeight.ToString(), GetTimeDisplay(MovieLength));
            UpdateProjectState();
            moviefirstplayafterinitialized = true;
        }

        [RelayCommand]
        public void Play()
        {
            OnUIThread(() =>
            {
                if (MovieInitialized)
                {
                    if (moviefirstplayafterinitialized)
                    {
                        moviefirstplayafterinitialized = false;
                        ChangeVolume(-1000);
                    }
                    MyGame.StartPlay();
                    MyGame.Movie?.Play();
                }
            });
        }

        [RelayCommand]
        public void Pause()
        {
            OnUIThread(() =>
            {
                if (MovieInitialized)
                {
                    MyGame.Movie?.Pause();
                    MyGame.StopPlay();
                    MyGame.StopRecord();
                }
            });
        }

        [RelayCommand]
        public void Stop()
        {
            OnUIThread(() =>
            {
                if (MovieInitialized)
                {
                    MyGame.Movie?.Stop();
                    SeekMovie(0);
                    MyGame.Movie?.Play();
                    MyGame.Movie?.Pause();
                    MyGame.StopRecord();
                    MyGame.StopPlay();
                    WindowUtility.Seekmain.Currenttime = 0;
                    WindowUtility.Seekmain.ResetScroll();
                }
            });
        }

        public void Seek(double time)
        {
            OnUIThread(() => SeekMovie(time));
        }

        public void PlayOrPause()
        {
            if (!MyGame.Playing)
            {
                Play();
            }
            else
            {
                Pause();
            }
        }

        /// <summary>
        /// 打鍵を記録します（記録しているときは止める）。
        /// </summary>
        [RelayCommand]
        private void RecordKey()
        {
            if (!MovieInitialized)
            {
                return;
            }
            if (MyGame.Recording)
            {
                MyGame.StopRecord();
                MyGame.Movie?.Pause();
            }
            else
            {
                MyGame.StartRecord();
                MyGame.StartPlay();
                MyGame.Movie?.Play();
            }
        }

        [RelayCommand]
        private void CutMovie()
        {
            var viewModel = new MovieCutViewModel(MovieTrimmingData);
            if (view.ShowDialog(viewModel))
            {
                MovieTrimmingData = viewModel.TrimmingData;
                if (viewModel.ApplyToIni)
                {
                    IniWriter.SetTrimmingData(viewModel.TrimmingData);
                }
                PPDStaticSetting.ApplyToIniMovieTrimming = viewModel.ApplyToIni;
            }
        }

        public void SetTrimming(MovieTrimmingData trimmingData, bool applyToIniFileWriter)
        {
            OnUIThread(() =>
            {
                MovieTrimmingData = trimmingData;
                if (applyToIniFileWriter)
                {
                    IniWriter.SetTrimmingData(MovieTrimmingData);
                }
            });
        }

        public void ChangeVolume(int vol)
        {
            if (MovieInitialized)
            {
                MyGame.Movie.Volume = vol;
            }
        }

        public void SeekMovie(double time)
        {
            if (MovieInitialized)
            {
                MyGame.Movie.Seek(time);
            }
            UpdateMovieStatus();
        }

        /// <summary>
        /// 再生や記録の間に、毎フレーム呼ばれます。時間の表示とタイムラインを進める。
        /// </summary>
        public void UpdateStatus()
        {
            UpdateMovieStatus();
            if (NativeMethods.timeGetTime() - movielastime > 50)
            {
                WindowUtility.Seekmain.Currenttime = MovieInitialized ? MoviePosition : 0;
                movielastime = NativeMethods.timeGetTime();
            }
        }

        private void UpdateMovieStatus()
        {
            if (MovieInitialized)
            {
                TimeText = GetTimeDisplay(MoviePosition);
            }
        }

        private static string GetTimeDisplay(double d)
        {
            int h = (int)d / 3600;
            int m = (int)(d - 3600 * h) / 60;
            var s = (int)(d - 3600 * h - 60 * m);
            var ms = (float)(d - 3600 * h - 60 * m - s);
            return h + ":" + (m >= 10 ? m.ToString() : "0" + m) + ":" + (s >= 10 ? s.ToString() : "0" + s) + ":" + GetMilliSecond(ms);
        }

        private static string GetMilliSecond(float ms)
        {
            var s = ms.ToString();
            if (s.Length == 1)
            {
                return "000";
            }
            if (s.Length < 5)
            {
                return s[2..].PadRight(3, '0');
            }
            return s.Substring(2, 3);
        }

        #endregion

        #region ゲームの画面とタイムライン

        public void Record(double time, int num)
        {
            WindowUtility.Seekmain.AddData(time, num);
        }

        public bool[] Updatemark(double time)
        {
            return WindowUtility.Seekmain.UpdateMark(time + PPDStaticSetting.MovieLatency);
        }

        public void ChangeMousePos(Point p)
        {
            // 起動して最初のダイアログ（作者名）を出している間は、まだゲームがない
            MyGame?.ChangeMousepos(p);
        }

        public void ChangeData(Vector2[] positions, float[] angles)
        {
            MyGame.ChangeData(positions, angles);
        }

        public void SetMarkInfo(Mark mark)
        {
            SetMarkInfo(mark.Position, mark.Rotation, mark.ID);
            Info.UpdateParameters([mark]);
        }

        public void SetMarksInfo(Mark[] marks)
        {
            Info.UpdateParameters(marks);
            if (marks.Length == 0)
            {
                Info.SetMarkInfo("", "", "");
            }
            else
            {
                SetMarkInfo(marks[0].Position, marks[0].Rotation, marks[0].ID);
            }
        }

        private static void SetMarkInfo(Vector2 vec, float rotation, uint id)
        {
            Info.SetMarkInfo(vec.ToString(), Math.Round((rotation / Math.PI * 180), 3).ToString(), id.ToString());
        }

        #endregion

        #region ゲーム（PPD）

        private bool CanStartGame()
        {
            return !IsProcessRunning && !IsCommandRunning;
        }

        /// <summary>
        /// PPD で譜面を遊びます。
        /// </summary>
        /// <param name="mode">開始する時間と、オートかどうか（"Current"、"Beginning"、"CurrentAuto"、"BeginningAuto"）。</param>
        [RelayCommand(CanExecute = nameof(CanStartGame))]
        private void StartGame(string mode)
        {
            var auto = mode.EndsWith("Auto", StringComparison.Ordinal);
            var startTime = mode.StartsWith("Current", StringComparison.Ordinal) ? WindowUtility.Seekmain.Currenttime : IniWriter.StartTime;
            if (!CheckCanExecutePpd() || !TryOverwriteProject())
            {
                return;
            }
            StartProcess(String.Format("-game PPDSingle -d \"{0}\" -auto {1} -bpm {2} -st {3} -dif {4} -connect {5} -mod \"{6}\" {7}",
                CurrentProjectDir,
                auto ? "1" : "0",
                IniWriter.BPM,
                startTime,
                CurrentDifficulty.ToString(),
                Connect ? "1" : "0",
                ModList,
                DisableExpansion ? "-disableexpansion" : ""));
        }

        /// <summary>
        /// PPD をメニューから始めます（プロジェクトのあるフォルダの曲を選べる）。
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanStartGame))]
        private void StartGameFromMenu()
        {
            if (!CheckCanExecutePpd() || !TryOverwriteProject())
            {
                return;
            }
            StartProcess(String.Format("-game PPDSingle -songdir \"{0}\"", projectFileDir));
        }

        private bool CheckCanExecutePpd()
        {
            if (!Directory.Exists(CurrentProjectDir))
            {
                view.ShowMessage("プロジェクトがありません。プロジェクトとして保存してください");
                return false;
            }
            if (!File.Exists(Program.PPDExePath))
            {
                view.ShowMessage("適切なPPD.exeの参照がありません");
                return false;
            }
            return true;
        }

        private void StartProcess(string arguments)
        {
            currentProcess?.Dispose();
            currentProcess = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo
                {
                    Arguments = arguments,
                    FileName = Program.PPDExePath,
                    WorkingDirectory = Path.GetDirectoryName(Program.PPDExePath)
                }
            };
            ProcessStarted?.Invoke();
            currentProcess.Exited += CurrentProcess_Exited;
            currentProcess.Start();
            IsProcessRunning = true;
        }

        void CurrentProcess_Exited(object sender, EventArgs e)
        {
            OnUIThread(() =>
            {
                IsProcessRunning = false;
                ProcessExited?.Invoke();
            });
        }

        [RelayCommand(CanExecute = nameof(IsProcessRunning))]
        private void KillProcess()
        {
            CheckProcess();
        }

        /// <summary>
        /// PPD を動かしていれば、終了するかを聞いて終了します。
        /// </summary>
        /// <returns>続けてよいかどうか（キャンセルしたときは false）。</returns>
        private bool CheckProcess()
        {
            if (currentProcess != null && !currentProcess.HasExited)
            {
                if (!view.Confirm(Utility.Language["AskKill"], Utility.Language["Confirm"], true))
                {
                    return false;
                }
                currentProcess.Kill();
            }
            return true;
        }

        /// <summary>
        /// コマンドを実行していれば、終了するかを聞いて終了します。
        /// </summary>
        /// <returns>続けてよいかどうか（キャンセルしたときは false）。</returns>
        private bool CheckExecutor()
        {
            if (currentExecutor != null && currentExecutor.IsWorking)
            {
                if (!view.Confirm(Utility.Language["AskKillCommand"], Utility.Language["Confirm"], true))
                {
                    return false;
                }
                currentExecutor.Kill();
                currentExecutor = null;
                CommandExecuted?.Invoke(null);
            }
            return true;
        }

        #endregion

        #region MOD

        private void LoadMods()
        {
            if (Directory.Exists("mods"))
            {
                LoadMods(Mods, null, "mods");
            }
        }

        private static void LoadMods(ObservableCollection<ModMenuItem> items, ModMenuItem parent, string dir)
        {
            foreach (string dirName in Directory.GetDirectories(dir))
            {
                var item = new ModMenuItem(Path.GetFileNameWithoutExtension(dirName), false, parent);
                items.Add(item);
                LoadMods(item.Children, item, dirName);
            }
            foreach (string fileName in Directory.GetFiles(dir, "*.mod"))
            {
                items.Add(new ModMenuItem(Path.GetFileNameWithoutExtension(fileName), true, parent));
            }
        }

        private IEnumerable<ModMenuItem> AllMods
        {
            get
            {
                var items = new Queue<ModMenuItem>(Mods);
                while (items.Count > 0)
                {
                    var item = items.Dequeue();
                    yield return item;
                    foreach (var child in item.Children)
                    {
                        items.Enqueue(child);
                    }
                }
            }
        }

        /// <summary>
        /// 使う MOD の一覧です（「フォルダ\名前」をカンマでつないだもの）。
        /// </summary>
        public string ModList
        {
            get { return String.Join(",", AllMods.Where(m => m.IsMod && m.IsChecked).Select(m => m.FullName)); }
            set
            {
                var mods = String.IsNullOrEmpty(value) ? [] : value.Split(',');
                foreach (var item in AllMods)
                {
                    item.IsChecked = item.IsMod && Array.IndexOf(mods, item.FullName) >= 0;
                }
            }
        }

        #endregion

        #region コマンド

        [RelayCommand]
        private void ReadCommands()
        {
            Commands.Clear();
            if (!Directory.Exists("Commands"))
            {
                Directory.CreateDirectory("Commands");
            }
            ReadCommands(Commands, "Commands");
        }

        private static void ReadCommands(ObservableCollection<CommandMenuItem> items, string dir)
        {
            foreach (string childDir in Directory.GetDirectories(dir))
            {
                var folder = new CommandMenuItem(Path.GetFileNameWithoutExtension(childDir), null, null);
                ReadCommands(folder.Children, childDir);
                items.Add(folder);
            }

            foreach (string childFile in Directory.GetFiles(dir))
            {
                if (!Path.GetExtension(childFile).Equals(".fsml", StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }
                var targetPath = Path.Combine(Path.GetDirectoryName(childFile), Path.GetFileNameWithoutExtension(childFile)).ToLower();
                var shortcut = WindowUtility.Seekmain.ShortcutManager.Shortcuts.FirstOrDefault(s =>
                    s.ShortcutType == ShortcutType.Custom && s.ScriptPath.Equals(targetPath, StringComparison.CurrentCultureIgnoreCase));
                items.Add(new CommandMenuItem(Path.GetFileNameWithoutExtension(childFile), childFile, shortcut?.GetCommandText()));
            }
        }

        [RelayCommand]
        private void ExecuteCommand(CommandMenuItem item)
        {
            ExecuteScript(item.Path);
        }

        /// <summary>
        /// コマンド（FlowScript）をレイヤーに対して実行します。
        /// </summary>
        public void ExecuteScript(string scriptPath)
        {
            if (!File.Exists(scriptPath))
            {
                return;
            }

            var selectedSheet = Layers.SelectedPpdSheet;
            var layers = Layers.Sheets.Select(sheet => new Layer(sheet, Layers.IDProvider.Clone(), sheet == selectedSheet)).ToArray();
            var fullPath = Path.GetFullPath(scriptPath).ToLower();
            CommandExecuting?.Invoke(fullPath);
            IsCommandRunning = true;
            currentExecutor = new Executor(fullPath, layers);
            currentExecutor.AddItem("Storage", storage);
            currentExecutor.AddItem("PosAndAngleLoader", PosAndAngleLoader.Instance);
            currentExecutor.AddItem("WindowManager", view);
            currentExecutor.AddItem("Events", Events.ChangeData.ToArray());
            currentExecutor.AddItem("Movie", MyGame.Movie);
            currentExecutor.AddItem("MovieManager", this);
            currentExecutor.AddItem("SongInfo", this);
            currentExecutor.AddItem("ParameterPresets", Info.Presets);
            currentExecutor.Finished += Executor_Finished;
            currentExecutor.Execute();
        }

        void Executor_Finished(object sender, EventArgs e)
        {
            currentExecutor = null;
            OnUIThread(() =>
            {
                var executor = (Executor)sender;
                CommandExecuted?.Invoke(executor.ScriptFilePath);
                if (executor.HasError)
                {
                    view.ShowMessage(executor.ErrorText);
                }
                foreach (var layer in executor.Layers)
                {
                    layer.Execute();
                }
                if (storage.IsChanged)
                {
                    storage.Save();
                }
                IsCommandRunning = false;
                WindowUtility.Seekmain.DrawAndRefresh();
            });
        }

        #endregion
    }
}
