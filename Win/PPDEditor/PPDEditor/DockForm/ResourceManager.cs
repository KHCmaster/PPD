using Effect2D;
using PPDEditor.Controls.Resource;
using PPDEditor.Forms;
using PPDFramework.PPDStructure;
using PPDFramework.PPDStructure.PPDData;
using PPDSound;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace PPDEditor
{
    public partial class ResourceManager : ChangableDockContent
    {
        [Flags]
        enum Filter
        {
            None = 0,
            Enabled = 1,
            Disabled = 2
        }

        enum NodeType
        {
            Image,
            Effect,
            Sound,
            Others
        }

        const string ResourcePath = "Resource";
        const string ImageDirPath = "Resource\\Image";
        const string EffectDirPath = "Resource\\Effect";
        const string SoundDirPath = "Resource\\Sound";
        const string OthersDirPath = "Resource\\Others";

        string[] allowImageExtensions = { ".jpg", ".jpeg", ".gif", ".png" };
        string[] allowSoundExtensions = { ".wav", ".ogg" };

        private Sound sound;

        public ResourceManager()
        {
            InitializeComponent();
            Initialize();
        }

        public void SetLang()
        {
            this.リソースを追加ToolStripMenuItem.Text = Utility.Language["AddResource"];
            this.imageを追加ToolStripMenuItem.Text = Utility.Language["AddImage"];
            this.effectを追加ToolStripMenuItem.Text = Utility.Language["AddEffect"];
            this.soundを追加ToolStripMenuItem.Text = Utility.Language["AddSound"];
            this.その他を追加ToolStripMenuItem.Text = Utility.Language["AddOthers"];
            this.削除ToolStripMenuItem.Text = Utility.Language["Delete"];
            this.無効化ToolStripMenuItem.Text = Utility.Language["MakeInvalid"];
        }

        public void SetSound(Sound sound)
        {
            this.sound = sound;
        }

        public void CreateResourceFolders()
        {
            var projectDir = WindowUtility.MainForm.CurrentProjectDir;
            Utility.RecursiveCreateFolder(Path.Combine(projectDir, ImageDirPath));
            Utility.RecursiveCreateFolder(Path.Combine(projectDir, EffectDirPath));
            Utility.RecursiveCreateFolder(Path.Combine(projectDir, SoundDirPath));
            Utility.RecursiveCreateFolder(Path.Combine(projectDir, OthersDirPath));
        }

        private void Initialize()
        {
            treeView1.ImageList = new System.Windows.Forms.ImageList
            {
                ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit
            };

            treeView1.ImageList.Images.Add("folder", PPDEditor.Properties.Resources.folder);
            treeView1.ImageList.Images.Add("disabled", PPDEditor.Properties.Resources.disabled);
            treeView1.ImageList.Images.Add("image", PPDEditor.Properties.Resources.image);
            treeView1.ImageList.Images.Add("effect", PPDEditor.Properties.Resources.effect);
            treeView1.ImageList.Images.Add("sound", PPDEditor.Properties.Resources.sound);
            treeView1.ImageList.Images.Add("others", PPDEditor.Properties.Resources.document);

            Win32.ImageList_SetOverlayImage(treeView1.ImageList.Handle, 1, 1);

            CreateFolderNode(treeView1.Nodes, "Image");
            CreateFolderNode(treeView1.Nodes, "Effect");
            CreateFolderNode(treeView1.Nodes, "Sound");
            CreateFolderNode(treeView1.Nodes, "Others");
        }

        public TreeNode ImageNode
        {
            get
            {
                return treeView1.Nodes[0];
            }
        }

        public TreeNode EffectNode
        {
            get
            {
                return treeView1.Nodes[1];
            }
        }

        public TreeNode SoundNode
        {
            get
            {
                return treeView1.Nodes[2];
            }
        }

        public TreeNode OthersNode
        {
            get
            {
                return treeView1.Nodes[3];
            }
        }

        public void Clear()
        {
            ImageNode.Nodes.Clear();
            EffectNode.Nodes.Clear();
            SoundNode.Nodes.Clear();
            OthersNode.Nodes.Clear();
            dataGridView1.RowCount = 0;
        }

        public void ReadResource(string projectDirPath, string disableList)
        {
            var dir = Path.Combine(projectDirPath, ImageDirPath);
            Utility.RecursiveCreateFolder(dir);
            foreach (string filePath in Directory.GetFiles(dir))
            {
                var extension = Path.GetExtension(filePath);
                if (Array.IndexOf(allowImageExtensions, extension.ToLower()) >= 0)
                {
                    CreateNode(ImageNode.Nodes, Path.GetFileName(filePath), NodeType.Image);
                }
            }

            dir = Path.Combine(projectDirPath, EffectDirPath);
            Utility.RecursiveCreateFolder(dir);
            foreach (string filePath in Directory.GetFiles(dir))
            {
                var extension = Path.GetExtension(filePath);
                if (extension.ToLower() == ".etd")
                {
                    CreateNode(EffectNode.Nodes, Path.GetFileName(filePath), NodeType.Effect);
                }
            }

            dir = Path.Combine(projectDirPath, SoundDirPath);
            Utility.RecursiveCreateFolder(dir);
            foreach (string filePath in Directory.GetFiles(dir))
            {
                var extension = Path.GetExtension(filePath);
                if (Array.IndexOf(allowSoundExtensions, extension.ToLower()) >= 0)
                {
                    CreateNode(SoundNode.Nodes, Path.GetFileName(filePath), NodeType.Sound);
                }
            }

            dir = Path.Combine(projectDirPath, OthersDirPath);
            Utility.RecursiveCreateFolder(dir);
            foreach (string filePath in Directory.GetFiles(dir))
            {
                var extension = Path.GetExtension(filePath);
                CreateNode(OthersNode.Nodes, Path.GetFileName(filePath), NodeType.Others);
            }

            var disables = disableList.Split('|');
            foreach (string disable in disables)
            {
                var node = Utility.GetTreeNodeFromPath(treeView1.Nodes, disable);
                if (node != null)
                {
                    Win32.SetTreeViewOverlay(node, 1);
                }
            }
        }

        public string GetDisableList()
        {
            var list = GetList(Filter.Disabled);
            return String.Join("|", list);
        }

        public string[] GetEnabledList()
        {
            return GetList(Filter.Enabled);
        }

        public string[] GetEffectList()
        {
            return GetChildrenList(EffectNode);
        }

        public string[] GetImageList()
        {
            return GetChildrenList(ImageNode);
        }

        public string[] GetSoundList()
        {
            return GetChildrenList(SoundNode);
        }

        public string[] GetOthersList()
        {
            return GetChildrenList(OthersNode);
        }

        private string[] GetChildrenList(TreeNode node)
        {
            var ret = new List<string>();
            foreach (TreeNode childNode in node.Nodes)
            {
                ret.Add(childNode.Text);
            }
            return ret.ToArray();
        }

        private string[] GetList(Filter filter)
        {
            var ret = new List<string>();
            var nodes = new Queue<TreeNode>();
            nodes.Enqueue(ImageNode);
            nodes.Enqueue(EffectNode);
            nodes.Enqueue(SoundNode);
            nodes.Enqueue(OthersNode);
            while (nodes.Count > 0)
            {
                var node = nodes.Dequeue();
                foreach (TreeNode childNode in node.Nodes)
                {
                    if (childNode.Name == "folder")
                    {
                        nodes.Enqueue(childNode);
                    }
                    else
                    {
                        var enabled = EnabledNode(childNode);
                        if (enabled)
                        {
                            if ((filter & Filter.Enabled) == Filter.Enabled)
                            {
                                ret.Add(childNode.FullPath);
                                ret.AddRange(GetListEffect(childNode));
                            }
                        }
                        else
                        {
                            if ((filter & Filter.Disabled) == Filter.Disabled)
                            {
                                ret.Add(childNode.FullPath);
                                ret.AddRange(GetListEffect(childNode));
                            }
                        }
                    }
                }
            }

            return ret.ToArray();
        }

        private string[] GetListEffect(TreeNode node)
        {
            var ret = new List<string>();
            var dir = Path.Combine(Path.Combine(WindowUtility.MainForm.CurrentProjectDir, EffectDirPath), Path.GetFileNameWithoutExtension(node.Text));
            if (node.Parent == EffectNode)
            {
                foreach (string filePath in Directory.GetFiles(dir))
                {
                    if (Array.IndexOf(allowImageExtensions, Path.GetExtension(filePath)) >= 0)
                    {
                        ret.Add(String.Format("Effect\\{0}\\{1}", Path.GetFileNameWithoutExtension(node.Text), Path.GetFileName(filePath)));
                    }
                }
            }

            return ret.ToArray();
        }

        public void WriteResource(Stream stream, string fileName)
        {
            var path = Path.Combine(Path.Combine(WindowUtility.MainForm.CurrentProjectDir, ResourcePath), fileName);

            using (FileStream fs = File.Open(path, FileMode.Open))
            {
                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                stream.Write(data, 0, data.Length);
            }
        }

        private void imageを追加ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = null;
            openFileDialog1.Filter = "Image(*.jpg;*.jpeg;*.gif;*.png)|*.jpg;*.jpeg;*.gif;*.png";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (string filePath in openFileDialog1.FileNames)
                {
                    AddImage(filePath);
                }
                ContentChanged();
            }
        }

        private void AddImage(string filePath)
        {
            var copyPath = Path.Combine(Path.Combine(WindowUtility.MainForm.CurrentProjectDir, ImageDirPath), Path.GetFileName(filePath));
            if (File.Exists(copyPath))
            {
                MessageBox.Show(String.Format("{0}\n{1}", Utility.Language["AlreadyExistResource"], filePath));
                return;
            }

            CheckResourceDir();
            File.Copy(filePath, copyPath, true);

            CreateNode(ImageNode.Nodes, Path.GetFileName(filePath), NodeType.Image);
        }

        private void effectを追加ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = null;
            openFileDialog1.Filter = "Effect(*.etd)|*.etd";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (string filePath in openFileDialog1.FileNames)
                {
                    AddEffect(filePath);
                }
                ContentChanged();
            }
        }

        private void AddEffect(string filePath)
        {
            var copyPath = Path.Combine(Path.Combine(WindowUtility.MainForm.CurrentProjectDir, EffectDirPath), Path.GetFileName(filePath));
            if (File.Exists(copyPath))
            {
                MessageBox.Show(String.Format("{0}\n{1}", Utility.Language["AlreadyExistResource"], filePath));
                return;
            }

            CheckResourceDir();
            File.Copy(filePath, copyPath, true);
            foreach (string imagePath in Directory.GetFiles(Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath))))
            {
                var newPath = Path.Combine(Path.Combine(Path.GetDirectoryName(copyPath), Path.GetFileNameWithoutExtension(filePath)), Path.GetFileName(imagePath));
                Utility.RecursiveCreateFolder(newPath);
                File.Copy(imagePath, newPath, true);
            }

            CreateNode(EffectNode.Nodes, Path.GetFileName(filePath), NodeType.Effect);

            var dir = Path.GetDirectoryName(filePath);
            foreach (string fileName in EffectLoader.GetEffectReference(filePath))
            {
                var path = Path.Combine(dir, fileName);
                if (File.Exists(path))
                {
                    AddEffect(path);
                }
            }
        }

        private void soundを追加ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = null;
            openFileDialog1.Filter = Utility.Language["SMWaveFilter"];
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (string filePath in openFileDialog1.FileNames)
                {
                    AddSound(filePath);
                }
                ContentChanged();
            }
        }

        private void AddSound(string filePath)
        {
            var copyPath = Path.Combine(Path.Combine(WindowUtility.MainForm.CurrentProjectDir, SoundDirPath), Path.GetFileName(filePath));
            if (File.Exists(copyPath))
            {
                MessageBox.Show(String.Format("{0}\n{1}", Utility.Language["AlreadyExistResource"], filePath));
                return;
            }

            CheckResourceDir();
            File.Copy(filePath, copyPath, true);

            CreateNode(SoundNode.Nodes, Path.GetFileName(filePath), NodeType.Sound);
        }

        private void その他を追加ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = null;
            openFileDialog1.Filter = Utility.Language["MovieFilter"];
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (string filePath in openFileDialog1.FileNames)
                {
                    AddOthers(filePath);
                }
                ContentChanged();
            }
        }

        private void AddOthers(string filePath)
        {
            var copyPath = Path.Combine(Path.Combine(WindowUtility.MainForm.CurrentProjectDir, OthersDirPath), Path.GetFileName(filePath));
            if (File.Exists(copyPath))
            {
                MessageBox.Show(String.Format("{0}\n{1}", Utility.Language["AlreadyExistResource"], filePath));
                return;
            }

            CheckResourceDir();
            File.Copy(filePath, copyPath, true);

            CreateNode(OthersNode.Nodes, Path.GetFileName(filePath), NodeType.Others);
        }

        private void CheckResourceDir()
        {
            var dir = Path.Combine(WindowUtility.MainForm.CurrentProjectDir, "resource");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var contents = new string[] { "image", "effect", "sound", "others" };
            foreach (string content in contents)
            {
                var path = Path.Combine(dir, content);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
        }

        private void リソースを追加ToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            imageを追加ToolStripMenuItem.Enabled = effectを追加ToolStripMenuItem.Enabled = soundを追加ToolStripMenuItem.Enabled = その他を追加ToolStripMenuItem.Enabled = WindowUtility.MainForm.IsProjectLoaded;
        }

        private void treeView1_NodeMouseClick(object sender, System.Windows.Forms.TreeNodeMouseClickEventArgs e)
        {
            treeView1.SelectedNode = e.Node;
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            無効化ToolStripMenuItem.Text = Win32.GetTreeViewOverlay(treeView1.SelectedNode) == 0 ? Utility.Language["MakeInvalid"] : Utility.Language["MakeValid"];
        }

        private bool EnabledNode(TreeNode node)
        {
            return Win32.GetTreeViewOverlay(node) == 0;
        }

        private void 削除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(String.Format(Utility.Language["DeleteConfirmText"], treeView1.SelectedNode.FullPath), Utility.Language["DeleteConfirm"], MessageBoxButtons.OKCancel) != DialogResult.OK)
            {
                return;
            }

            var path = Path.Combine(WindowUtility.MainForm.CurrentProjectDir, Path.Combine("Resource", treeView1.SelectedNode.FullPath));
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            if (Path.GetExtension(path).ToLower() == ".etd")
            {
                var dir = Path.Combine(WindowUtility.MainForm.CurrentProjectDir, Path.Combine(Path.GetDirectoryName(path), Path.GetFileName(path)));
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
            }

            treeView1.SelectedNode.Remove();

            ContentChanged();
        }

        private void 無効化ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                Win32.SetTreeViewOverlay(treeView1.SelectedNode, EnabledNode(treeView1.SelectedNode) ? (uint)1 : 0);
                ContentChanged();
            }
        }

        private TreeNode CreateFolderNode(TreeNodeCollection nodes, string name)
        {
            var node = nodes.Add("folder", name);
            node.ImageKey = "folder";
            node.SelectedImageKey = "folder";
            return node;
        }

        private TreeNode CreateNode(TreeNodeCollection nodes, string name, NodeType nodeType)
        {
            string key = "";
            switch (nodeType)
            {
                case NodeType.Effect:
                    key = "effect";
                    break;
                case NodeType.Image:
                    key = "image";
                    break;
                case NodeType.Sound:
                    key = "sound";
                    break;
                case NodeType.Others:
                    key = "others";
                    break;
            }

            var node = nodes.Add(key, name);
            node.ImageKey = key;
            node.SelectedImageKey = key;
            node.ContextMenuStrip = contextMenuStrip1;
            return node;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ChangeDataGrid();
        }

        private void ChangeDataGrid()
        {
            dataGridView1.RowCount = 0;

            if (treeView1.SelectedNode == null || !WindowUtility.MainForm.IsProjectLoaded)
            {
                return;
            }

            switch (treeView1.SelectedNode.ImageKey)
            {
                case "folder":
                    AddFolderInfo();
                    break;
                case "image":
                    AddImageInfo();
                    break;
                case "effect":
                    AddEffectInfo();
                    break;
                case "sound":
                    AddSoundInfo();
                    break;
                case "others":
                    AddOthersInfo();
                    break;
            }
        }

        private void AddFolderInfo()
        {
        }

        private void AddImageInfo()
        {
            var path = Path.Combine(WindowUtility.MainForm.CurrentProjectDir, Path.Combine("Resource", treeView1.SelectedNode.FullPath));
            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                using (Image image = Image.FromFile(path))
                {
                    AddInfo(Utility.Language["ImageKind"], GetImageFormatString(image.RawFormat));
                    AddInfo(Utility.Language["ImageWidth"], image.Width);
                    AddInfo(Utility.Language["ImageHeight"], image.Height);
                    AddInfo(Utility.Language["ImageHorizontalDPI"], image.HorizontalResolution);
                    AddInfo(Utility.Language["ImageVerticalDPI"], image.VerticalResolution);
                    foreach (PropertyItem propertyItem in image.PropertyItems)
                    {
                        string val = "";
                        switch (propertyItem.Type)
                        {
                            case 1:
                                break;
                            case 2:
                                val = System.Text.Encoding.ASCII.GetString(propertyItem.Value);
                                val = val.Trim(new char[] { '\0' });
                                break;
                            case 3:
                                var uint16Value = BitConverter.ToUInt16(propertyItem.Value, 0);
                                val = uint16Value.ToString();
                                break;
                            case 4:
                                var uint32Value = BitConverter.ToUInt32(propertyItem.Value, 0);
                                val = uint32Value.ToString();
                                break;
                        }
                        AddInfo(propertyItem.Id, val);
                    }
                }
                var imagePanel = new ImagePanel();
                imagePanel.OpenFile(path);
                ChangePanel(imagePanel);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void AddInfo(params object[] args)
        {
            this.dataGridView1.Rows.Add(args);
        }

        private string GetImageFormatString(ImageFormat format)
        {
            if (format.Guid == ImageFormat.Bmp.Guid)
            {
                return "BMP";
            }
            else if (format.Guid == ImageFormat.Emf.Guid)
            {
                return "EMF";
            }
            else if (format.Guid == ImageFormat.Exif.Guid)
            {
                return "EXIF";
            }
            else if (format.Guid == ImageFormat.Gif.Guid)
            {
                return "GIF";
            }
            else if (format.Guid == ImageFormat.Icon.Guid)
            {
                return "ICON";
            }
            else if (format.Guid == ImageFormat.Jpeg.Guid)
            {
                return "JPEG";
            }
            else if (format.Guid == ImageFormat.MemoryBmp.Guid)
            {
                return "MEMORYBMP";
            }
            else if (format.Guid == ImageFormat.Png.Guid)
            {
                return "PNG";
            }
            else if (format.Guid == ImageFormat.Tiff.Guid)
            {
                return "TIFF";
            }
            else if (format.Guid == ImageFormat.Wmf.Guid)
            {
                return "WMF";
            }

            return "";
        }

        private void AddEffectInfo()
        {
            var path = Path.Combine(WindowUtility.MainForm.CurrentProjectDir, Path.Combine("Resource", treeView1.SelectedNode.FullPath));
            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                var refs = EffectLoader.GetEffectReference(path);
                var manager = EffectLoader.Load(path, false, (fn) =>
                {
                });
                AddInfo(Utility.Language["ReferenceEffect"], String.Join(", ", refs));
                AddInfo(Utility.Language["FrameCount"], manager.FrameLength);
                AddInfo(Utility.Language["StartFrame"], manager.StartFrame);
                int layerCount = 0;
                var effects = new Queue<IEffect>();
                effects.Enqueue(manager);
                while (effects.Count > 0)
                {
                    var effect = effects.Dequeue();
                    if (effect.Effects.Count == 0)
                    {
                        layerCount++;
                    }
                    else
                    {
                        foreach (IEffect childEffect in effect.Effects)
                        {
                            effects.Enqueue(childEffect);
                        }
                    }
                }
                AddInfo(Utility.Language["LayerCount"], layerCount);
                AddInfo(Utility.Language["FPS"], manager.FPS);

                var effectPanel = new EffectPanel();
                effectPanel.SetLang();
                effectPanel.OpenFile(path);
                ChangePanel(effectPanel);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void AddSoundInfo()
        {
            var path = Path.Combine(WindowUtility.MainForm.CurrentProjectDir, Path.Combine("Resource", treeView1.SelectedNode.FullPath));
            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                FMODEX.SOUND_TYPE soundType = FMODEX.SOUND_TYPE.AIFF;
                FMODEX.SOUND_FORMAT soundFormat = FMODEX.SOUND_FORMAT.AT9;
                sound.GetSoundInfo(path, out soundType, out soundFormat, out int channelCount, out int bits, out float length);
                AddInfo(Utility.Language["SoundLength"], length);
                AddInfo(Utility.Language["ChannelCount"], channelCount);
                AddInfo("", soundType);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void AddOthersInfo()
        {
            var path = Path.Combine(WindowUtility.MainForm.CurrentProjectDir, Path.Combine("Resource", treeView1.SelectedNode.FullPath));
            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                using (FileStream fs = File.Open(path, FileMode.Open))
                {
                    bool b = false;
                    var ppdData = PPDReader.Read(fs, ref b);
                    if (!b)
                    {
                        int normalCount = 0, longCount = 0;
                        float startTime = float.MaxValue, endTime = float.MinValue;
                        foreach (MarkDataBase data in ppdData)
                        {
                            if (startTime > data.Time)
                            {
                                startTime = data.Time;
                            }

                            if (endTime < data.Time)
                            {
                                endTime = data.Time;
                            }

                            if (data is ExMarkData)
                            {
                                longCount++;
                                if (endTime < (data as ExMarkData).EndTime)
                                {
                                    endTime = (data as ExMarkData).EndTime;
                                }
                            }
                            else if (data is MarkData)
                            {
                                normalCount++;
                            }
                        }

                        if (ppdData.Length == 0)
                        {
                            startTime = endTime = 0;
                        }

                        AddInfo("通常マークの数", normalCount);
                        AddInfo("長押しマークの数", longCount);
                        AddInfo("最初のマークの時間", startTime);
                        AddInfo("最後のマークの時間", endTime);
                    }
                    else
                    {
                        AddInfo("サイズ", fs.Length);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void ChangePanel(Control control)
        {
            this.splitContainer2.Panel1.Controls.Clear();
            this.splitContainer2.Panel1.Controls.Add(control);
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                if (treeView1.SelectedNode != null)
                {
                    Clipboard.SetText(treeView1.SelectedNode.Text);
                }
                e.Handled = true;
            }
        }
    }
}
