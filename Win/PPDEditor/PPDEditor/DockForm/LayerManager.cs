using PPDEditor.Command.PPDSheet;
using PPDEditor.Controls;
using PPDEditor.Forms;
using PPDFramework.PPDStructure.PPDData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PPDEditor
{
    public partial class LayerManager : ScrollableForm
    {
        string BPMtext = "BPM";
        string BPMOffsettext = "BPM offset";
        string Displaywidthtext = "タイムラインの幅";
        string layer = "レイヤー";
        string copy = " のコピー";
        LayerDisplay lastld;
        float lastBPM;
        float lastBPMOffset;
        int lastDisplayWidth;
        DisplayLineMode lastDisplayMode;
        DisplayBeatType lastBeatType;

        bool mousedown;
        Point mousedownpos;
        TransParentForm tpf;
        BlackBar bb;
        int movecount;

        List<LayerDisplay> layers;

        IDProvider idProvider;

        public LayerManager()
        {
            InitializeComponent();

            idProvider = new IDProvider(0);
            layers = new List<LayerDisplay>();
        }
        public void Initialize()
        {
            var ld = CreateLayerDisplay(layer + 0);
            layers.Add(ld);
            ld.Selected = true;
            Relocate();
        }
        public void SetLang()
        {
            BPMtext = Utility.Language["LMBPM"];
            BPMOffsettext = Utility.Language["LMBPMOffset"];
            Displaywidthtext = Utility.Language["LMDisplayWidth"];
            layer = Utility.Language["LMLayer"];
            copy = Utility.Language["LMCopy"];
            this.新規レイヤーToolStripMenuItem.Text = Utility.Language["LMMenu1"];
        }
        private void 新規レイヤーToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fm6 = new NewLayerForm();
            fm6.SetLang();
            fm6.LayerName = layer + layers.Count;
            if (fm6.ShowDialog() == DialogResult.OK)
            {
                var ld = CreateLayerDisplay(fm6.LayerName);
                AddLayerToPanel(ld);
                ContentChanged();
            }
        }

        private void Relocate()
        {
            Controls.Clear();
            int nextY = 0;
            for (int i = layers.Count - 1; i >= 0; i--)
            {
                layers[i].Location = new Point(0, nextY);
                Controls.Add(layers[i]);
                nextY += layers[i].Height;
            }
        }

        private void AddLayerToPanel(LayerDisplay ld)
        {
            layers.Add(ld);
            Relocate();
        }

        private LayerDisplay CreateLayerDisplay(string layername)
        {
            return CreateLayerDisplay(layername,
                WindowUtility.MainForm.BPM,
                WindowUtility.MainForm.BPMOffset,
                WindowUtility.MainForm.DisplayWidth, DisplayLineMode.Fourth, DisplayBeatType.Fourth);
        }

        private LayerDisplay CreateLayerDisplay(string layername, float bpm, float bpmoffset, int displaywidth, DisplayLineMode displayMode, DisplayBeatType beatType)
        {
            var ld = new LayerDisplay();
            ld.SetLang();
            ld.DisplayName = layername;
            ld.BPM = BPMtext + " " + bpm;
            ld.PPDData.BPM = bpm;
            ld.PPDData.BPMStart = bpmoffset;
            ld.PPDData.DisplayWidth = displaywidth;
            ld.PPDData.DisplayName = layername;
            ld.PPDData.DisplayMode = displayMode;
            ld.PPDData.BeatType = beatType;
            ld.MouseDown += ld_MouseDown;
            ld.MouseMove += ld_MouseMove;
            ld.MouseUp += ld_MouseUp;
            ld.SelectStateChanged += ld_SelectStateChanged;
            ld.Deleted += ld_Deleted;
            ld.Duplicated += ld_Duplicated;
            ld.VisibleStateChanged += ld_VisibleStateChanged;
            ld.PPDData.DisplayDataChanged += PPDData_DisplayDataChanged;
            ld.PPDData.CommandChanged += PPDData_CommandChanged;
            return ld;
        }

        void PPDData_CommandChanged(object sender, EventArgs e)
        {
            if (WindowUtility.TimeLineForm != null)
            {
                WindowUtility.TimeLineForm.ContentChanged();
            }
        }

        void ld_MouseUp(object sender, MouseEventArgs e)
        {
            if (mousedown && tpf != null)
            {
                int index = (SelectedLayer.Location.Y + e.Y + VerticalScroll.Value) / SelectedLayer.Height;
                if (index < 0)
                {
                    index = 0;
                }
                if (index > layers.Count)
                {
                    index = layers.Count;
                }
                index = layers.Count - index;
                var selectedIndex = layers.IndexOf(SelectedLayer);
                var layer = SelectedLayer;
                if (selectedIndex < index)
                {
                    index--;
                }
                if (selectedIndex != index)
                {
                    layers.RemoveAt(selectedIndex);
                    layers.Insert(index, layer);
                    Relocate();
                }
            }
            mousedown = false;
            if (tpf != null)
            {
                tpf.Close();
                tpf.Dispose();
                tpf = null;
            }
            if (bb != null)
            {
                this.Controls.Remove(bb);
                bb.Dispose();
                bb = null;
            }
        }
        void ld_MouseMove(object sender, MouseEventArgs e)
        {
            if (mousedown)
            {
                if (tpf == null)
                {
                    if (Math.Abs(mousedownpos.X - e.X) >= 5 || Math.Abs(mousedownpos.Y - e.Y) >= 5)
                    {
                        tpf = new TransParentForm();
                        tpf.Show();
                        tpf.SetInfo(SelectedLayer, SelectedLayer.Size);
                        tpf.Location = this.PointToScreen(new Point(SelectedLayer.Location.X, SelectedLayer.Location.Y));
                        bb = new BlackBar();
                        this.Controls.Add(bb);
                        this.Controls.SetChildIndex(bb, 0);
                        mousedownpos = new Point(Cursor.Position.X, Cursor.Position.Y);
                        movecount = 0;
                    }
                }
                else
                {
                    Point p = Cursor.Position;
                    tpf.Location = new Point(tpf.Location.X + p.X - mousedownpos.X, tpf.Location.Y + p.Y - mousedownpos.Y);
                    mousedownpos = new Point(p.X, p.Y);
                    int index = (SelectedLayer.Location.Y + e.Y + VerticalScroll.Value) / SelectedLayer.Height;
                    if (index < 0)
                    {
                        index = 0;
                    }
                    if (index > layers.Count)
                    {
                        index = layers.Count;
                    }
                    bb.Location = new Point(0, SelectedLayer.Height * index - VerticalScroll.Value);
                    if (movecount % 10 == 0)
                    {
                        if (bb.Location.Y < 0)
                        {
                            GainVScroll(-SelectedLayer.Width);
                        }
                        else if (bb.Location.Y > this.ClientSize.Height)
                        {
                            GainVScroll(SelectedLayer.Width);
                        }
                    }
                    bb.Location = new Point(0, SelectedLayer.Height * index - VerticalScroll.Value);
                    movecount++;
                }
            }
        }

        private void GainVScroll(int gain)
        {
            var value = VerticalScroll.Value + gain;
            if (value < VerticalScroll.Minimum)
            {
                value = VerticalScroll.Minimum;
            }
            if (value > VerticalScroll.Maximum)
            {
                value = VerticalScroll.Maximum;
            }
            VerticalScroll.Value = value;
        }

        void ld_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mousedown = true;
                mousedownpos = new Point(e.X, e.Y);
            }
        }
        void PPDData_DisplayDataChanged(object sender, EventArgs e)
        {
            ChangeDisplayData();
        }
        private void ChangeDisplayData()
        {
            LayerDisplay sld = SelectedLayer;
            if (sld != null && (sld != lastld || sld.PPDData.BPM != lastBPM || sld.PPDData.BPMStart != lastBPMOffset
                || sld.PPDData.DisplayWidth != lastDisplayWidth || sld.PPDData.BeatType != lastBeatType))
            {
                WindowUtility.Seekmain.SetSelectedSheetInfo(sld.PPDData.BPM, sld.PPDData.BPMStart, sld.PPDData.DisplayWidth,
                    sld.PPDData.DisplayMode, sld.PPDData.BeatType);
                lastld = sld;
                lastBPM = sld.PPDData.BPM;
                lastBPMOffset = sld.PPDData.BPMStart;
                lastDisplayWidth = sld.PPDData.DisplayWidth;
                lastDisplayMode = sld.PPDData.DisplayMode;
                lastBeatType = sld.PPDData.BeatType;
                WindowUtility.MainForm.BPM = lastBPM;
                WindowUtility.MainForm.BPMOffset = lastBPMOffset;
                WindowUtility.MainForm.DisplayWidth = lastDisplayWidth;
                WindowUtility.MainForm.DisplayMode = lastDisplayMode;
                WindowUtility.MainForm.BeatType = lastBeatType;
            }
        }
        void ld_VisibleStateChanged(object sender, EventArgs e)
        {
            WindowUtility.Seekmain.DrawAndRefresh();
        }

        void ld_Duplicated(object sender, EventArgs e)
        {
            if (sender is LayerDisplay senderld)
            {
                var index = layers.IndexOf(senderld);
                var ld = CreateLayerDisplay(senderld.DisplayName + copy);
                layers.Insert(index + 1, ld);
                Relocate();
                WindowUtility.Seekmain.DrawAndRefresh();
                SortedList<float, Mark>[] source = senderld.PPDData.Data;
                for (int i = 0; i < source.Length; i++)
                {
                    foreach (Mark mk in source[i].Values)
                    {
                        if (mk is ExMark exmk)
                        {
                            ld.PPDData.AddExMark(exmk.Time, exmk.EndTime, exmk.Position.X, exmk.Position.Y, exmk.Rotation, (int)exmk.Type, exmk.ID, exmk.Parameters.ToArray());
                        }
                        else
                        {
                            ld.PPDData.AddMark(mk.Time, mk.Position.X, mk.Position.Y, mk.Rotation, (int)mk.Type, mk.ID, mk.Parameters.ToArray());
                        }
                    }
                }
                ld.PPDData.BPM = senderld.PPDData.BPM;
                ld.PPDData.BPMStart = senderld.PPDData.BPMStart;
                ld.PPDData.DisplayWidth = senderld.PPDData.DisplayWidth;
                ld.PPDData.DisplayMode = senderld.PPDData.DisplayMode;
                ld.PPDData.BeatType = senderld.PPDData.BeatType;
                ld.BPM = senderld.BPM;
                ld.MarkColor = senderld.MarkColor;
                ContentChanged();
            }
        }

        void ld_Deleted(object sender, EventArgs e)
        {
            var senderld = sender as LayerDisplay;
            var index = layers.IndexOf(senderld);
            layers.Remove(senderld);
            Relocate();
            index--;
            if (index >= layers.Count) index = layers.Count - 1;
            if (index < 0) index = 0;
            if (layers.Count != 0)
            {
                LayerDisplay ld = layers[index];
                if (ld != null)
                {
                    ld.SelectedWithEvent = true;
                }
            }
            WindowUtility.Seekmain.DrawAndRefresh();
            ContentChanged();
        }

        void ld_SelectStateChanged(object sender, EventArgs e)
        {
            var senderld = sender as LayerDisplay;
            foreach (LayerDisplay layer in layers)
            {
                if (senderld != layer)
                {
                    layer.Selected = false;
                }
            }
            ChangeDisplayData();
        }
        private LayerDisplay SelectedLayer
        {
            get
            {
                return layers.FirstOrDefault(l => l.Selected);
            }
        }
        public void Clear()
        {
            idProvider = new IDProvider(0);
            layers.Clear();
        }
        public MarkData[] GetAllData(bool any)
        {
            SortedList<float, MarkData>[] dest = new SortedList<float, MarkData>[10];
            for (int i = 0; i < dest.Length; i++)
            {
                dest[i] = new SortedList<float, MarkData>();
            }

            foreach (var layer in layers)
            {
                if (layer.DisplayVisible || any)
                {
                    SortedList<float, Mark>[] source = layer.PPDData.Data;
                    for (int j = 0; j < source.Length; j++)
                    {
                        foreach (Mark mk in source[j].Values)
                        {
                            var exmk = mk as ExMark;
                            if (!dest[j].ContainsKey(mk.Time))
                            {
                                if (exmk != null)
                                {
                                    dest[j].Add(exmk.Time, exmk.Convert());
                                }
                                else
                                {
                                    dest[j].Add(mk.Time, mk.Convert());
                                }
                            }
                        }
                    }
                }
            }
            return Utility.GetSortedData<MarkData>(dest);
        }

        public void AddLayer(string name, float bpm, float bpmoffset, int displaywidth, bool visible, DisplayLineMode displayMode, DisplayBeatType beatType)
        {
            var ld = CreateLayerDisplay(name, bpm, bpmoffset, displaywidth, displayMode, beatType);
            ld.DisplayVisible = visible;
            AddLayerToPanel(ld);
        }

        public void SelectLastLayer()
        {
            if (layers.Count > 0)
            {
                layers[layers.Count - 1].SelectedWithEvent = true;
            }
        }

        public void SelectLayerByIndex(int index)
        {
            if (index < 0 || index >= layers.Count)
            {
                return;
            }
            layers[index].SelectedWithEvent = true;
        }

        public void CalculateMaxID()
        {
            uint max = 0;
            foreach (MarkData markData in GetAllData(true))
            {
                if (max < markData.ID)
                {
                    max = markData.ID;
                }
            }
            idProvider = new IDProvider(max);
        }
        public IDProvider IDProvider
        {
            get
            {
                return idProvider;
            }
        }
        public PPDSheet SelectedPpdSheet
        {
            get
            {
                LayerDisplay sld = SelectedLayer;
                if (sld != null)
                {
                    return sld.PPDData;
                }
                return null;
            }
        }
        public int SelectedLayerIndex
        {
            get
            {
                LayerDisplay sld = SelectedLayer;
                if (sld == null)
                {
                    return -1;
                }
                return layers.IndexOf(sld);
            }
        }
        public PPDSheet[] Sheets
        {
            get
            {
                return layers.Where(l => l.DisplayVisible).Select(l => l.PPDData).ToArray();
            }
        }
        public LayerDisplay[] AllLayerDisplay
        {
            get
            {
                return layers.ToArray();
            }
        }
        public float BPM
        {
            set
            {
                LayerDisplay sld = SelectedLayer;
                if (sld != null)
                {
                    sld.BPM = BPMtext + " " + value;
                    sld.PPDData.BPM = value;
                }
            }
        }
        public float BPMOffset
        {
            set
            {
                LayerDisplay sld = SelectedLayer;
                if (sld != null)
                {
                    sld.PPDData.BPMStart = value;
                }
            }
        }
        public int DisplayWidth
        {
            set
            {
                LayerDisplay sld = SelectedLayer;
                if (sld != null)
                {
                    sld.PPDData.DisplayWidth = value;
                }
            }
        }
        public DisplayLineMode DisplayMode
        {
            set
            {
                LayerDisplay sld = SelectedLayer;
                if (sld != null)
                {
                    sld.PPDData.DisplayMode = value;
                }
            }
        }
        public DisplayBeatType BeatType
        {
            set
            {
                LayerDisplay sld = SelectedLayer;
                if (sld != null)
                {
                    sld.PPDData.BeatType = value;
                }
            }
        }
    }
}
