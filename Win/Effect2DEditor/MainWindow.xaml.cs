using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using BezierCaliculator;
using Effect2D;
using Effect2DEditor.Command;
using Effect2DEditor.DockForm;
using Microsoft.Win32;
using PPDConfiguration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ColorTranslator = System.Drawing.ColorTranslator;
using PointF = System.Drawing.PointF;

namespace Effect2DEditor
{
    public partial class MainWindow : Window
    {
        const string iniFileName = "Effect2DEditor.ini";
        const string recentUsedFileName = "recentused.ini";
        const string dockFileName = "dockinfo.xml";
        const string effectFilter = "ETD|*.etd";
        const int DefaultCanvasSize = 512;

        bool Saved = true;

        string openfilename = "";
        string asksave = "未保存のデータがあります。保存しますか？";
        string askanimation = "編集履歴が削除されますがよろしいですか？";
        string confirm = "確認";

        string addeffect = "エフェクト追加:";
        string addimage = "イメージ追加:";
        string delete = "削除:";
        string selectstate = "ステート選択:";
        string state = "ステート";
        string changepermutation = "順序入れ替え:";
        string reversepermutation = "順序反転";
        string fix = "固定";
        string move = "移動:";
        string key = "キー";
        string insertstate = "ステート挿入:";
        string deletestate = "ステート削除:";
        string changeparamater = "パラメータ変更:";
        string changeblend = "ブレンド変更:";
        string changeratio = "変分変更";
        string changebezierposition = "ベジエ位置調整";
        string resetpath = "リセットパス";
        string copy = "コピー:";
        string cleareffect = "クリアエフェクト";

        private string langFileName = "";
        private string langFileISO = "";

        SettingReader lang;

        public BezierControlPoint[] CopiedBezier
        {
            get;
            set;
        }

        public CanvasDock CanvasDock
        {
            get
            {
                return canvasDock;
            }
        }

        public PropertyDock PropertyDock
        {
            get
            {
                return propertyDock;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            timeLineDock.MainForm = this;

            canvasDock.BezierEdited += CanvasDock_BezierEdited;
            canvasDock.BezierReset += CanvasDock_BezierReset;
            historyDock.CommandManager.CommandChanged += CommandManager_CommandChanged;
            propertyDock.StateValueChanged += PropertyDock_StateValueChanged;
            propertyDock.SetRatio += PropertyDock_SetRatio;
            propertyDock.BlendModeChanged += PropertyDock_BlendModeChanged;
            timeLineDock.FpsChanged += TimeLineDock_FpsChanged;
            timeLineDock.EffectCleared += TimeLineDock_EffectCleared;
            timeLineDock.EffectCopyed += TimeLineDock_EffectCopyed;
            timeLineDock.EffectDeleted += TimeLineDock_EffectDeleted;
            timeLineDock.EffectLoaded += TimeLineDock_EffectLoaded;
            timeLineDock.EffectReversed += TimeLineDock_EffectReversed;

            LoadConfig();
            if (canvasDock.BezierControl.Drawer == null)
            {
                canvasDock.Initialize(DefaultCanvasSize, DefaultCanvasSize);
            }
            CheckSetting();
            CheckLangFiles();
            // 配置を読み込むとウィンドウのタイトルも読み込まれるので、言語はその後に設定する
            RestoreDock();
            SetLanguage(langFileName);

            ResetEffect(new EffectManager());
        }

        /// <summary>
        /// ウィンドウの配置を読み込みます。以前の WinForms 版の形式など、読み込めないときは既定の配置のままにします。
        /// </summary>
        private void RestoreDock()
        {
            if (!File.Exists(dockFileName))
            {
                return;
            }
            try
            {
                var serializer = new XmlLayoutSerializer(dockingManager);
                serializer.LayoutSerializationCallback += (sender, e) =>
                {
                    e.Content = e.Model.ContentId switch
                    {
                        "Effect2DEditor.DockForm.CanvasDock" => canvasDock,
                        "Effect2DEditor.DockForm.PropertyDock" => propertyDock,
                        "Effect2DEditor.DockForm.HistoryDock" => historyDock,
                        "Effect2DEditor.DockForm.TimeLineDock" => timeLineDock,
                        _ => null,
                    };
                    e.Cancel = e.Content == null;
                };
                serializer.Deserialize(dockFileName);
            }
            catch (Exception)
            {
            }
            RebindAnchorables();
        }

        /// <summary>
        /// 配置を読み込むと、ウィンドウ（LayoutAnchorable）は新しく作られるので、「表示」メニューで扱うものを付け替えます。
        /// 読み込んだ配置になかったウィンドウは、配置に加え直します。
        /// </summary>
        private void RebindAnchorables()
        {
            var anchorables = dockingManager.Layout.Descendents().OfType<LayoutAnchorable>()
                .Concat(dockingManager.Layout.Hidden)
                .Distinct()
                .ToList();
            canvasAnchorable = Rebind(anchorables, canvasAnchorable);
            timeLineAnchorable = Rebind(anchorables, timeLineAnchorable);
            historyAnchorable = Rebind(anchorables, historyAnchorable);
            propertyAnchorable = Rebind(anchorables, propertyAnchorable);
        }

        private LayoutAnchorable Rebind(List<LayoutAnchorable> anchorables, LayoutAnchorable original)
        {
            var found = anchorables.FirstOrDefault(a => a.ContentId == original.ContentId);
            if (found != null)
            {
                return found;
            }
            if (original.Root == null)
            {
                original.AddToLayout(dockingManager, AnchorableShowStrategy.Most);
            }
            return original;
        }

        private void SaveDock()
        {
            try
            {
                new XmlLayoutSerializer(dockingManager).Serialize(dockFileName);
            }
            catch (IOException)
            {
            }
        }

        void TimeLineDock_EffectReversed()
        {
            if (canvasDock.EffectManager.Effects.Count >= 1 && canvasDock.SelectedManager.Effect != null)
            {
                this.ReverseEffects();
            }
        }

        void TimeLineDock_EffectLoaded(string obj)
        {
            LoadEffect(obj);
        }

        void TimeLineDock_EffectDeleted()
        {
            if (canvasDock.SelectedManager.Effect != null)
            {
                var index = canvasDock.EffectManager.Effects.IndexOf(canvasDock.SelectedManager.Effect);
                if (index != -1)
                {
                    DeleteEffect(index);
                }
            }
        }

        void TimeLineDock_EffectCopyed()
        {
            if (canvasDock.EffectManager.Effects.Count >= 1 && canvasDock.SelectedManager.Effect != null)
            {
                this.CopyEffect();
            }
        }

        void TimeLineDock_EffectCleared()
        {
            if (canvasDock.EffectManager.Effects.Count >= 1)
            {
                this.ClearEffect();
            }
        }

        void TimeLineDock_FpsChanged(float obj)
        {
            canvasDock.EffectManager.FPS = obj;
        }

        void PropertyDock_SetRatio(RatioType obj)
        {
            if (canvasDock.SelectedManager.Set != null)
            {
                var rcf = new RatioChangeWindow
                {
                    Owner = this,
                    MainForm = this
                };
                rcf.SetLang(lang);
                if (canvasDock.SelectedManager.Set[obj] is BezierRatioMaker brm && brm.Analyzer.BCPS != null)
                {
                    rcf.Bcps = brm.Analyzer.BCPS;
                }
                if (rcf.ShowDialog() == true)
                {
                    if (rcf.IsLinear) ChangeRatioMaker(obj, new LinearRatioMaker());
                    else ChangeRatioMaker(obj, new BezierRatioMaker(rcf.Bcps[0], rcf.Bcps[1]));
                }
            }
        }

        void PropertyDock_StateValueChanged(RatioType arg1, float arg2)
        {
            if (canvasDock.SelectedManager.State != null)
            {
                ChangeStateParameter(arg1, arg2);
                canvasDock.EffectManager?.Update(timeLineDock.CurrentFrame, null);
                RefreshCanvas();
            }
        }

        private void PropertyDock_BlendModeChanged(BlendMode obj)
        {
            if (canvasDock.SelectedManager.State != null)
            {
                ChangeBlendMode(obj);
                canvasDock.EffectManager?.Update(timeLineDock.CurrentFrame, null);
                RefreshCanvas();
            }
        }

        void CanvasDock_BezierReset(object sender, EventArgs e)
        {
            ResetPath();
        }

        void CanvasDock_BezierEdited(object sender, EventArgs e)
        {
            BezierEdit();
        }

        private void CheckSetting()
        {
            if (File.Exists(iniFileName))
            {
                var setting = new SettingReader(File.ReadAllText(iniFileName));
                langFileISO = setting["Language"];
            }
            if (string.IsNullOrEmpty(langFileISO))
            {
                // 言語を選んでいないときは、最初から表示している日本語にする
                langFileISO = "jp";
            }
            langFileName = Path.Combine("Lang", String.Format("lang_{0}_{1}.ini", this.GetType().Assembly.GetName().Name, langFileISO));
        }

        private void CheckLangFiles()
        {
            if (Directory.Exists("Lang"))
            {
                var componentName = this.GetType().Assembly.GetName().Name;
                foreach (string fileName in Directory.GetFiles("Lang", String.Format("lang_{0}_*.ini", componentName)))
                {
                    var setting = new SettingReader(File.ReadAllText(fileName));
                    var menuItem = new MenuItem
                    {
                        // 名前の「_」がアクセスキーとして扱われないように、TextBlock で表示する
                        Header = new TextBlock { Text = setting["DisplayName"] },
                        Tag = fileName.ToLower(),
                        IsChecked = Path.GetFileName(fileName).Equals(String.Format("lang_{0}_{1}.ini", componentName, langFileISO), StringComparison.CurrentCultureIgnoreCase)
                    };
                    menuItem.Click += Langtsmi_Click;
                    言語ToolStripMenuItem.Items.Add(menuItem);
                }
            }
        }

        void Langtsmi_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem tsmi)
            {
                langFileName = (string)tsmi.Tag;
                var m = LangFileRegex().Match(Path.GetFileName(langFileName));
                if (m.Success)
                {
                    langFileISO = m.Groups["ISO"].Value;
                }
                SetLanguage(langFileName);
                foreach (var child in 言語ToolStripMenuItem.Items.OfType<MenuItem>())
                {
                    child.IsChecked = false;
                }
                tsmi.IsChecked = true;
            }
        }

        private void SetLanguage(string fileName)
        {
            if (File.Exists(fileName))
            {
                lang = new SettingReader(File.ReadAllText(fileName));
                言語ToolStripMenuItem.Header = lang["Language"];
                ファイルToolStripMenuItem.Header = lang["File"];
                新規作成ToolStripMenuItem.Header = lang["CreateNew"];
                開くToolStripMenuItem.Header = lang["Open"];
                保存するToolStripMenuItem.Header = lang["Save"];
                上書き保存するToolStripMenuItem.Header = lang["OverWrite"];
                最近使用したファイルToolStripMenuItem.Header = lang["RecentUsedFile"];
                終了するToolStripMenuItem.Header = lang["Exit"];
                編集ToolStripMenuItem.Header = lang["Edit"];
                元に戻すToolStripMenuItem.Header = lang["Undo"];
                やり直すToolStripMenuItem.Header = lang["Redo"];
                アニメーションToolStripMenuItem.Header = lang["Animation"];
                カンバスサイズToolStripMenuItem.Header = lang["CanvasSize"];
                カンバス背景変更ToolStripMenuItem.Header = lang["CanvasBackGround"];
                表示ToolStripMenuItem.Header = lang["Show"];
                askanimation = lang["AskAnimation"];
                asksave = lang["AskSave"];
                confirm = lang["Confirm"];

                addeffect = lang["CAddEffect"];
                addimage = lang["CAddImage"];
                delete = lang["CDelete"];
                selectstate = lang["CSelectState"];
                state = lang["CState"];
                changepermutation = lang["CChangePermutation"];
                reversepermutation = lang["CReversePermutation"];
                fix = lang["CFixed"];
                move = lang["CMove"];
                key = lang["CKey"];
                insertstate = lang["CInsertState"];
                deletestate = lang["CDeleteState"];
                changeparamater = lang["CChangeParameter"];
                changeblend = lang["CChangeBlend"];
                changeratio = lang["CChangeRatio"];
                changebezierposition = lang["CChangeBezierPosition"];
                resetpath = lang["CResetPath"];
                copy = lang["CCopy"];
                cleareffect = lang["CClearEffect"];

                キャンバスウィンドウToolStripMenuItem.Header = canvasAnchorable.Title = lang["CanvasWindow"];
                プロパティウィンドウToolStripMenuItem.Header = propertyAnchorable.Title = lang["PropertyWindow"];
                ヒストリーウィンドウToolStripMenuItem.Header = historyAnchorable.Title = lang["HistoryWindow"];
                タイムラインウィンドウToolStripMenuItem.Header = timeLineAnchorable.Title = lang["TimeLineWindow"];

                canvasDock.SetLang(lang);
                timeLineDock.SetLang(lang);
            }
        }

        void SelectedManager_StateChanged(object sender, StateChangeEventArgs e)
        {
            SelectState(e);
            RefreshState();
        }

        void CommandManager_CommandChanged(object sender, EventArgs e)
        {
            Saved = false;
            canvasDock.EffectManager.CheckFrameLength();
            timeLineDock.AdjustHScroll();
            RefreshTimeLine();
        }

        public void RefreshTimeLine()
        {
            timeLineDock.DrawAndRefresh();
        }

        private void LoadEffect(string filename)
        {
            if (Path.GetExtension(filename) == ".etd") ReadEffect(filename);
            else AddEffect(filename);
        }

        #region Command

        private void ReadEffect(string filename)
        {
            try
            {
                var lef = new LoadEffectCommand(canvasDock.EffectManager, addeffect + Path.GetFileNameWithoutExtension(filename), canvasDock.EffectManager.Effects.Count, filename);
                lef.Execute();
                historyDock.CommandManager.AddCommand(lef);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void AddEffect(string filename)
        {
            try
            {
                var afc = new AddEffectCommand(canvasDock.EffectManager, addimage + Path.GetFileNameWithoutExtension(filename), canvasDock.EffectManager.Effects.Count, filename);
                afc.Execute();
                historyDock.CommandManager.AddCommand(afc);
                timeLineDock.MoveVScroll(0);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void DeleteEffect(int index)
        {
            try
            {
                var be = canvasDock.EffectManager.Effects[index] as BaseEffect;
                var name = Path.GetFileNameWithoutExtension(be.Filename);
                var dec = new DeleteEffectCommand(canvasDock.EffectManager, delete + name, index);
                dec.Execute();
                historyDock.CommandManager.AddCommand(dec);
                timeLineDock.MoveVScroll(-1);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void SelectState(StateChangeEventArgs ea)
        {
            try
            {
                if (historyDock.CommandManager.LastDoneCommand is SelectStateCommand ifssc)
                {
                    if (ifssc.StateIndex == ea.StateIndex && ifssc.ItemIndex == ea.ItemIndex) return;
                }
                var be = canvasDock.EffectManager.Effects[ea.ItemIndex] as BaseEffect;
                var name = Path.GetFileNameWithoutExtension(be.Filename);
                var ssc = new SelectStateCommand(canvasDock.EffectManager, selectstate + name + "," + ea.StateIndex + state, canvasDock.SelectedManager, ea.LastItemIndex, ea.ItemIndex, ea.LastStateIndex, ea.StateIndex);
                //ssc.Execute();
                historyDock.CommandManager.AddCommand(ssc);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void ChangeIndex(int index, int beforeindex)
        {
            try
            {
                var be = canvasDock.EffectManager.Effects[beforeindex] as BaseEffect;
                var name = Path.GetFileNameWithoutExtension(be.Filename);
                var cic = new ChangeIndexCommand(canvasDock.EffectManager, changepermutation + name, index, beforeindex);
                cic.Execute();
                historyDock.CommandManager.AddCommand(cic);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void ReverseEffects()
        {
            try
            {
                var rec = new ReverseEffectsCommand(canvasDock.EffectManager, reversepermutation);
                rec.Execute();
                historyDock.CommandManager.AddCommand(rec);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void MoveState(int beforeframe, int newframe, TimeLine.KeyOperationMode opmode)
        {
            if (beforeframe == 0)
            {
                return;
            }
            try
            {
                var be = canvasDock.SelectedManager.Effect as BaseEffect;
                var name = Path.GetFileNameWithoutExtension(be.Filename);
                var msc = new MoveStateCommand(canvasDock.EffectManager, state + (opmode == TimeLine.KeyOperationMode.LeftMoveOnly || opmode == TimeLine.KeyOperationMode.RightMoveOnly ? fix : "") + move + name, beforeframe, newframe, opmode, canvasDock.SelectedManager);
                //msc.Execute();
                historyDock.CommandManager.AddCommand(msc);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        public void AddState(int itemindex, int keyindex, bool iskey)
        {
            try
            {
                var be = canvasDock.EffectManager.Effects[itemindex] as BaseEffect;
                var name = Path.GetFileNameWithoutExtension(be.Filename);
                var asc = new AddStateCommand(canvasDock.EffectManager, (iskey ? key : "") + insertstate + name, iskey, itemindex, keyindex);
                asc.Execute();
                historyDock.CommandManager.AddCommand(asc);
                RefreshState();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        public void DeleteState(int itemindex, int keyindex)
        {
            try
            {
                var be = canvasDock.EffectManager.Effects[itemindex] as BaseEffect;
                var name = Path.GetFileNameWithoutExtension(be.Filename);
                var dsc = new DeleteStateCommand(canvasDock.EffectManager, deletestate + name, itemindex, keyindex);
                dsc.Execute();
                historyDock.CommandManager.AddCommand(dsc);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void ChangeStateParameter(RatioType type, float value)
        {
            try
            {
                if (historyDock.CommandManager.LastCommand is StateParameterChangeCommand lastspcc && lastspcc.Type == type)
                {
                    lastspcc.Value = value;
                    lastspcc.SpecialExecute();
                    return;
                }
                var spcc = new StateParameterChangeCommand(canvasDock.EffectManager, changeparamater + type, canvasDock.SelectedManager, type, value);
                spcc.Execute();
                historyDock.CommandManager.AddCommand(spcc);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void ChangeBlendMode(BlendMode blendMode)
        {
            try
            {
                if (historyDock.CommandManager.LastCommand is StateBlendModeChangeCommand lastsbmcc && lastsbmcc.BlendMode == blendMode)
                {
                    lastsbmcc.BlendMode = blendMode;
                    lastsbmcc.SpecialExecute();
                    return;
                }
                var sbmcc = new StateBlendModeChangeCommand(canvasDock.EffectManager, changeblend + blendMode, canvasDock.SelectedManager, blendMode);
                sbmcc.Execute();
                historyDock.CommandManager.AddCommand(sbmcc);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void ChangeRatioMaker(RatioType type, IRatioMaker maker)
        {
            try
            {
                var srmcc = new SetRatioMakerChangeCommand(canvasDock.EffectManager, changeratio, canvasDock.SelectedManager, type, maker);
                srmcc.Execute();
                historyDock.CommandManager.AddCommand(srmcc);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void BezierEdit()
        {
            try
            {
                BezierControlPoint[] bcps = new BezierControlPoint[canvasDock.BezierControl.Controller.BCPS.Length];
                for (int i = 0; i < bcps.Length; i++) bcps[i] = (BezierControlPoint)canvasDock.BezierControl.Controller.BCPS[i].Clone();
                if (historyDock.CommandManager.LastCommand is BezierPositionChangeCommand lastbpcc)
                {
                    lastbpcc.BCPS = bcps;
                    lastbpcc.SpecialExecute();
                    return;
                }
                var bpcc = new BezierPositionChangeCommand(canvasDock.EffectManager, changebezierposition, canvasDock.SelectedManager, bcps);
                bpcc.Execute();
                historyDock.CommandManager.AddCommand(bpcc);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void ResetPath()
        {
            try
            {
                var rpc = new ResetPathCommand(canvasDock.EffectManager, resetpath, canvasDock.SelectedManager);
                rpc.Execute();
                RefreshState();
                historyDock.CommandManager.AddCommand(rpc);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void CopyEffect()
        {
            try
            {
                var be = canvasDock.SelectedManager.Effect as BaseEffect;
                var name = Path.GetFileNameWithoutExtension(be.Filename);
                var cec = new CopyEffectCommand(canvasDock.EffectManager, copy + name, canvasDock.SelectedManager);
                cec.Execute();
                historyDock.CommandManager.AddCommand(cec);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void ClearEffect()
        {
            try
            {
                var cec = new ClearEffectCommand(canvasDock.EffectManager, cleareffect);
                cec.Execute();
                historyDock.CommandManager.AddCommand(cec);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        #endregion

        public void RefreshCanvas()
        {
            canvasDock.RefreshCanvas();
        }

        private void CheckState()
        {
            if (canvasDock.SelectedManager.Set != null)
            {
                if (canvasDock.SelectedManager.Set.IsBezierPosition)
                {
                    canvasDock.BezierControl.Controller.BCPS = canvasDock.SelectedManager.Set.BAnalyzer.BCPS;
                    canvasDock.BezierControl.Controller.AllowMouseOperation = true;
                }
                else if (canvasDock.SelectedManager.Set[RatioType.X] is not ConstantRatioMaker)
                {
                    var bcp = new BezierControlPoint();
                    var bcp2 = new BezierControlPoint();
                    bcp.Second = new PointF(canvasDock.SelectedManager.Set.StartState.X, canvasDock.SelectedManager.Set.StartState.Y);
                    bcp2.Second = new PointF(canvasDock.SelectedManager.Set.EndState.X, canvasDock.SelectedManager.Set.EndState.Y);
                    canvasDock.BezierControl.Controller.BCPS = [bcp, bcp2];
                    canvasDock.BezierControl.Controller.AllowMouseOperation = true;
                }
                else
                {
                    canvasDock.BezierControl.Controller.BCPS = [];
                    canvasDock.BezierControl.Controller.AllowMouseOperation = false;
                }
                RefreshCanvas();
                if (propertyDock.IsRatioMakerAvailable)
                {
                    propertyDock.SetEnables(canvasDock.SelectedManager.Set.IsBezierPosition);
                }
            }
        }

        private void 保存するToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveAs();
        }

        /// <summary>
        /// 名前を付けて保存します。キャンセルしたときは false を返します。
        /// </summary>
        private bool SaveAs()
        {
            var dialog = new SaveFileDialog
            {
                Filter = effectFilter,
                DefaultExt = "etd",
                FileName = openfilename == "" ? "NewEffect.etd" : Path.GetFileName(openfilename)
            };
            if (openfilename != "")
            {
                dialog.InitialDirectory = Path.GetDirectoryName(Path.GetFullPath(openfilename));
            }
            if (dialog.ShowDialog(this) == true)
            {
                SaveEffect(dialog.FileName);
                Saved = true;
                return true;
            }
            return false;
        }

        private void SaveEffect(string filename)
        {
            canvasDock.EffectManager.Save(filename);
            var fn = Path.GetFileNameWithoutExtension(filename);
            string dir = Path.GetDirectoryName(filename) + "/" + fn;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            foreach (BaseEffect be in canvasDock.EffectManager.Effects.Cast<BaseEffect>())
            {
                if (be.Effects.Count != 0) continue;
                var resourcefname = Path.GetFileName(be.Filename);
                if (!File.Exists(dir + "/" + resourcefname)) File.Copy(be.Filename, dir + "/" + resourcefname, true);
            }
            openfilename = filename;
        }

        private void 元に戻すToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (historyDock.CommandManager.CanUndo)
            {
                var cu = historyDock.CommandManager.Undo();
                if (cu is StateParameterChangeCommand)
                {
                    propertyDock.SetState(canvasDock.SelectedManager.State);
                }
                else if (cu is ResetPathCommand || cu is BezierPositionChangeCommand)
                {
                    RefreshState();
                }
            }
        }

        private void やり直すToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (historyDock.CommandManager.CanRedo)
            {
                var cu = historyDock.CommandManager.Redo();
                if (cu is StateParameterChangeCommand)
                {
                    propertyDock.SetState(canvasDock.SelectedManager.State);
                }
                else if (cu is ResetPathCommand || cu is BezierPositionChangeCommand)
                {
                    RefreshState();
                }
            }
        }

        private void アニメーションToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var asc = new AskFrameCountWindow
            {
                Owner = this
            };
            asc.SetLang(lang);
            if (asc.ShowDialog() == true && MessageBox.Show(this, askanimation, confirm, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                CreateAnimation(asc.FrameCount, asc.WithReverse);
            }
        }

        private void CreateAnimation(int framecount, bool withreverse)
        {
            bool first = true;
            if (!withreverse)
            {
                for (int i = 0; i < canvasDock.EffectManager.Effects.Count; i++)
                {
                    IEffect effect = canvasDock.EffectManager.Effects[canvasDock.EffectManager.Effects.Count - 1 - i];
                    effect.Sets.Clear();
                    var ess1 = new EffectStateStructure();
                    var ess2 = new EffectStateStructure();
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        ess1.Alpha = 0;
                        var set1 = new EffectStateRatioSet();
                        foreach (RatioType type in Effect2D.Utility.RatioTypeArray)
                        {
                            set1[type] = new ConstantRatioMaker();
                        }
                        set1.StartFrame = 0;
                        set1.EndFrame = i * framecount;
                        set1.StartState = ess1;
                        set1.EndState = ess2;
                        effect.Sets.Add(set1.StartFrame, set1);
                    }
                    var ess3 = new EffectStateStructure
                    {
                        Alpha = 0
                    };
                    var set2 = new EffectStateRatioSet();
                    foreach (RatioType type in Effect2D.Utility.RatioTypeArray)
                    {
                        set2[type] = new ConstantRatioMaker();
                    }
                    set2.StartFrame = i * framecount;
                    set2.EndFrame = (i + 1) * framecount;
                    set2.StartState = ess2;
                    set2.EndState = ess3;
                    effect.Sets.Add(set2.StartFrame, set2);
                    if (i != canvasDock.EffectManager.Effects.Count - 1)
                    {
                        var ess4 = new EffectStateStructure
                        {
                            Alpha = 0
                        };
                        var set3 = new EffectStateRatioSet();
                        foreach (RatioType type in Effect2D.Utility.RatioTypeArray)
                        {
                            set3[type] = new ConstantRatioMaker();
                        }
                        set3.StartFrame = (i + 1) * framecount;
                        set3.EndFrame = (canvasDock.EffectManager.Effects.Count) * framecount;
                        set3.StartState = ess3;
                        set3.EndState = ess4;
                        effect.Sets.Add(set3.StartFrame, set3);
                    }
                }
            }
            else
            {
                for (int i = 0; i < canvasDock.EffectManager.Effects.Count; i++)
                {
                    IEffect effect = canvasDock.EffectManager.Effects[canvasDock.EffectManager.Effects.Count - 1 - i];
                    effect.Sets.Clear();
                    var ess1 = new EffectStateStructure();
                    var ess2 = new EffectStateStructure();
                    if (!first)
                    {
                        ess1.Alpha = 0;
                        var set1 = new EffectStateRatioSet();
                        foreach (RatioType type in Effect2D.Utility.RatioTypeArray)
                        {
                            set1[type] = new ConstantRatioMaker();
                        }
                        set1.StartFrame = 0;
                        set1.EndFrame = i * framecount;
                        set1.StartState = ess1;
                        set1.EndState = ess2;
                        effect.Sets.Add(set1.StartFrame, set1);
                    }
                    var ess3 = new EffectStateStructure
                    {
                        Alpha = 0
                    };
                    var set2 = new EffectStateRatioSet();
                    foreach (RatioType type in Effect2D.Utility.RatioTypeArray)
                    {
                        set2[type] = new ConstantRatioMaker();
                    }
                    set2.StartFrame = i * framecount;
                    set2.EndFrame = (i + 1) * framecount;
                    set2.StartState = ess2;
                    set2.EndState = ess3;
                    effect.Sets.Add(set2.StartFrame, set2);
                    if (i != canvasDock.EffectManager.Effects.Count - 1)
                    {
                        var ess4 = new EffectStateStructure
                        {
                            Alpha = 1
                        };
                        var set3 = new EffectStateRatioSet();
                        foreach (RatioType type in Effect2D.Utility.RatioTypeArray)
                        {
                            set3[type] = new ConstantRatioMaker();
                        }
                        set3.StartFrame = (i + 1) * framecount;
                        set3.EndFrame = ((canvasDock.EffectManager.Effects.Count - 1) * 2 - i) * framecount;
                        set3.StartState = ess3;
                        set3.EndState = ess4;
                        effect.Sets.Add(set3.StartFrame, set3);
                        ess3 = ess4;
                    }
                    var ess5 = new EffectStateStructure();
                    var set4 = new EffectStateRatioSet();
                    ess5.Alpha = 0;
                    foreach (RatioType type in Effect2D.Utility.RatioTypeArray)
                    {
                        set4[type] = new ConstantRatioMaker();
                    }
                    set4.StartFrame = ((canvasDock.EffectManager.Effects.Count - 1) * 2 - i) * framecount;
                    set4.EndFrame = set4.StartFrame + framecount;
                    set4.StartState = ess3;
                    set4.EndState = ess5;
                    if (i != canvasDock.EffectManager.Effects.Count - 1) effect.Sets.Add(set4.StartFrame, set4);
                    if (!first)
                    {
                        var set5 = new EffectStateRatioSet();
                        var ess6 = new EffectStateStructure
                        {
                            Alpha = 0
                        };
                        foreach (RatioType type in Effect2D.Utility.RatioTypeArray)
                        {
                            set5[type] = new ConstantRatioMaker();
                        }
                        set5.StartFrame = set4.EndFrame;
                        set5.EndFrame = framecount * (2 * canvasDock.EffectManager.Effects.Count - 1);
                        set5.StartState = i == canvasDock.EffectManager.Effects.Count - 1 ? ess3 : ess5;
                        set5.EndState = ess6;
                        effect.Sets.Add(set5.StartFrame, set5);
                    }
                    first = false;
                }
            }
            historyDock.CommandManager.ClearAll();
            timeLineDock.AdjustHScroll();
            RefreshTimeLine();
        }

        private void 開くToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CheckSave())
            {
                var dialog = new OpenFileDialog
                {
                    Filter = effectFilter,
                    Multiselect = false
                };
                if (openfilename != "")
                {
                    dialog.FileName = Path.GetFileName(openfilename);
                    dialog.InitialDirectory = Path.GetDirectoryName(Path.GetFullPath(openfilename));
                }
                if (dialog.ShowDialog(this) == true)
                {
                    OpenEffect(dialog.FileName);
                }
            }
        }

        private void OpenEffect(string effectfilename)
        {
            ResetEffect(EffectLoader.Load(effectfilename, (filename) =>
            {
                try
                {
                    Utility.ValidateImage(filename);
                }
                catch
                {
                    MessageBox.Show(this, "Failed to load image");
                }
            }));
            openfilename = effectfilename;

            var existing = 最近使用したファイルToolStripMenuItem.Items.OfType<MenuItem>().FirstOrDefault(item => (string)item.Tag == effectfilename);
            if (existing != null)
            {
                最近使用したファイルToolStripMenuItem.Items.Remove(existing);
                最近使用したファイルToolStripMenuItem.Items.Insert(0, existing);
            }
            else
            {
                最近使用したファイルToolStripMenuItem.Items.Insert(0, CreateRecentMenuItem(effectfilename));
                if (最近使用したファイルToolStripMenuItem.Items.Count > 5)
                {
                    最近使用したファイルToolStripMenuItem.Items.RemoveAt(最近使用したファイルToolStripMenuItem.Items.Count - 1);
                }
            }
        }

        private MenuItem CreateRecentMenuItem(string path)
        {
            var menuItem = new MenuItem
            {
                // パスの「_」がアクセスキーとして扱われないように、TextBlock で表示する
                Header = new TextBlock { Text = path },
                Tag = path
            };
            menuItem.Click += Tsmi_Click;
            return menuItem;
        }

        private void カンバスサイズToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var csf = new CanvasSizeWindow
            {
                Owner = this
            };
            csf.SetLang(lang);
            csf.CanvasHeight = canvasDock.CanvasHeight;
            csf.CanvasWidth = canvasDock.CanvasWidth;
            if (csf.ShowDialog() == true)
            {
                canvasDock.Initialize(Math.Max(1, csf.CanvasWidth), Math.Max(1, csf.CanvasHeight));
            }
        }

        private void RefreshState()
        {
            propertyDock.SetState(canvasDock.SelectedManager.State);
            CheckState();
        }

        private void カンバス背景変更ToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var cbgf = new ChangeBackGroundWindow
            {
                Owner = this
            };
            cbgf.SetLang(lang);
            cbgf.Mode = canvasDock.BackGroundMode;
            cbgf.Color = canvasDock.BackGroundColor;
            cbgf.Filename = canvasDock.BackImageFileName;
            if (cbgf.ShowDialog() == true)
            {
                canvasDock.BackGroundMode = cbgf.Mode;
                switch (cbgf.Mode)
                {
                    case BackGroundMode.Default:
                        break;
                    case BackGroundMode.Color:
                        canvasDock.BackGroundColor = cbgf.Color;
                        break;
                    case BackGroundMode.Image:
                        canvasDock.BackImageFileName = cbgf.Filename;
                        break;
                }
                RefreshCanvas();
            }
        }

        private void 上書き保存するToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(openfilename))
            {
                SaveEffect(openfilename);
                Saved = true;
            }
            else
            {
                SaveAs();
            }
        }

        private void 終了するToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MainForm_FormClosing(object sender, CancelEventArgs e)
        {
            if (!CheckSave())
            {
                e.Cancel = true;
            }
            else
            {
                SaveConfig();
                SaveDock();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            canvasDock.Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // 以前のメニューのショートカットキー（テキストボックスの中では、テキストボックスの操作が優先される）
            var modifiers = Keyboard.Modifiers;
            if (modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.N:
                        新規作成ToolStripMenuItem_Click(this, e);
                        break;
                    case Key.O:
                        開くToolStripMenuItem_Click(this, e);
                        break;
                    case Key.S:
                        上書き保存するToolStripMenuItem_Click(this, e);
                        break;
                    case Key.Z:
                        元に戻すToolStripMenuItem_Click(this, e);
                        break;
                    case Key.Y:
                        やり直すToolStripMenuItem_Click(this, e);
                        break;
                    default:
                        return;
                }
                e.Handled = true;
            }
            else if (modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.S)
            {
                保存するToolStripMenuItem_Click(this, e);
                e.Handled = true;
            }
        }

        private void SaveConfig()
        {
            var setting = new SettingReader(File.Exists(iniFileName) ? File.ReadAllText(iniFileName) : "");
            var bounds = WindowState == WindowState.Normal ? new Rect(Left, Top, Width, Height) : RestoreBounds;
            setting.ReplaceOrAdd("Language", langFileISO);
            setting.ReplaceOrAdd("WindowWidth", (int)bounds.Width);
            setting.ReplaceOrAdd("WindowHeight", (int)bounds.Height);
            setting.ReplaceOrAdd("WindowState", (int)WindowState);
            setting.ReplaceOrAdd("CanvasWidth", canvasDock.CanvasWidth);
            setting.ReplaceOrAdd("CanvasHeight", canvasDock.CanvasHeight);
            setting.ReplaceOrAdd("CanvasBackGround", (int)canvasDock.BackGroundMode);
            setting.ReplaceOrAdd("CanvasBackGroundColor", ColorTranslator.ToHtml(canvasDock.BackGroundColor));
            setting.ReplaceOrAdd("CanvasBackGroundImage", canvasDock.BackImageFileName);
            setting.ReplaceOrAdd("Language", langFileISO);
            using (SettingWriter sw = new(iniFileName, false))
            {
                foreach (KeyValuePair<string, string> kvp in setting.Dictionary)
                {
                    sw.Write(kvp.Key, kvp.Value);
                }
            }
            using (StreamWriter sw = new(recentUsedFileName))
            {
                foreach (var item in 最近使用したファイルToolStripMenuItem.Items.OfType<MenuItem>())
                {
                    sw.WriteLine((string)item.Tag);
                }
            }
        }

        private void LoadConfig()
        {
            try
            {
                if (File.Exists(iniFileName))
                {
                    var setting = new SettingReader(File.ReadAllText(iniFileName));
                    var width = DefaultCanvasSize;
                    var height = DefaultCanvasSize;
                    if (int.TryParse(setting["WindowWidth"], out int temp)) Width = temp;
                    if (int.TryParse(setting["WindowHeight"], out temp)) Height = temp;
                    // 最小化のまま起動しないようにする（値は WinForms の FormWindowState と同じ）
                    if (int.TryParse(setting["WindowState"], out temp) && (WindowState)temp == WindowState.Maximized) WindowState = WindowState.Maximized;
                    if (int.TryParse(setting["CanvasWidth"], out temp))
                    {
                        width = temp;
                    }
                    if (int.TryParse(setting["CanvasHeight"], out temp))
                    {
                        height = temp;
                    }
                    canvasDock.Initialize(Math.Max(1, width), Math.Max(1, height));

                    if (int.TryParse(setting["CanvasBackGround"], out temp))
                    {
                        canvasDock.BackGroundMode = (BackGroundMode)temp;
                        switch (canvasDock.BackGroundMode)
                        {
                            case BackGroundMode.Default:
                                break;
                            case BackGroundMode.Color:
                                try
                                {
                                    canvasDock.BackGroundColor = ColorTranslator.FromHtml(setting["CanvasBackGroundColor"]);
                                }
                                catch
                                {
                                }
                                break;
                            case BackGroundMode.Image:
                                canvasDock.BackImageFileName = setting["CanvasBackGroundImage"];
                                break;
                        }
                        RefreshCanvas();
                    }
                }
                if (File.Exists(recentUsedFileName))
                {
                    foreach (var path in File.ReadAllLines(recentUsedFileName))
                    {
                        if (!File.Exists(path)) continue;
                        最近使用したファイルToolStripMenuItem.Items.Add(CreateRecentMenuItem(path));
                    }
                }
            }
            catch
            {
            }
        }

        void Tsmi_Click(object sender, RoutedEventArgs e)
        {
            if (CheckSave())
            {
                OpenEffect((string)((MenuItem)sender).Tag);
            }
        }

        /// <summary>
        /// 保存していない変更があれば、保存するかを確認します。続けてよいときは true を返します。
        /// </summary>
        private bool CheckSave()
        {
            if (!Saved)
            {
                var dr = MessageBox.Show(this, asksave, confirm, MessageBoxButton.YesNoCancel);
                if (dr == MessageBoxResult.Yes)
                {
                    return SaveAs();
                }
                return dr != MessageBoxResult.Cancel;
            }
            return true;
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop, false) is not string[] fileName)
            {
                return;
            }
            if (File.Exists(fileName[0]) && Path.GetExtension(fileName[0]) == ".etd")
            {
                if (CheckSave())
                {
                    OpenEffect(fileName[0]);
                }
            }
        }

        private void 新規作成ToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CheckSave())
            {
                ResetEffect(new EffectManager());
            }
        }

        private void ResetEffect(EffectManager effectManager)
        {
            openfilename = "";
            canvasDock.SetEffectManager(effectManager);
            timeLineDock.Initialize();
            historyDock.CommandManager.ClearAll();
            RefreshTimeLine();
            Saved = true;
            canvasDock.SelectedManager.StateChanged += SelectedManager_StateChanged;
            canvasDock.EffectManager.Finish += EffectManager_Finish;
        }

        void EffectManager_Finish(object sender, EventArgs e)
        {
            timeLineDock.EffectManagerPlayFinished();
        }

        private void キャンバスウィンドウToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Utility.ShowOrHideWindow(canvasAnchorable);
        }

        private void プロパティウィンドウToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Utility.ShowOrHideWindow(propertyAnchorable);
        }

        private void ヒストリーウィンドウToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Utility.ShowOrHideWindow(historyAnchorable);
        }

        private void タイムラインウィンドウToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Utility.ShowOrHideWindow(timeLineAnchorable);
        }

        [GeneratedRegex("^lang_\\w+_(?<ISO>\\w+).ini$")]
        private static partial Regex LangFileRegex();
    }
}
