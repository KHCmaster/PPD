using BezierCaliculator;
using Effect2D;
using Effect2DEditor.Command;
using Effect2DEditor.DockForm;
using PPDConfiguration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Effect2DEditor
{
    public partial class MainForm : Form
    {
        const string iniFileName = "Effect2DEditor.ini";
        const string recentUsedFileName = "recentused.ini";
        const string dockFileName = "dockinfo.xml";

        bool Saved = true;

        private static CanvasDock canvasDock = new CanvasDock();
        private static HistoryDock historyDock = new HistoryDock();
        private static PropertyDock propertyDock = new PropertyDock();
        private static TimeLineDock timeLineDock = new TimeLineDock();

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

        public MainForm()
        {
            timeLineDock.MainForm = this;
            InitializeComponent();

            canvasDock.BezierEdited += canvasDock_BezierEdited;
            canvasDock.BezierReset += canvasDock_BezierReset;
            historyDock.CommandManager.CommandChanged += CommandManager_CommandChanged;
            propertyDock.StateValueChanged += propertyDock_StateValueChanged;
            propertyDock.SetRatio += propertyDock_SetRatio;
            propertyDock.BlendModeChanged += PropertyDock_BlendModeChanged;
            timeLineDock.FpsChanged += timeLineDock_FpsChanged;
            timeLineDock.EffectCleared += timeLineDock_EffectCleared;
            timeLineDock.EffectCopyed += timeLineDock_EffectCopyed;
            timeLineDock.EffectDeleted += timeLineDock_EffectDeleted;
            timeLineDock.EffectLoaded += timeLineDock_EffectLoaded;
            timeLineDock.EffectReversed += timeLineDock_EffectReversed;

            LoadConfig();
            CheckSetting();
            CheckLangFiles();
            SetLanguage(langFileName);

            RestoreDock();
            ResetEffect(new EffectManager());
        }

        private void RestoreDock()
        {
            if (File.Exists(dockFileName))
            {
                dockPanel1.LoadFromXml(dockFileName, (str) =>
                {
                    switch (str)
                    {
                        case "Effect2DEditor.DockForm.CanvasDock":
                            return canvasDock;
                        case "Effect2DEditor.DockForm.PropertyDock":
                            return propertyDock;
                        case "Effect2DEditor.DockForm.HistoryDock":
                            return historyDock;
                        case "Effect2DEditor.DockForm.TimeLineDock":
                            return timeLineDock;
                    }

                    return null;
                });
            }
            else
            {
                canvasDock.Show(dockPanel1, WeifenLuo.WinFormsUI.Docking.DockState.Document);
                historyDock.Show(dockPanel1, WeifenLuo.WinFormsUI.Docking.DockState.DockRight);
                propertyDock.Show(dockPanel1, WeifenLuo.WinFormsUI.Docking.DockState.DockRight);
                timeLineDock.Show(dockPanel1, WeifenLuo.WinFormsUI.Docking.DockState.DockBottom);
            }
        }

        private void SaveDock()
        {
            dockPanel1.SaveAsXml(dockFileName);
        }

        void timeLineDock_EffectReversed()
        {
            if (canvasDock.EffectManager.Effects.Count >= 1 && canvasDock.SelectedManager.Effect != null)
            {
                this.ReverseEffects();
            }
        }

        void timeLineDock_EffectLoaded(string obj)
        {
            LoadEffect(obj);
        }

        void timeLineDock_EffectDeleted()
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

        void timeLineDock_EffectCopyed()
        {
            if (canvasDock.EffectManager.Effects.Count >= 1 && canvasDock.SelectedManager.Effect != null)
            {
                this.CopyEffect();
            }
        }

        void timeLineDock_EffectCleared()
        {
            if (canvasDock.EffectManager.Effects.Count >= 1)
            {
                this.ClearEffect();
            }
        }

        void timeLineDock_FpsChanged(float obj)
        {
            canvasDock.EffectManager.FPS = obj;
        }

        void propertyDock_SetRatio(RatioType obj)
        {
            if (canvasDock.SelectedManager.Set != null)
            {
                var rcf = new RatioChangeForm();
                rcf.SetLang(lang);
                rcf.MainForm = this;
                if (canvasDock.SelectedManager.Set[obj] is BezierRatioMaker brm && brm.Analyzer.BCPS != null)
                {
                    rcf.bcps = brm.Analyzer.BCPS;
                }
                if (rcf.ShowDialog() == DialogResult.OK)
                {
                    if (rcf.IsLinear) ChangeRatioMaker(obj, new LinearRatioMaker());
                    else ChangeRatioMaker(obj, new BezierRatioMaker(rcf.bcps[0], rcf.bcps[1]));
                }
            }
        }

        void propertyDock_StateValueChanged(RatioType arg1, float arg2)
        {
            if (canvasDock.SelectedManager.State != null)
            {
                ChangeStateParameter(arg1, arg2);
                if (canvasDock.EffectManager != null)
                {
                    canvasDock.EffectManager.Update(timeLineDock.CurrentFrame, null);
                }
                RefreshCanvas();
            }
        }

        private void PropertyDock_BlendModeChanged(BlendMode obj)
        {
            if (canvasDock.SelectedManager.State != null)
            {
                ChangeBlendMode(obj);
                if (canvasDock.EffectManager != null)
                {
                    canvasDock.EffectManager.Update(timeLineDock.CurrentFrame, null);
                }
                RefreshCanvas();
            }
        }

        void canvasDock_BezierReset(object sender, EventArgs e)
        {
            ResetPath();
        }

        void canvasDock_BezierEdited(object sender, EventArgs e)
        {
            BezierEdit();
        }

        private void CheckSetting()
        {
            if (File.Exists(iniFileName))
            {
                var sr = new StreamReader(iniFileName);
                var setting = new SettingReader(sr.ReadToEnd());
                sr.Close();
                langFileISO = setting["Language"];
                langFileName = Path.Combine("Lang", String.Format("lang_{0}_{1}.ini", this.GetType().Assembly.GetName().Name, langFileISO));
            }
        }

        private void CheckLangFiles()
        {
            if (Directory.Exists("Lang"))
            {
                foreach (string fileName in Directory.GetFiles("Lang", String.Format("lang_{0}_*.ini", this.GetType().Assembly.GetName().Name)))
                {
                    var sr = new StreamReader(fileName);
                    var setting = new SettingReader(sr.ReadToEnd());
                    sr.ReadToEnd();
                    string name = setting["DisplayName"];
                    var tsmi = new ToolStripMenuItem
                    {
                        Text = name,
                        Name = fileName.ToLower(),
                        Checked = Path.GetFileName(fileName).ToLower() == String.Format("lang_{0}_{1}.ini", this.GetType().Assembly.GetName().Name, langFileISO).ToLower()
                    };
                    tsmi.Click += langtsmi_Click;
                    言語ToolStripMenuItem.DropDownItems.Add(tsmi);
                }
            }
        }

        void langtsmi_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem tsmi)
            {
                langFileName = tsmi.Name;
                var m = Regex.Match(Path.GetFileName(langFileName), "^lang_\\w+_(?<ISO>\\w+).ini$");
                if (m.Success)
                {
                    langFileISO = m.Groups["ISO"].Value;
                }
                SetLanguage(langFileName);
                foreach (ToolStripMenuItem child in (tsmi.OwnerItem as ToolStripMenuItem).DropDownItems)
                {
                    child.Checked = false;
                }
                tsmi.Checked = true;
            }
        }

        private void SetLanguage(string fileName)
        {
            if (File.Exists(fileName))
            {
                var sr = new StreamReader(fileName);
                lang = new SettingReader(sr.ReadToEnd());
                sr.Close();
                言語ToolStripMenuItem.Text = lang["Language"];
                ファイルToolStripMenuItem.Text = lang["File"];
                新規作成ToolStripMenuItem.Text = lang["CreateNew"];
                開くToolStripMenuItem.Text = lang["Open"];
                保存するToolStripMenuItem.Text = lang["Save"];
                上書き保存するToolStripMenuItem.Text = lang["OverWrite"];
                最近使用したファイルToolStripMenuItem.Text = lang["RecentUsedFile"];
                終了するToolStripMenuItem.Text = lang["Exit"];
                編集ToolStripMenuItem.Text = lang["Edit"];
                元に戻すToolStripMenuItem.Text = lang["Undo"];
                やり直すToolStripMenuItem.Text = lang["Redo"];
                アニメーションToolStripMenuItem.Text = lang["Animation"];
                カンバスサイズToolStripMenuItem.Text = lang["CanvasSize"];
                カンバス背景変更ToolStripMenuItem.Text = lang["CanvasBackGround"];
                表示ToolStripMenuItem.Text = lang["Show"];
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

                キャンバスウィンドウToolStripMenuItem.Text = canvasDock.TabText = lang["CanvasWindow"];
                プロパティウィンドウToolStripMenuItem.Text = propertyDock.TabText = lang["PropertyWindow"];
                ヒストリーウィンドウToolStripMenuItem.Text = historyDock.TabText = lang["HistoryWindow"];
                タイムラインウィンドウToolStripMenuItem.Text = timeLineDock.TabText = lang["TimeLineWindow"];

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
                var lef = new LoadEffectCommand(canvasDock.EffectManager, addeffect + Path.GetFileNameWithoutExtension(filename), canvasDock.EffectManager.Effects.Count, filename, canvasDock.ImagePool);
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
                var afc = new AddEffectCommand(canvasDock.EffectManager, addimage + Path.GetFileNameWithoutExtension(filename), canvasDock.EffectManager.Effects.Count, filename, canvasDock.ImagePool);
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
                else if (!(canvasDock.SelectedManager.Set[RatioType.X] is ConstantRatioMaker))
                {
                    var bcp = new BezierControlPoint();
                    var bcp2 = new BezierControlPoint();
                    bcp.Second = new PointF(canvasDock.SelectedManager.Set.StartState.X, canvasDock.SelectedManager.Set.StartState.Y);
                    bcp2.Second = new PointF(canvasDock.SelectedManager.Set.EndState.X, canvasDock.SelectedManager.Set.EndState.Y);
                    canvasDock.BezierControl.Controller.BCPS = new BezierControlPoint[] { bcp, bcp2 };
                    canvasDock.BezierControl.Controller.AllowMouseOperation = true;
                }
                else
                {
                    canvasDock.BezierControl.Controller.BCPS = new BezierControlPoint[0];
                    canvasDock.BezierControl.Controller.AllowMouseOperation = false;
                }
                RefreshCanvas();
                if (propertyDock.IsRatioMakerAvailable)
                {
                    propertyDock.SetEnables(canvasDock.SelectedManager.Set.IsBezierPosition);
                }
            }
        }

        private void 保存するToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = openfilename;
            if (saveFileDialog1.FileName == "") saveFileDialog1.FileName = "NewEffect.etd";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                SaveEffect(saveFileDialog1.FileName);
                Saved = true;
            }
        }

        private void SaveEffect(string filename)
        {
            canvasDock.EffectManager.Save(filename);
            var fn = Path.GetFileNameWithoutExtension(filename);
            string dir = Path.GetDirectoryName(filename) + "/" + fn;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            foreach (BaseEffect be in canvasDock.EffectManager.Effects)
            {
                if (be.Effects.Count != 0) continue;
                var resourcefname = Path.GetFileName(be.Filename);
                if (!File.Exists(dir + "/" + resourcefname)) File.Copy(be.Filename, dir + "/" + resourcefname, true);
            }
            openfilename = filename;
        }

        private void 元に戻すToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void やり直すToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void アニメーションToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var asc = new AskFrameCount();
            asc.SetLang(lang);
            if (asc.ShowDialog() == DialogResult.OK && MessageBox.Show(askanimation, confirm, MessageBoxButtons.YesNo) == DialogResult.Yes)
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

        private void 開くToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "ETD|*.etd";
            openFileDialog1.FileName = openfilename;
            openFileDialog1.Multiselect = false;
            if (CheckSave() != DialogResult.Cancel)
            {
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    OpenEffect(openFileDialog1.FileName);
                }
            }
        }

        private void OpenEffect(string effectfilename)
        {
            ResetEffect(EffectLoader.Load(effectfilename, (filename) =>
            {
                try
                {
                    var image = Image.FromFile(filename);
                    if (!canvasDock.ImagePool.ContainsKey(filename))
                    {
                        canvasDock.ImagePool.Add(filename, image);
                    }
                }
                catch
                {
                    MessageBox.Show("Failed to load image");
                }
            }));
            openfilename = effectfilename;


            if (this.最近使用したファイルToolStripMenuItem.DropDownItems.ContainsKey(effectfilename))
            {
                var tsmis = this.最近使用したファイルToolStripMenuItem.DropDownItems.Find(effectfilename, false);
                if (tsmis.Length == 1)
                {
                    this.最近使用したファイルToolStripMenuItem.DropDownItems.Remove(tsmis[0]);
                    this.最近使用したファイルToolStripMenuItem.DropDownItems.Insert(0, tsmis[0]);
                }
            }
            else
            {
                var tsmi = new ToolStripMenuItem(effectfilename)
                {
                    Name = effectfilename
                };
                tsmi.Click += tsmi_Click;
                this.最近使用したファイルToolStripMenuItem.DropDownItems.Insert(0, tsmi);
                if (this.最近使用したファイルToolStripMenuItem.DropDownItems.Count > 5)
                {
                    this.最近使用したファイルToolStripMenuItem.DropDownItems.RemoveAt(this.最近使用したファイルToolStripMenuItem.DropDownItems.Count - 1);
                }
            }
        }

        private void カンバスサイズToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var csf = new CanvasSizeForm();
            csf.SetLang(lang);
            csf.CanvasHeight = canvasDock.CanvasHeight;
            csf.CanvasWidth = canvasDock.CanvasWidth;
            if (csf.ShowDialog() == DialogResult.OK)
            {
                canvasDock.Initialize(Math.Max(1, csf.CanvasWidth), Math.Max(1, csf.CanvasHeight));
            }
        }

        private void RefreshState()
        {
            propertyDock.SetState(canvasDock.SelectedManager.State);
            CheckState();
        }

        private void カンバス背景変更ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var cbgf = new ChangeBackGroundForm();
            cbgf.SetLang(lang);
            cbgf.Mode = canvasDock.BackGroundMode;
            cbgf.Color = canvasDock.BackGroundColor;
            cbgf.Filename = canvasDock.BackImageFileName;
            if (cbgf.ShowDialog() == DialogResult.OK)
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

        private void 上書き保存するToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists(openfilename))
            {
                SaveEffect(openfilename);
                Saved = true;
            }
            else
            {
                保存するToolStripMenuItem_Click(this, e);
            }
        }

        private void 終了するToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CheckSave() == DialogResult.Cancel)
            {
                e.Cancel = true;
            }
            else
            {
                SaveConfig();
                SaveDock();
            }
        }

        private void SaveConfig()
        {
            if (!File.Exists(iniFileName))
            {
                File.Create(iniFileName).Close();
            }
            var sr = new StreamReader(iniFileName);
            var setting = new SettingReader(sr.ReadToEnd());
            sr.Close();
            setting.ReplaceOrAdd("Language", langFileISO);
            setting.ReplaceOrAdd("WindowWidth", Width);
            setting.ReplaceOrAdd("WindowHeight", Height);
            setting.ReplaceOrAdd("WindowState", (int)WindowState);
            setting.ReplaceOrAdd("CanvasWidth", canvasDock.CanvasWidth);
            setting.ReplaceOrAdd("CanvasHeight", canvasDock.CanvasHeight);
            setting.ReplaceOrAdd("CanvasBackGround", (int)canvasDock.BackGroundMode);
            setting.ReplaceOrAdd("CanvasBackGroundColor", ColorTranslator.ToHtml(canvasDock.BackGroundColor));
            setting.ReplaceOrAdd("CanvasBackGroundImage", canvasDock.BackImageFileName);
            setting.ReplaceOrAdd("Language", langFileISO);
            using (SettingWriter sw = new SettingWriter(iniFileName, false))
            {
                foreach (KeyValuePair<string, string> kvp in setting.Dictionary)
                {
                    sw.Write(kvp.Key, kvp.Value);
                }
            }
            using (StreamWriter sw = new StreamWriter(recentUsedFileName))
            {
                foreach (ToolStripMenuItem tsmi in this.最近使用したファイルToolStripMenuItem.DropDownItems)
                {
                    sw.WriteLine(tsmi.Text);
                }
            }
        }

        private void LoadConfig()
        {
            try
            {
                if (File.Exists(iniFileName))
                {
                    var sr = new StreamReader(iniFileName);
                    var setting = new SettingReader(sr.ReadToEnd());
                    sr.Close();
                    var width = 512;
                    var height = 512;
                    if (int.TryParse(setting["WindowWidth"], out int temp)) Width = temp;
                    if (int.TryParse(setting["WindowHeight"], out temp)) Height = temp;
                    if (int.TryParse(setting["WindowState"], out temp)) WindowState = (FormWindowState)temp;
                    if (int.TryParse(setting["CanvasWidth"], out temp))
                    {
                        width = temp;
                    }
                    if (int.TryParse(setting["CanvasHeight"], out temp))
                    {
                        height = temp;
                    }
                    canvasDock.Initialize(width, height);

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
                                canvasDock.BackImageFileName = setting["CanvasBackGround"];
                                break;
                        }
                        RefreshCanvas();
                    }
                }
                if (File.Exists(recentUsedFileName))
                {
                    using (StreamReader sr = new StreamReader(recentUsedFileName))
                    {
                        while (!sr.EndOfStream)
                        {
                            var path = sr.ReadLine();
                            if (!File.Exists(path)) continue;
                            var tsmi = new ToolStripMenuItem(path)
                            {
                                Name = path
                            };
                            this.最近使用したファイルToolStripMenuItem.DropDownItems.Add(tsmi);
                            tsmi.Click += tsmi_Click;
                        }
                    }
                }
            }
            catch
            {
            }
        }

        void tsmi_Click(object sender, EventArgs e)
        {
            if (CheckSave() != DialogResult.Cancel)
            {
                OpenEffect((sender as ToolStripMenuItem).Text);
            }
        }

        private DialogResult CheckSave()
        {
            bool cancel = false;
            if (!Saved)
            {
                var dr = MessageBox.Show(asksave, confirm, MessageBoxButtons.YesNoCancel);
                if (dr == DialogResult.Yes)
                {
                    saveFileDialog1.FileName = openfilename;
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        SaveEffect(saveFileDialog1.FileName);
                        Saved = true;
                    }
                    else
                    {
                        cancel = true;
                    }

                }
                else cancel |= dr == DialogResult.Cancel;
            }
            return cancel ? DialogResult.Cancel : DialogResult.OK;
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
            else e.Effect = DragDropEffects.None;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            var fileName = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (File.Exists(fileName[0]) && Path.GetExtension(fileName[0]) == ".etd")
            {
                if (CheckSave() != DialogResult.Cancel)
                {
                    OpenEffect(fileName[0]);
                }
            }
        }

        private void 新規作成ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CheckSave() != DialogResult.Cancel)
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

        private void キャンバスウィンドウToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.ShowOrHideWindow(dockPanel1, canvasDock, ModifierKeys);
        }

        private void プロパティウィンドウToolStripMenuItem_Click(object sender, EventArgs e)
        {

            Utility.ShowOrHideWindow(dockPanel1, propertyDock, ModifierKeys);
        }

        private void ヒストリーウィンドウToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.ShowOrHideWindow(dockPanel1, historyDock, ModifierKeys);
        }

        private void タイムラインウィンドウToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Utility.ShowOrHideWindow(dockPanel1, timeLineDock, ModifierKeys);
        }
    }
}
