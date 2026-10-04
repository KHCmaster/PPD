using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PPDConfiguration;
using PPDEditor.Models;
using PPDEditor.ViewModels.Dialogs;
using PPDEditor.ViewModels.Docks;
using PPDEditorCommon;
using PPDEditorCommon.Dialog.ViewModel;
using PPDFramework;
using PPDFramework.PPDStructure;
using PPDFramework.PPDStructure.EVDData;
using PPDFramework.PPDStructure.PPDData;
using PPDPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PPDEditor.ViewModels
{
    // プロジェクトと、譜面のファイルの読み書き
    public partial class MainWindowViewModel : ISongInfo
    {
        const string rescuedirectory = "rescueddata";

        string projectFileName = "";
        string projectFileDir = "";

        private static LayerManager Layers => WindowUtility.LayerManager;
        private static TimeLineForm TimeLine => WindowUtility.TimeLineForm;
        private static KasiEditor Kasi => WindowUtility.KasiEditor;
        private static SoundManager Sounds => WindowUtility.SoundManager;
        private static IniFileWriter IniWriter => WindowUtility.IniFileWriter;
        private static EventManager Events => WindowUtility.EventManager;
        private static GeometryCreator Geometry => WindowUtility.GeometryCreator;
        private static MemoWindow Memo => WindowUtility.MemoWindow;
        private static ResourceManager Resources => WindowUtility.ResourceManager;
        private static ScriptManager Scripts => WindowUtility.ScriptManager;
        private static PosAndAngleLoaderSaver PosAndAngle => WindowUtility.PosAndAngleLoaderSaver;
        private static InfoForm Info => WindowUtility.InfoForm;

        [ObservableProperty]
        private string windowTitle = "PPDEditor";

        /// <summary>
        /// プロジェクトを使うメニュー（上書き保存、難易度、発行）を使えるかどうか。
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OverwriteProjectCommand))]
        [NotifyCanExecuteChangedFor(nameof(PublishCommand))]
        [NotifyCanExecuteChangedFor(nameof(PublishModCommand))]
        private bool projectMenusEnabled;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ImportEventCommand))]
        private AvailableDifficulty currentDifficulty = AvailableDifficulty.Base;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ImportEventCommand))]
        private AvailableDifficulty availableDifficulty;

        /// <summary>
        /// 最近使ったプロジェクトです（新しい順に 5 つまで）。
        /// </summary>
        public ObservableCollection<FileMenuItem> RecentProjects { get; } = [];

        public string CurrentProjectDir
        {
            get { return Path.Combine(projectFileDir, CurrentProjectName); }
        }

        public string CurrentProjectFilePath
        {
            get { return Path.Combine(projectFileDir, projectFileName); }
        }

        public string CurrentProjectName
        {
            get { return Path.GetFileNameWithoutExtension(CurrentProjectFilePath); }
        }

        public bool IsProjectLoaded
        {
            get { return Directory.Exists(projectFileDir); }
        }

        public int AvailableDifficultyCount
        {
            get { return ProjectFiles.DifficultyArray.Where(d => d != AvailableDifficulty.None).Count(d => AvailableDifficulty.HasFlag(d)); }
        }

        private static bool IsNotSaved
        {
            get
            {
                return Sounds.IsContentChanged || TimeLine.IsContentChanged || IniWriter.IsContentChanged || Kasi.IsContentChanged
                    || Resources.IsContentChanged || Scripts.IsContentChanged || Events.IsContentChanged;
            }
        }

        private void UpdateProjectState()
        {
            ProjectMenusEnabled = IsProjectLoaded;
            ImportEventCommand.NotifyCanExecuteChanged();
        }

        private void ChangeWindowText()
        {
            WindowTitle = string.Format("{0}({1}) - PPDEditor", Path.GetFileNameWithoutExtension(projectFileName), CurrentDifficulty);
        }

        #region 最近使ったプロジェクト

        private void AddRecentProject(string path)
        {
            var existing = RecentProjects.FirstOrDefault(m => m.Path == path);
            if (existing != null)
            {
                RecentProjects.Move(RecentProjects.IndexOf(existing), 0);
                return;
            }
            RecentProjects.Insert(0, new FileMenuItem(path, path));
            if (RecentProjects.Count > 5)
            {
                RecentProjects.RemoveAt(RecentProjects.Count - 1);
            }
        }

        private void SaveRecentProjects()
        {
            EditorSettingsStore.SaveRecentProjects(RecentProjects.Select(p => p.Path));
        }

        [RelayCommand]
        private void OpenRecentProject(FileMenuItem item)
        {
            CheckReadProject(item.Path);
        }

        #endregion

        #region プロジェクト

        [RelayCommand]
        private void NewProject()
        {
            if (!CheckSaveStateAndSave(false))
            {
                return;
            }

            var dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            ProjectFiles.EnsureDirectory(Path.Combine(dir, "Projects"));
            var viewModel = new NewProjectWindowViewModel(Utility.Language)
            {
                FolderPath = Path.Combine(dir, "Projects")
            };
            if (!view.ShowDialog(viewModel))
            {
                return;
            }

            // restore settings
            ClearData();
            Layers.Initialize();

            projectFileDir = viewModel.FolderPath;
            projectFileName = Path.ChangeExtension(viewModel.Name, ".ppdproj");
            var template = viewModel.SelectedItem.Value;
            foreach (var p in template.EventManagerInfo.Events)
            {
                var eventData = new EventData
                {
                    BPM = p.Value.BPM
                };
                for (int i = 0; i < Math.Min(eventData.InitializeOrder.Length, p.Value.InitializeOrders.Length); i++)
                {
                    eventData.InitializeOrder[i] = (ButtonType)p.Value.InitializeOrders[i];
                }
                eventData.NoteType = p.Value.NoteType;
                Events.AddChange(p.Key, eventData);
            }
            var rowManager = TimeLine.RowManager;
            Array.Copy(template.TimeLineInfo.RowOrders, rowManager.RowOrders, Math.Min(rowManager.RowOrders.Length, template.TimeLineInfo.RowOrders.Length));
            Array.Copy(template.TimeLineInfo.RowVisibilities, rowManager.RowVisibilities, Math.Min(rowManager.RowVisibilities.Length, template.TimeLineInfo.RowVisibilities.Length));
            IsTimeLineRowLimited = template.TimeLineInfo.RowLimited;
            var sounds = new List<string>();
            foreach (var sound in template.SoundManagerInfo.Sounds.Select(s => Path.Combine(Utility.AppDir, s)))
            {
                if (!File.Exists(sound))
                {
                    view.ShowMessage(String.Format("{0}\n{1}", Utility.Language["SoundNotExist"], sound));
                    continue;
                }
                sounds.Add(sound);
            }
            if (sounds.Count > 0)
            {
                Sounds.Readsounds([.. sounds]);
            }
            foreach (var change in template.SoundManagerInfo.Changes)
            {
                for (int i = 0; i < change.Value.Length; i++)
                {
                    if (change.Value[i] > sounds.Count)
                    {
                        change.Value[i] = 0;
                    }
                }
            }
            Sounds.Readchangedata([.. template.SoundManagerInfo.Changes.Select(p => p.Key)],
                [.. template.SoundManagerInfo.Changes.Select(p => p.Value)]);

            // create folders
            ProjectFiles.EnsureDirectory(Path.Combine(CurrentProjectDir, "Base_Scripts"));
            ProjectFiles.EnsureDirectory(Path.Combine(CurrentProjectDir, "Base_layers"));
            Resources.CreateResourceFolders();
            foreach (var script in template.ScriptManagerInfo.Scripts)
            {
                var src = Path.Combine(Utility.AppDir, script.Src);
                if (!File.Exists(src))
                {
                    view.ShowMessage(String.Format("{0}\n{1}", Utility.Language["ScriptNotExist"], src));
                    continue;
                }
                var dest = Path.Combine(CurrentProjectDir, "Base_Scripts", script.Dest);
                Utility.SafeCopyFile(script.Src, dest);
            }
            Scripts.ReadScript(CurrentProjectDir, AvailableDifficulty.Base, "");

            ReadMovie(viewModel.MoviePath);
            SaveProject();
            UpdateProjectState();
            ChangeWindowText();
            AddRecentProject(CurrentProjectFilePath);
            SaveRecentProjects();
            TimeLine.InvalidateAll();
            ReadMovieFromProject();
        }

        [RelayCommand]
        private void OpenProject()
        {
            var path = view.OpenFile(Utility.Language["ProjectFilter"], "");
            if (path != null)
            {
                CheckReadProject(path);
            }
        }

        /// <summary>
        /// ウィンドウに落としたファイルがプロジェクトなら開きます。
        /// </summary>
        public void OpenDroppedFile(string path)
        {
            if (File.Exists(path) && Path.GetExtension(path) == ".ppdproj")
            {
                CheckReadProject(path);
            }
        }

        private void CheckReadProject(string path)
        {
            if (!CheckSaveStateAndSave(false))
            {
                return;
            }

            if (CheckProjectVersion(path))
            {
                ReadProject(path);
            }
        }

        private bool CheckProjectVersion(string path)
        {
            var version = ProjectFiles.GetVersion(path);
            if (version == ProjectFiles.LatestVersion)
            {
                return true;
            }
            if (!view.Confirm(Utility.Language["OldProjectDetected"], Utility.Language["Confirm"]))
            {
                return false;
            }
            ProjectFiles.BackUp(path);
            ProjectFiles.Convert(path, version);
            return true;
        }

        /// <summary>
        /// 保存していない変更があれば、保存するかを聞いて保存します。
        /// </summary>
        /// <param name="okCancel">OK とキャンセルで聞くか（false のときは、はい、いいえ、キャンセル）。</param>
        /// <returns>続けてよいかどうか（キャンセルしたときは false）。</returns>
        private bool CheckSaveStateAndSave(bool okCancel)
        {
            if (!IsNotSaved)
            {
                return true;
            }
            var answer = view.AskSave(Utility.Language["AskSave"], Utility.Language["Confirm"], okCancel);
            if (answer == null)
            {
                return false;
            }
            if (answer == true)
            {
                if (String.IsNullOrEmpty(projectFileDir))
                {
                    return SaveProjectAs();
                }
                SaveProject();
            }
            return true;
        }

        /// <summary>
        /// 保存する場所を選んで保存します。
        /// </summary>
        /// <returns>保存したかどうか（やめたときは false）。</returns>
        private bool SaveProjectAs()
        {
            var path = view.SaveFile(Utility.Language["ProjectFilter"], projectFileName != "" ? projectFileName : "New.ppdproj",
                projectFileDir != "" ? projectFileDir : null);
            if (path == null)
            {
                return false;
            }
            path = WithExtension(path, ".ppdproj");
            projectFileName = Path.GetFileName(path);
            projectFileDir = Path.GetDirectoryName(path);
            SaveProject();
            UpdateProjectState();
            ChangeWindowText();
            return true;
        }

        [RelayCommand(CanExecute = nameof(ProjectMenusEnabled))]
        private void OverwriteProject()
        {
            if (TryOverwriteProject())
            {
                view.ShowSaveDone();
            }
        }

        private bool TryOverwriteProject()
        {
            if (Directory.Exists(projectFileDir) && File.Exists(CurrentProjectFilePath))
            {
                SaveProject();
                return true;
            }
            return false;
        }

        private void SaveProject()
        {
            SaveProject(CurrentDifficulty, true);
        }

        private void SaveProject(AvailableDifficulty difficulty, bool changeDifficulty)
        {
            var projectdir = CurrentProjectDir;
            string projectData = "";
            if (File.Exists(CurrentProjectFilePath))
            {
                projectData = File.ReadAllText(CurrentProjectFilePath);
            }
            var setting = new SettingReader(projectData);
            var head = difficulty.ToString();
            string headdot = head + ".";
            LayerViewModel[] lds = Layers.AllLayerDisplay;
            var layersDir = Path.Combine(projectdir, head + "_" + "layers");
            if (Directory.Exists(layersDir))
            {
                Directory.Delete(layersDir, true);
            }
            Directory.CreateDirectory(layersDir);
            for (int i = 0; i < lds.Length; i++)
            {
                setting.ReplaceOrAdd(headdot + "layer" + i, lds[i].PPDData.DisplayName);
                setting.ReplaceOrAdd(headdot + "layer" + i + "BPM", lds[i].PPDData.BPM);
                setting.ReplaceOrAdd(headdot + "layer" + i + "BPMOffset", lds[i].PPDData.BPMStart);
                setting.ReplaceOrAdd(headdot + "layer" + i + "DisplayWidth", lds[i].PPDData.DisplayWidth);
                setting.ReplaceOrAdd(headdot + "layer" + i + "Visible", lds[i].DisplayVisible);
                setting.ReplaceOrAdd(headdot + "layer" + i + "DisplayMode", (int)lds[i].PPDData.DisplayMode);
                setting.ReplaceOrAdd(headdot + "layer" + i + "BeatType", (int)lds[i].PPDData.BeatType);
                setting.ReplaceOrAdd(headdot + "layer" + i + "MarkColor", lds[i].MarkColor.ToArgb());
                var writepath = Path.Combine(layersDir, "layer" + i + ".ppd");
                ProjectFiles.DeleteIfExists(writepath);
                SavePpdFile(writepath, lds[i].PPDData.GetSortedDataAsMarkData());
            }
            Layers.ContentSaved();

            setting.ReplaceOrAdd(headdot + "selectedlayerindex", Layers.SelectedLayerIndex);
            setting.ReplaceOrAdd(headdot + "bpm", WindowUtility.Seekmain.BPM);
            setting.ReplaceOrAdd(headdot + "bpmstart", WindowUtility.Seekmain.BPMSTART);
            setting.ReplaceOrAdd(headdot + "lastseektime", MovieInitialized ? MoviePosition : 0);
            setting.ReplaceOrAdd(headdot + "displaymode", (int)DisplayMode);
            setting.ReplaceOrAdd(headdot + "beattype", (int)BeatType);
            setting.ReplaceOrAdd(headdot + "bpmfixed", Bpmfixed);
            setting.ReplaceOrAdd(headdot + "drawtoggle", DrawToggle);
            setting.ReplaceOrAdd(headdot + "showholdextent", ShowHoldExtent);
            setting.ReplaceOrAdd(headdot + "markfocus", MarkFocus);
            setting.ReplaceOrAdd(headdot + "pandadisplayangle", PosAndAngle.DisplayAngle);
            setting.ReplaceOrAdd(headdot + "displaywidth", DisplayWidth);
            setting.ReplaceOrAdd(headdot + "speedscale", SpeedScale);
            setting.ReplaceOrAdd(headdot + "fixeddistance", Geometry.FixedDistance);
            setting.ReplaceOrAdd(headdot + "antialias", Geometry.Antialias);
            setting.ReplaceOrAdd(headdot + "drawpos", Geometry.DrawPos);
            setting.ReplaceOrAdd(headdot + "drawangle", Geometry.DrawAngle);
            setting.ReplaceOrAdd(headdot + "applymoved", Geometry.ApplyMoved);
            setting.ReplaceOrAdd(headdot + "gridwidth", Grid.Width);
            setting.ReplaceOrAdd(headdot + "gridheight", Grid.Height);
            setting.ReplaceOrAdd(headdot + "gridoffsetx", Grid.OffsetX);
            setting.ReplaceOrAdd(headdot + "gridoffsety", Grid.OffsetY);
            setting.ReplaceOrAdd(headdot + "gridcolor", System.Drawing.ColorTranslator.ToHtml(Grid.GridColor));
            setting.ReplaceOrAdd(headdot + "displaygrid", DisplayGrid);
            setting.ReplaceOrAdd(headdot + "gcgridwidth", Geometry.SquareGrid.Width);
            setting.ReplaceOrAdd(headdot + "gcgridheight", Geometry.SquareGrid.Height);
            setting.ReplaceOrAdd(headdot + "gcgridoffsetx", Geometry.SquareGrid.OffsetX);
            setting.ReplaceOrAdd(headdot + "gcgridoffsety", Geometry.SquareGrid.OffsetY);
            setting.ReplaceOrAdd(headdot + "gcgridcolor", System.Drawing.ColorTranslator.ToHtml(Geometry.SquareGrid.GridColor));
            setting.ReplaceOrAdd(headdot + "gcdisplaygrid", Geometry.SquareGridEnabled);
            setting.ReplaceOrAdd(headdot + "farness", Farness);
            setting.ReplaceOrAdd(headdot + "anglerestriction", Geometry.AngleRestriction);
            setting.ReplaceOrAdd(headdot + "disabledscript", Scripts.GetDisableScriptList());
            setting.ReplaceOrAdd(headdot + "connect", Connect);
            setting.ReplaceOrAdd(headdot + "modlist", ModList);
            setting.ReplaceOrAdd(headdot + "disableexpansion", DisableExpansion);

            setting.ReplaceOrAdd("memo", Memo.MemoText.Replace("[", "\\[").Replace("]", "\\]"));
            Memo.ContentSaved();
            setting.ReplaceOrAdd("version", ProjectFiles.LatestVersion);
            if (changeDifficulty)
            {
                setting.ReplaceOrAdd("selecteddifficulty", CurrentDifficulty.ToString());
            }
            setting.ReplaceOrAdd("disabledresources", Resources.GetDisableList());
            setting.ReplaceOrAdd("timelineroworders", String.Join(",", TimeLine.RowManager.RowOrders));
            setting.ReplaceOrAdd("timelinerowvisibilities", String.Join(",", TimeLine.RowManager.RowVisibilities.Select(r => r ? 1 : 0)));
            setting.ReplaceOrAdd("timelinerowlimited", IsTimeLineRowLimited);

            var sw = new SettingWriter(CurrentProjectFilePath, false);
            foreach (KeyValuePair<string, string> kvp in setting.Dictionary)
            {
                sw.Write(kvp.Key, kvp.Value);
            }
            sw.Close();

            ProjectFiles.EnsureDirectory(projectdir);
            ProjectFiles.DeleteIfExists(Path.Combine(projectdir, "kasi.txt"));
            SaveKasi(Path.Combine(projectdir, "kasi.txt"));
            ProjectFiles.DeleteIfExists(Path.Combine(projectdir, "data.ini"));
            SaveIni(Path.Combine(projectdir, "data.ini"));
            ProjectFiles.DeleteIfExists(Path.Combine(projectdir, "soundset.txt"));
            SaveSoundSet(Path.Combine(projectdir, "soundset.txt"));
            ProjectFiles.DeleteIfExists(Path.Combine(projectdir, headdot + "scd"));
            SaveScd(Path.Combine(projectdir, headdot + "scd"));

            ProjectFiles.DeleteIfExists(Path.Combine(projectdir, headdot + "ppd"));
            Scripts.SaveAll();
            var pw = new PackWriter(Path.Combine(projectdir, headdot + "ppd"));
            var packFileList = new List<string>(Scripts.GetEnabledScriptList());
            packFileList.AddRange(["ppd", "evd"]);
            foreach (PPDPackStreamWriter ppdpsw in pw.Write([.. packFileList]))
            {
                switch (ppdpsw.Filename)
                {
                    case "ppd":
                        SavePpd(ppdpsw, Layers.GetAllData(false));
                        break;
                    case "evd":
                        SaveEvd(ppdpsw);
                        break;
                    default:
                        Scripts.WriteScript(ppdpsw, ppdpsw.Filename);
                        break;
                }
            }
            pw.Close();
            Scripts.ContentSaved();

            pw = new PackWriter(Path.Combine(projectdir, "resource.pak"));
            foreach (PPDPackStreamWriter ppdpsw in pw.Write(Resources.GetEnabledList()))
            {
                Resources.WriteResource(ppdpsw, ppdpsw.Filename);
            }
            pw.Close();
            Resources.ContentSaved();

            CopySoundsToProject(projectdir);
            if (!Directory.GetFiles(projectdir).Any(f => Path.GetFileNameWithoutExtension(f) == "movie"))
            {
                File.Copy(MovieFileName, Path.Combine(projectdir, "movie" + Path.GetExtension(MovieFileName)), true);
            }
        }

        /// <summary>
        /// 使っている音をプロジェクトの sound のフォルダにコピーし、使っていない音を消します。
        /// </summary>
        private void CopySoundsToProject(string projectdir)
        {
            var soundDir = Path.Combine(projectdir, "sound");
            ProjectFiles.EnsureDirectory(soundDir);
            List<string> datas = Sounds.Sounddatapaths;
            for (int i = 1; i < datas.Count; i++)
            {
                if (!ExDirectory.Contains(soundDir, Path.GetFileName(datas[i])))
                {
                    if (File.Exists(datas[i]))
                    {
                        File.Copy(datas[i], Path.Combine(soundDir, Path.GetFileName(datas[i])), true);
                    }
                    else
                    {
                        view.ShowMessage(datas[i] + Utility.Language["NoSoundFile"]);
                    }
                }
            }
            foreach (var filename in Directory.GetFiles(soundDir))
            {
                if (!datas.Skip(1).Any(data => Path.GetFileName(filename) == Path.GetFileName(data)))
                {
                    File.Delete(filename);
                }
            }
        }

        private void ClearData()
        {
            IsTimeLineRowLimited = false;
            TimeLine.RowManager.Restore();
            WindowUtility.Seekmain.DeleteData(-1);
            Kasi.Clear();
            Sounds.Clear();
            Layers.Clear();
            Events.Clear();
            IniWriter.Clear();
            Resources.Clear();
            Scripts.Clear();
            Memo.Clear();

            BPM = 100;
            BPMOffset = 0;
            DisplayMode = 0;
            Bpmfixed = true;
            ShowHoldExtent = true;
            SpeedScale = 2;
            DisplayWidth = 240;
            CurrentDifficulty = AvailableDifficulty.Base;
            AvailableDifficulty = AvailableDifficulty.Base;
        }

        private void ReadMovieFromProject()
        {
            var file = Directory.GetFiles(CurrentProjectDir).FirstOrDefault(f => Path.GetFileNameWithoutExtension(f) == "movie");
            if (!String.IsNullOrEmpty(file))
            {
                ReadMovie(file);
            }
        }

        private void ReadProject(string path)
        {
            ClearData();
            projectFileName = Path.GetFileName(path);
            projectFileDir = Path.GetDirectoryName(path);
            ReadMovieFromProject();
            if (File.Exists(Path.Combine(CurrentProjectDir, "kasi.txt")))
            {
                ReadKasi(Path.Combine(CurrentProjectDir, "kasi.txt"));
            }
            if (File.Exists(Path.Combine(CurrentProjectDir, "data.ini")))
            {
                ReadIni(Path.Combine(CurrentProjectDir, "data.ini"));
            }
            if (File.Exists(Path.Combine(CurrentProjectDir, "soundset.txt")))
            {
                ReadSoundSet(Path.Combine(CurrentProjectDir, "soundset.txt"));
            }
            var setting = new SettingReader(File.ReadAllText(path));
            var difficulty = ProjectFiles.ParseDifficulty(setting.ReadString("selecteddifficulty"));
            CurrentDifficulty = difficulty;
            AvailableDifficulty = ProjectFiles.GetAvailableDifficulty(CurrentProjectDir);
            Memo.MemoText = setting.ReadString("memo").Replace("\n", System.Environment.NewLine);
            Memo.ContentSaved();
            Resources.ReadResource(CurrentProjectDir, setting.ReadString("disabledresources"));
            var temp = setting.ReadString("timelineroworders");
            if (!String.IsNullOrEmpty(temp))
            {
                var array = temp.Split(',').Select(r => int.Parse(r)).ToArray();
                if (array.Length == 10 && array.All(r => r >= 0 && r < 10) && array.Distinct().Count() == 10)
                {
                    TimeLine.RowManager.RowOrders = array;
                }
            }
            temp = setting.ReadString("timelinerowvisibilities");
            if (!String.IsNullOrEmpty(temp))
            {
                var array = temp.Split(',').Select(r => int.Parse(r) == 1).ToArray();
                if (array.Length == 10)
                {
                    TimeLine.RowManager.RowVisibilities = array;
                }
            }
            IsTimeLineRowLimited = setting.ReadBoolean("timelinerowlimited");
            ReadSpecificDifficulty(setting, difficulty, CurrentProjectDir);
            Resources.ContentSaved();

            AddRecentProject(path);
            SaveRecentProjects();
            UpdateProjectState();
            ChangeWindowText();
        }

        private void ReadSpecificDifficulty()
        {
            var setting = new SettingReader(File.ReadAllText(CurrentProjectFilePath));
            ReadSpecificDifficulty(setting, CurrentDifficulty, CurrentProjectDir);
        }

        private void ReadSpecificDifficulty(SettingReader setting, AvailableDifficulty difficulty, string projectdir)
        {
            var head = difficulty.ToString();
            string headdot = head + ".";

            Sounds.ClearSetting();
            if (File.Exists(Path.Combine(projectdir, headdot + "scd")))
            {
                ReadScd(Path.Combine(projectdir, headdot + "scd"));
            }

            if (File.Exists(Path.Combine(projectdir, headdot + "ppd")))
            {
                var pr = new PackReader(Path.Combine(projectdir, headdot + "ppd"));
                using (Stream stream = pr.Read("evd"))
                {
                    ReadEvd(stream);
                }
                pr.Close();
            }

            Scripts.Clear();
            Scripts.ReadScript(projectdir, difficulty, setting.ReadString(headdot + "disabledscript"));
            Scripts.ContentSaved();

            // layer data
            int iter = 0;
            Layers.Clear();
            string headlayer = headdot + "layer";
            int iTemp;
            DisplayLineMode displayMode;
            DisplayBeatType beatType;
            while (true)
            {
                var layername = setting.ReadString(headlayer + iter);
                var ppdfilePath = Path.Combine(Path.Combine(projectdir, head + "_" + "layers"), "layer" + iter + ".ppd");
                if (layername == "" || !File.Exists(ppdfilePath)) break;
                float.TryParse(setting.ReadString(headlayer + iter + "BPM"), NumberStyles.Float, CultureInfo.InvariantCulture, out float layerbpm);
                float.TryParse(setting.ReadString(headlayer + iter + "BPMOffset"), NumberStyles.Float, CultureInfo.InvariantCulture, out float layerbpmoffset);
                int.TryParse(setting.ReadString(headlayer + iter + "DisplayWidth"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int layerdisplaywidth);
                bool visible = setting.ReadString(headlayer + iter + "Visible") == "1";
                int.TryParse(setting.ReadString(headlayer + iter + "DisplayMode"), NumberStyles.Integer, CultureInfo.InvariantCulture, out iTemp);
                displayMode = (DisplayLineMode)iTemp;
                if (!int.TryParse(setting.ReadString(headlayer + iter + "BeatType"), NumberStyles.Integer, CultureInfo.InvariantCulture, out iTemp))
                {
                    iTemp = (int)DisplayBeatType.Fourth;
                }
                beatType = (DisplayBeatType)iTemp;
                Layers.AddLayer(layername, layerbpm, layerbpmoffset, layerdisplaywidth, visible, displayMode, beatType);
                Layers.SelectLastLayer();
                ReadPpd(ppdfilePath);
                if (int.TryParse(setting.ReadString(headlayer + iter + "MarkColor"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int markColor))
                {
                    Layers.AllLayerDisplay[Layers.SelectedLayerIndex].MarkColor = System.Drawing.Color.FromArgb(markColor);
                }
                iter++;
            }

            if (!int.TryParse(setting.ReadString(headdot + "selectedlayerindex"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int selectedlayerindex))
            {
                selectedlayerindex = -1;
            }
            Layers.SelectLayerByIndex(selectedlayerindex);
            Layers.CalculateMaxID();

            if (!float.TryParse(setting.ReadString(headdot + "bpm"), NumberStyles.Float, CultureInfo.InvariantCulture, out float bpm))
            {
                bpm = 100;
            }
            if (!float.TryParse(setting.ReadString(headdot + "bpmstart"), NumberStyles.Float, CultureInfo.InvariantCulture, out float bpmstart))
            {
                bpmstart = 0;
            }
            if (!int.TryParse(setting.ReadString(headdot + "displaymode"), NumberStyles.Integer, CultureInfo.InvariantCulture, out iTemp))
            {
                iTemp = (int)DisplayLineMode.Fourth;
            }
            displayMode = (DisplayLineMode)iTemp;
            if (!int.TryParse(setting.ReadString(headdot + "beattype"), NumberStyles.Integer, CultureInfo.InvariantCulture, out iTemp))
            {
                iTemp = (int)DisplayBeatType.Fourth;
            }
            beatType = (DisplayBeatType)iTemp;
            bool bpmfixed = setting.ReadString(headdot + "bpmfixed") == "1";
            bool drawtoggle = setting.ReadString(headdot + "drawtoggle") == "1";
            bool markfocus = setting.ReadString(headdot + "markfocus") == "1";
            bool connect = setting.ReadString(headdot + "connect") == "1";
            bool showHoldExtent = setting.ReadString(headdot + "showholdextent") == "1";
            bool disableExpansion = setting.ReadString(headdot + "disableexpansion") == "1";
            var modList = setting.ReadString(headdot + "modlist");
            if (!int.TryParse(setting.ReadString(headdot + "speedscale"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int speedscale))
            {
                speedscale = 0;
            }
            var dw = setting.ReadString(headdot + "displaywidth");
            int displaywidth = IsNumber(dw) ? int.Parse(dw, CultureInfo.InvariantCulture) : 240;
            if (setting.ReadString(headdot + "layer0") == "")
            {
                BPM = bpm;
                BPMOffset = bpmstart;
                DisplayWidth = displaywidth;
            }
            Bpmfixed = bpmfixed;
            SpeedScale = speedscale;
            DisplayMode = displayMode;
            BeatType = beatType;
            ShowHoldExtent = showHoldExtent;
            DrawToggle = drawtoggle;
            MarkFocus = markfocus;
            Connect = connect;
            ModList = modList;
            DisableExpansion = disableExpansion;
            double.TryParse(setting.ReadString(headdot + "lastseektime"), NumberStyles.Float, CultureInfo.InvariantCulture, out double seektime);
            if (MovieInitialized)
            {
                SeekMovie(seektime);
                WindowUtility.Seekmain.Currenttime = seektime;
            }
            int.TryParse(setting.ReadString(headdot + "fixeddistance"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int fixeddistance);
            Geometry.FixedDistance = fixeddistance;
            Geometry.Antialias = setting.ReadString(headdot + "antialias") == "1";
            Geometry.DrawPos = setting.ReadString(headdot + "drawpos") == "1";
            Geometry.DrawAngle = setting.ReadString(headdot + "drawangle") == "1";
            Geometry.ApplyMoved = setting.ReadString(headdot + "applymoved") == "1";
            if (!int.TryParse(setting.ReadString(headdot + "gridwidth"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int gridwidth)) gridwidth = 5;
            if (!int.TryParse(setting.ReadString(headdot + "gridheight"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int gridheight)) gridheight = 5;
            if (!int.TryParse(setting.ReadString(headdot + "gridoffsetx"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int gridoffsetx)) gridoffsetx = 0;
            if (!int.TryParse(setting.ReadString(headdot + "gridoffsety"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int gridoffsety)) gridoffsety = 0;
            if (!float.TryParse(setting.ReadString(headdot + "farness"), NumberStyles.Integer, CultureInfo.InvariantCulture, out float farness)) farness = 0.5f;
            Farness = farness;
            try
            {
                Grid.GridColor = System.Drawing.ColorTranslator.FromHtml(setting.ReadString(headdot + "gridcolor"));
            }
            catch
            {
            }
            Grid.Width = gridwidth;
            Grid.Height = gridheight;
            Grid.OffsetX = gridoffsetx;
            Grid.OffsetY = gridoffsety;
            DisplayGrid = setting.ReadString(headdot + "displaygrid") == "1";

            if (!int.TryParse(setting.ReadString(headdot + "gcgridwidth"), NumberStyles.Integer, CultureInfo.InvariantCulture, out gridwidth)) gridwidth = 25;
            if (!int.TryParse(setting.ReadString(headdot + "gcgridheight"), NumberStyles.Integer, CultureInfo.InvariantCulture, out gridheight)) gridheight = 25;
            if (!int.TryParse(setting.ReadString(headdot + "gcgridoffsetx"), NumberStyles.Integer, CultureInfo.InvariantCulture, out gridoffsetx)) gridoffsetx = 0;
            if (!int.TryParse(setting.ReadString(headdot + "gcgridoffsety"), NumberStyles.Integer, CultureInfo.InvariantCulture, out gridoffsety)) gridoffsety = 0;
            try
            {
                Geometry.SquareGrid.GridColor = System.Drawing.ColorTranslator.FromHtml(setting.ReadString(headdot + "gcgridcolor"));
            }
            catch
            {
            }
            Geometry.SquareGrid.Width = gridwidth;
            Geometry.SquareGrid.Height = gridheight;
            Geometry.SquareGrid.OffsetX = gridoffsetx;
            Geometry.SquareGrid.OffsetY = gridoffsety;
            Geometry.SquareGridEnabled = setting.ReadString(headdot + "gcdisplaygrid") == "1";

            int.TryParse(setting.ReadString(headdot + "anglerestriction"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int anglerestriction);
            Geometry.AngleRestriction = anglerestriction;
        }

        private static bool IsNumber(string s)
        {
            return !string.IsNullOrEmpty(s) && s.All(char.IsNumber);
        }

        [RelayCommand]
        private void MergeProject()
        {
            var viewModel = new MergeViewModel();
            if (!view.ShowDialog(viewModel))
            {
                return;
            }
            if (viewModel.MergeDifficulty == AvailableDifficulty.None)
            {
                view.ShowMessage(Utility.Language["NoMergeDifficulty"]);
                return;
            }
            ProjectFiles.Merge(viewModel.MergeDifficulty, viewModel.GetMergeInfo, viewModel.BaseInfoProjectDifficulty, viewModel.DestProjectFilePath);
        }

        [RelayCommand(CanExecute = nameof(ProjectMenusEnabled))]
        private void Publish()
        {
            if (!CheckSaveStateAndSave(true))
            {
                return;
            }
            var viewModel = new PublishViewModel(AvailableDifficulty, CurrentProjectName, CurrentProjectDir);
            if (!view.ShowDialog(viewModel))
            {
                return;
            }
            if (viewModel.PublishDifficulty == AvailableDifficulty.None)
            {
                view.ShowMessage(Utility.Language["NoSelectedDifficulty"]);
                return;
            }
            ProjectFiles.EnsureDirectory(viewModel.PublishFolderPath);
            var destdir = Path.Combine(viewModel.PublishFolderPath, viewModel.PublishFolderName);
            ProjectFiles.Publish(CurrentProjectDir, destdir, viewModel.PublishDifficulty);
            view.ShowInExplorer(destdir);
        }

        [RelayCommand(CanExecute = nameof(ProjectMenusEnabled))]
        private void PublishMod()
        {
            if (Scripts.GetEnabledScriptList().Length < 1)
            {
                view.ShowMessage("MODの発行には少なくともひとつのスクリプトが必要です。");
                return;
            }

            var viewModel = new ModPublishViewModel();
            if (!view.ShowDialog(viewModel))
            {
                return;
            }
            var path = Path.ChangeExtension(Path.Combine(viewModel.PublishFolder, viewModel.ModFileName), "mod");
            ModPackage.Write(path, viewModel.ModDisplayName, viewModel.ModAuthorName, viewModel.ModVersion,
                Scripts.GetEnabledScriptList(), Resources.GetEnabledList(), Scripts.WriteScript, Resources.WriteResource);
            view.ShowInExplorer(path);
        }

        /// <summary>
        /// データを退避します（エラーで止まったとき）。
        /// </summary>
        public void RescueData()
        {
            ProjectFiles.EnsureDirectory(rescuedirectory);
            SavePpdFile(rescuedirectory + "\\base.ppd", Layers.GetAllData(true));
            SaveScd(rescuedirectory + "\\base.scd");
            SaveSoundSet(rescuedirectory + "\\soundset.txt");
            SaveKasi(rescuedirectory + "\\kasi.txt");
            SaveIni(rescuedirectory + "\\data.ini");
            SaveEvd(rescuedirectory + "\\base.evd");
            SaveLayers(rescuedirectory);
            view.ShowMessage("Data was rescued at\n" + Path.GetFullPath(rescuedirectory));
        }

        [RelayCommand]
        private void Exit()
        {
            if (!CheckProcess() || !CheckExecutor())
            {
                return;
            }
            if (IsNotSaved)
            {
                var answer = view.AskSave(Utility.Language["AskSave"], Utility.Language["Confirm"], false);
                if (answer == null || answer == true && !SaveProjectAs())
                {
                    return;
                }
            }
            var placement = view.Placement;
            EditorSettingsStore.SaveConfig(placement.Bounds, placement.Maximized);
            SaveRecentProjects();
            view.Close();
        }

        #endregion

        #region 難易度

        [RelayCommand]
        private void ChangeDifficulty(AvailableDifficulty difficulty)
        {
            if (CurrentDifficulty != difficulty)
            {
                if (!CheckSaveStateAndSave(true))
                {
                    return;
                }
                CurrentDifficulty = difficulty;
                ReadSpecificDifficulty();
            }
            ChangeWindowText();
        }

        [RelayCommand]
        private void CopyDifficulty()
        {
            if (!CheckSaveStateAndSave(true))
            {
                return;
            }
            var viewModel = new CopyDifficultyViewModel(CurrentDifficulty);
            if (!view.ShowDialog(viewModel))
            {
                return;
            }
            var dest = viewModel.DestDifficulty;
            if (AvailableDifficulty.HasFlag(dest) && !view.Confirm(Utility.Language["AlreadyExistDifficulty"], Utility.Language["Confirm"]))
            {
                return;
            }
            AvailableDifficulty |= dest;
            SaveProject(dest, false);

            string srcFolder = Path.Combine(CurrentProjectDir, CurrentDifficulty + "_" + "Scripts"),
                destFolder = Path.Combine(CurrentProjectDir, dest + "_" + "Scripts");
            if (Directory.Exists(srcFolder))
            {
                Utility.CopyDirectory(srcFolder, destFolder, []);
            }
        }

        #endregion

        #region イベントの読み込み

        /// <summary>
        /// ほかの難易度のイベントを読み込みます。
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanImportEvent))]
        private void ImportEvent(AvailableDifficulty difficulty)
        {
            ImportEventFrom(Path.Combine(CurrentProjectDir, difficulty.ToString().ToLower() + ".ppd"));
        }

        private bool CanImportEvent(AvailableDifficulty difficulty)
        {
            return IsProjectLoaded && AvailableDifficulty.HasFlag(difficulty) && CurrentDifficulty != difficulty;
        }

        [RelayCommand]
        private void ImportEventFile()
        {
            var path = view.OpenFile(Utility.Language["EVDFilter"], "");
            if (path != null)
            {
                ImportEventFrom(path);
            }
        }

        private void ImportEventFrom(string filePath)
        {
            if (!File.Exists(filePath))
            {
                view.ShowMessage(String.Format("Not Exist File:{0}", filePath));
                return;
            }
            var data = ChartFileReader.ReadEventsFromFile(filePath, IniWriter.BPM);
            var viewModel = new ImportEventViewModel(Events.ChangeData, data);
            if (view.ShowDialog(viewModel))
            {
                Events.ChangeData = viewModel.ChangeData;
            }
        }

        #endregion

        #region ファイルを開く

        [RelayCommand]
        private void OpenPpd()
        {
            var path = view.OpenFile(Utility.Language["PPDFilter"], "");
            if (path == null)
            {
                return;
            }
            try
            {
                using PackReader reader = new(path);
                ReadPpd(reader.Read("ppd"));
            }
            catch
            {
                ReadPpd(path);
            }
        }

        [RelayCommand]
        private void OpenMovie()
        {
            var path = view.OpenFile(Utility.Language["MovieFilter"], "");
            if (path != null)
            {
                ReadMovie(path);
            }
        }

        [RelayCommand]
        private void OpenKasi()
        {
            var path = view.OpenFile(Utility.Language["SoundSetFilter"], "kasi.txt");
            if (path != null)
            {
                ReadKasi(path);
            }
        }

        [RelayCommand]
        private void OpenDataIni()
        {
            var path = view.OpenFile(Utility.Language["INIFilter"], "data.ini");
            if (path != null)
            {
                ReadIni(path);
            }
        }

        [RelayCommand]
        private void OpenEvd()
        {
            var path = view.OpenFile(Utility.Language["EVDFilter"], "");
            if (path == null)
            {
                return;
            }
            var data = ChartFileReader.ReadEventsFromFile(path, IniWriter.BPM);
            Events.Clear();
            foreach (KeyValuePair<float, EventData> kvp in data)
            {
                Events.AddChange(kvp.Key, kvp.Value);
            }
            Events.ContentSaved();
        }

        private static void ReadKasi(string filename)
        {
            Kasi.Setdata(File.ReadAllText(filename));
            Kasi.ContentSaved();
        }

        private void ReadPpd(string filename)
        {
            using FileStream fs = File.Open(filename, FileMode.Open);
            ReadPpd(fs);
        }

        private void ReadPpd(Stream stream)
        {
            bool signError = false;
            var markData = PPDReader.Read(stream, ref signError);
            if (signError)
            {
                view.ShowMessage(Utility.Language["InvalidData"]);
                return;
            }

            var seekmain = WindowUtility.Seekmain;
            foreach (MarkDataBase data in markData)
            {
                if (data is ExMarkData exData)
                {
                    seekmain.ExAddData(data.Time, exData.EndTime, data.X, data.Y, data.Angle, (int)data.ButtonType, data.ID, data.Parameters, false);
                }
                else if (data is MarkData)
                {
                    seekmain.AddData(data.Time, data.X, data.Y, data.Angle, (int)data.ButtonType, data.ID, data.Parameters, false);
                }
            }
            seekmain.ClearHistory();
            seekmain.DrawAndRefresh();
            TimeLine.ContentSaved();
        }

        private void ReadIni(string filename)
        {
            var content = ChartFileReader.ReadDataIni(filename);
            MovieTrimmingData = content.Trimming;
            IniWriter.SetIni(content.Times, content.Difficulties, content.Bpm, content.BpmString, MovieTrimmingData);
            if (content.Guid != "")
            {
                IniWriter.SongGuid = new Guid(content.Guid);
            }
            IniWriter.ContentSaved();
        }

        private static void ReadSoundSet(string filename)
        {
            var pathes = ChartFileReader.ReadSoundSet(filename);
            if (pathes.Length == 0)
            {
                return;
            }
            Sounds.Readsounds(pathes);
            Sounds.ContentSaved();
        }

        private static void ReadScd(string filename)
        {
            var changes = ChartFileReader.ReadSoundChanges(filename);
            if (changes == null)
            {
                return;
            }
            Sounds.Readchangedata(changes.Value.Times, changes.Value.Changes);
            Sounds.ContentSaved();
        }

        private static void ReadEvd(Stream stream)
        {
            Events.Clear();
            foreach (KeyValuePair<float, EventData> kvp in ChartFileReader.ReadEvents(stream, IniWriter.BPM))
            {
                Events.AddChange(kvp.Key, kvp.Value);
            }
            Events.ContentSaved();
        }

        #endregion

        #region ファイルに保存

        /// <summary>
        /// 保存するファイルを選ばせます。拡張子が違うときは直し、すでにあるファイルは消しておく。
        /// </summary>
        /// <returns>キャンセルしたときは null。</returns>
        private string AskSavePath(string filterKey, string defaultName)
        {
            var path = view.SaveFile(Utility.Language[filterKey], defaultName);
            if (path == null)
            {
                return null;
            }
            path = WithExtension(path, Path.GetExtension(defaultName));
            ProjectFiles.DeleteIfExists(path);
            return path;
        }

        private static string WithExtension(string path, string extension)
        {
            return path.EndsWith(extension, StringComparison.Ordinal) ? path : Path.ChangeExtension(path, extension);
        }

        [RelayCommand]
        private void SavePpdAs()
        {
            var path = AskSavePath("PPDFilter", "NewFile.ppd");
            if (path != null)
            {
                SavePpdFile(path, WindowUtility.Seekmain.GetSorteData());
            }
        }

        [RelayCommand]
        private void SaveKasiAs()
        {
            var path = AskSavePath("SoundSetFilter", "kasi.txt");
            if (path != null)
            {
                SaveKasi(path);
            }
        }

        [RelayCommand]
        private void SaveSoundSetAs()
        {
            var path = AskSavePath("SoundSetFilter", "soundset.txt");
            if (path != null)
            {
                SaveSoundSet(path);
            }
        }

        [RelayCommand]
        private void SaveScdAs()
        {
            var path = AskSavePath("SCDFilter", "new.scd");
            if (path != null)
            {
                SaveScd(path);
            }
        }

        [RelayCommand]
        private void SaveDataIniAs()
        {
            var path = AskSavePath("INIFilter", "data.ini");
            if (path != null)
            {
                SaveIni(path);
            }
        }

        [RelayCommand]
        private void SaveEvdAs()
        {
            var path = AskSavePath("EVDFilter", "NewFile.evd");
            if (path != null)
            {
                SaveEvd(path);
            }
        }

        private static void SaveSoundSet(string filename)
        {
            File.WriteAllText(filename, Sounds.Savesoundset());
            Sounds.ContentSaved();
        }

        private static void SaveScd(string filename)
        {
            using (FileStream fs = File.Create(filename))
            {
                SCDWriter.Write(fs, Sounds.GetSCDData());
            }
            Sounds.ContentSaved();
        }

        private static void SavePpd(Stream stream, MarkData[] sorted)
        {
            PPDWriter.Write(stream, sorted);
            TimeLine.ContentSaved();
        }

        private static void SavePpdFile(string filename, MarkData[] sorted)
        {
            using FileStream fs = File.Create(filename);
            SavePpd(fs, sorted);
        }

        private static void SaveKasi(string filename)
        {
            File.WriteAllText(filename, Kasi.Savedata());
            Kasi.ContentSaved();
        }

        private static void SaveIni(string filename)
        {
            var sw = new SettingWriter(filename, true);
            IniWriter.SaveIni(sw);
            sw.Close();
            IniWriter.ContentSaved();
        }

        private static void SaveEvd(Stream stream)
        {
            EVDWriter.Write(stream, Events.GetEvdData());
            Events.ContentSaved();
        }

        private static void SaveEvd(string filename)
        {
            using FileStream fs = File.Create(filename);
            SaveEvd(fs);
        }

        private static void SaveLayers(string insideDirectory)
        {
            LayerViewModel[] lds = Layers.AllLayerDisplay;
            var dir = Path.Combine(insideDirectory, "layers");
            ProjectFiles.EnsureDirectory(dir);
            for (int i = 0; i < lds.Length; i++)
            {
                var path = Path.Combine(dir, "layer" + i + ".ppd");
                ProjectFiles.DeleteIfExists(path);
                SavePpdFile(path, lds[i].PPDData.GetSortedDataAsMarkData());
            }
        }

        #endregion
    }
}
