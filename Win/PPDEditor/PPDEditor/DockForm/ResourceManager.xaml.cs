using Effect2D;
using PPDEditor.Controls.Resource;
using PPDEditor.Forms;
using PPDFramework.PPDStructure;
using PPDFramework.PPDStructure.PPDData;
using PPDSound;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PPDEditor
{
    public partial class ResourceManager : DockView
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

        readonly string[] allowImageExtensions = [".jpg", ".jpeg", ".gif", ".png"];
        readonly string[] allowSoundExtensions = [".wav", ".ogg"];

        private Sound sound;

        /// <summary>
        /// 木の 1 項目の情報です。<see cref="TreeViewItem.Tag"/> に入れる。
        /// </summary>
        sealed class ResourceNode(string key, string name)
        {
            /// <summary>
            /// 種類です（folder, image, effect, sound, others）。
            /// </summary>
            public string Key
            {
                get;
            } = key;

            public string Name
            {
                get;
            } = name;

            public bool Disabled
            {
                get;
                set;
            }

            /// <summary>
            /// 無効のときにアイコンに重ねる印です。
            /// </summary>
            public UIElement DisabledMark
            {
                get;
                set;
            }
        }

        static readonly Dictionary<string, ImageSource> icons = new()
        {
            { "folder", LoadIcon("folder") },
            { "image", LoadIcon("image") },
            { "effect", LoadIcon("effect") },
            { "sound", LoadIcon("sound") },
            { "others", LoadIcon("document") },
        };
        static readonly ImageSource disabledIcon = LoadIcon("disabled");

        readonly ObservableCollection<KeyValuePair<object, object>> infoRows = [];

        public ResourceManager()
        {
            InitializeComponent();
            Initialize();
        }

        public void SetLang()
        {
            this.リソースを追加MenuItem.Header = Utility.Language["AddResource"];
            this.imageを追加MenuItem.Header = Utility.Language["AddImage"];
            this.effectを追加MenuItem.Header = Utility.Language["AddEffect"];
            this.soundを追加MenuItem.Header = Utility.Language["AddSound"];
            this.その他を追加MenuItem.Header = Utility.Language["AddOthers"];
            this.削除MenuItem.Header = Utility.Language["Delete"];
            this.無効化MenuItem.Header = Utility.Language["MakeInvalid"];
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
            dataGridView1.ItemsSource = infoRows;
            CreateFolderNode(treeView1.Items, "Image");
            CreateFolderNode(treeView1.Items, "Effect");
            CreateFolderNode(treeView1.Items, "Sound");
            CreateFolderNode(treeView1.Items, "Others");
        }

        private static ImageSource LoadIcon(string name)
        {
            var image = new BitmapImage(new Uri(String.Format("pack://application:,,,/PPDEditor;component/img/{0}.png", name)));
            image.Freeze();
            return image;
        }

        private TreeViewItem ImageNode
        {
            get
            {
                return (TreeViewItem)treeView1.Items[0];
            }
        }

        private TreeViewItem EffectNode
        {
            get
            {
                return (TreeViewItem)treeView1.Items[1];
            }
        }

        private TreeViewItem SoundNode
        {
            get
            {
                return (TreeViewItem)treeView1.Items[2];
            }
        }

        private TreeViewItem OthersNode
        {
            get
            {
                return (TreeViewItem)treeView1.Items[3];
            }
        }

        private TreeViewItem SelectedNode
        {
            get
            {
                return treeView1.SelectedItem as TreeViewItem;
            }
        }

        private static ResourceNode GetNode(TreeViewItem item)
        {
            return (ResourceNode)item.Tag;
        }

        /// <summary>
        /// 一番上からの名前を「\」でつないだもの（以前の TreeNode.FullPath と同じ）を返す。
        /// </summary>
        private static string GetFullPath(TreeViewItem item)
        {
            var names = new List<string>();
            for (ItemsControl current = item; current is TreeViewItem treeViewItem; current = ItemsControl.ItemsControlFromItemContainer(current))
            {
                names.Insert(0, GetNode(treeViewItem).Name);
            }
            return String.Join("\\", names);
        }

        private TreeViewItem FindNode(string path)
        {
            ItemsControl current = treeView1;
            foreach (var name in path.Split('\\'))
            {
                TreeViewItem found = null;
                foreach (TreeViewItem child in current.Items)
                {
                    if (GetNode(child).Name == name)
                    {
                        found = child;
                        break;
                    }
                }
                if (found == null)
                {
                    return null;
                }
                current = found;
            }
            return current as TreeViewItem;
        }

        private static void SetDisabled(TreeViewItem item, bool disabled)
        {
            var node = GetNode(item);
            node.Disabled = disabled;
            node.DisabledMark.Visibility = disabled ? Visibility.Visible : Visibility.Collapsed;
        }

        public void Clear()
        {
            ImageNode.Items.Clear();
            EffectNode.Items.Clear();
            SoundNode.Items.Clear();
            OthersNode.Items.Clear();
            infoRows.Clear();
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
                    CreateNode(ImageNode.Items, Path.GetFileName(filePath), NodeType.Image);
                }
            }

            dir = Path.Combine(projectDirPath, EffectDirPath);
            Utility.RecursiveCreateFolder(dir);
            foreach (string filePath in Directory.GetFiles(dir))
            {
                var extension = Path.GetExtension(filePath);
                if (extension.Equals(".etd", StringComparison.CurrentCultureIgnoreCase))
                {
                    CreateNode(EffectNode.Items, Path.GetFileName(filePath), NodeType.Effect);
                }
            }

            dir = Path.Combine(projectDirPath, SoundDirPath);
            Utility.RecursiveCreateFolder(dir);
            foreach (string filePath in Directory.GetFiles(dir))
            {
                var extension = Path.GetExtension(filePath);
                if (Array.IndexOf(allowSoundExtensions, extension.ToLower()) >= 0)
                {
                    CreateNode(SoundNode.Items, Path.GetFileName(filePath), NodeType.Sound);
                }
            }

            dir = Path.Combine(projectDirPath, OthersDirPath);
            Utility.RecursiveCreateFolder(dir);
            foreach (string filePath in Directory.GetFiles(dir))
            {
                CreateNode(OthersNode.Items, Path.GetFileName(filePath), NodeType.Others);
            }

            var disables = disableList.Split('|');
            foreach (string disable in disables)
            {
                var node = FindNode(disable);
                if (node != null && GetNode(node).Key != "folder")
                {
                    SetDisabled(node, true);
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

        private static string[] GetChildrenList(TreeViewItem node)
        {
            var ret = new List<string>();
            foreach (TreeViewItem childNode in node.Items)
            {
                ret.Add(GetNode(childNode).Name);
            }
            return [.. ret];
        }

        private string[] GetList(Filter filter)
        {
            var ret = new List<string>();
            var nodes = new Queue<TreeViewItem>();
            nodes.Enqueue(ImageNode);
            nodes.Enqueue(EffectNode);
            nodes.Enqueue(SoundNode);
            nodes.Enqueue(OthersNode);
            while (nodes.Count > 0)
            {
                var node = nodes.Dequeue();
                foreach (TreeViewItem childNode in node.Items)
                {
                    if (GetNode(childNode).Key == "folder")
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
                                ret.Add(GetFullPath(childNode));
                                ret.AddRange(GetListEffect(childNode));
                            }
                        }
                        else
                        {
                            if ((filter & Filter.Disabled) == Filter.Disabled)
                            {
                                ret.Add(GetFullPath(childNode));
                                ret.AddRange(GetListEffect(childNode));
                            }
                        }
                    }
                }
            }

            return [.. ret];
        }

        private string[] GetListEffect(TreeViewItem node)
        {
            var ret = new List<string>();
            var name = GetNode(node).Name;
            var dir = Path.Combine(Path.Combine(WindowUtility.MainForm.CurrentProjectDir, EffectDirPath), Path.GetFileNameWithoutExtension(name));
            if (ItemsControl.ItemsControlFromItemContainer(node) == EffectNode)
            {
                foreach (string filePath in Directory.GetFiles(dir))
                {
                    if (Array.IndexOf(allowImageExtensions, Path.GetExtension(filePath)) >= 0)
                    {
                        ret.Add(String.Format("Effect\\{0}\\{1}", Path.GetFileNameWithoutExtension(name), Path.GetFileName(filePath)));
                    }
                }
            }

            return [.. ret];
        }

        public void WriteResource(Stream stream, string fileName)
        {
            var path = Path.Combine(Path.Combine(WindowUtility.MainForm.CurrentProjectDir, ResourcePath), fileName);

            using FileStream fs = File.Open(path, FileMode.Open);
            byte[] data = new byte[fs.Length];
            fs.ReadAtLeast(data, data.Length, throwOnEndOfStream: false);
            stream.Write(data, 0, data.Length);
        }

        private void Imageを追加MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog1 = new OpenFileDialog
            {
                Multiselect = true
            };
            openFileDialog1.Filter = "Image(*.jpg;*.jpeg;*.gif;*.png)|*.jpg;*.jpeg;*.gif;*.png";
            if (openFileDialog1.ShowDialog(Window.GetWindow(this)) == true)
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
                MessageBox.Show(Window.GetWindow(this), String.Format("{0}\n{1}", Utility.Language["AlreadyExistResource"], filePath));
                return;
            }

            CheckResourceDir();
            File.Copy(filePath, copyPath, true);

            CreateNode(ImageNode.Items, Path.GetFileName(filePath), NodeType.Image);
        }

        private void Effectを追加MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog1 = new OpenFileDialog
            {
                Multiselect = true
            };
            openFileDialog1.Filter = "Effect(*.etd)|*.etd";
            if (openFileDialog1.ShowDialog(Window.GetWindow(this)) == true)
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
                MessageBox.Show(Window.GetWindow(this), String.Format("{0}\n{1}", Utility.Language["AlreadyExistResource"], filePath));
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

            CreateNode(EffectNode.Items, Path.GetFileName(filePath), NodeType.Effect);

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

        private void Soundを追加MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog1 = new OpenFileDialog
            {
                Multiselect = true
            };
            openFileDialog1.Filter = Utility.Language["SMWaveFilter"];
            if (openFileDialog1.ShowDialog(Window.GetWindow(this)) == true)
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
                MessageBox.Show(Window.GetWindow(this), String.Format("{0}\n{1}", Utility.Language["AlreadyExistResource"], filePath));
                return;
            }

            CheckResourceDir();
            File.Copy(filePath, copyPath, true);

            CreateNode(SoundNode.Items, Path.GetFileName(filePath), NodeType.Sound);
        }

        private void その他を追加MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog1 = new OpenFileDialog
            {
                Multiselect = true
            };
            openFileDialog1.Filter = Utility.Language["MovieFilter"];
            if (openFileDialog1.ShowDialog(Window.GetWindow(this)) == true)
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
                MessageBox.Show(Window.GetWindow(this), String.Format("{0}\n{1}", Utility.Language["AlreadyExistResource"], filePath));
                return;
            }

            CheckResourceDir();
            File.Copy(filePath, copyPath, true);

            CreateNode(OthersNode.Items, Path.GetFileName(filePath), NodeType.Others);
        }

        private static void CheckResourceDir()
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

        private void リソースを追加MenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            imageを追加MenuItem.IsEnabled = effectを追加MenuItem.IsEnabled = soundを追加MenuItem.IsEnabled = その他を追加MenuItem.IsEnabled = WindowUtility.MainForm.IsProjectLoaded;
        }

        private void TreeView1_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 右クリックした項目を選ぶ
            for (var element = e.OriginalSource as DependencyObject; element != null && element != treeView1; element = VisualTreeHelper.GetParent(element))
            {
                if (element is TreeViewItem item)
                {
                    item.IsSelected = true;
                    item.Focus();
                    break;
                }
            }
        }

        private void TreeView1_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // メニューはファイルの項目にだけ出す
            if (SelectedNode == null || GetNode(SelectedNode).Key == "folder")
            {
                e.Handled = true;
                return;
            }
            無効化MenuItem.Header = EnabledNode(SelectedNode) ? Utility.Language["MakeInvalid"] : Utility.Language["MakeValid"];
        }

        private static bool EnabledNode(TreeViewItem node)
        {
            return !GetNode(node).Disabled;
        }

        private void 削除MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedNode = SelectedNode;
            if (selectedNode == null)
            {
                return;
            }
            if (MessageBox.Show(Window.GetWindow(this), String.Format(Utility.Language["DeleteConfirmText"], GetFullPath(selectedNode)), Utility.Language["DeleteConfirm"], MessageBoxButton.OKCancel) != MessageBoxResult.OK)
            {
                return;
            }

            var path = Path.Combine(WindowUtility.MainForm.CurrentProjectDir, Path.Combine("Resource", GetFullPath(selectedNode)));
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            if (Path.GetExtension(path).Equals(".etd", StringComparison.CurrentCultureIgnoreCase))
            {
                var dir = Path.Combine(WindowUtility.MainForm.CurrentProjectDir, Path.Combine(Path.GetDirectoryName(path), Path.GetFileName(path)));
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
            }

            ((ItemsControl)ItemsControl.ItemsControlFromItemContainer(selectedNode)).Items.Remove(selectedNode);

            ContentChanged();
        }

        private void 無効化MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedNode != null)
            {
                SetDisabled(SelectedNode, EnabledNode(SelectedNode));
                ContentChanged();
            }
        }

        private static TreeViewItem CreateFolderNode(ItemCollection nodes, string name)
        {
            return AddNode(nodes, "folder", name);
        }

        /// <summary>
        /// アイコンと名前を出す項目を加える。
        /// </summary>
        private static TreeViewItem AddNode(ItemCollection nodes, string key, string name)
        {
            var node = new ResourceNode(key, name);
            var icon = new Grid
            {
                Width = 16,
                Height = 16,
                Margin = new Thickness(0, 0, 3, 0)
            };
            icon.Children.Add(new Image { Source = icons[key] });
            node.DisabledMark = new Image { Source = disabledIcon, Visibility = Visibility.Collapsed };
            icon.Children.Add(node.DisabledMark);
            var header = new StackPanel { Orientation = Orientation.Horizontal };
            header.Children.Add(icon);
            header.Children.Add(new TextBlock { Text = name, VerticalAlignment = VerticalAlignment.Center });
            var item = new TreeViewItem
            {
                Header = header,
                Tag = node
            };
            nodes.Add(item);
            return item;
        }

        private static TreeViewItem CreateNode(ItemCollection nodes, string name, NodeType nodeType)
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

            return AddNode(nodes, key, name);
        }

        private void TreeView1_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ChangeDataGrid();
        }

        private void ChangeDataGrid()
        {
            infoRows.Clear();

            if (SelectedNode == null || !WindowUtility.MainForm.IsProjectLoaded)
            {
                return;
            }

            switch (GetNode(SelectedNode).Key)
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

        private static void AddFolderInfo()
        {
        }

        private void AddImageInfo()
        {
            var path = Path.Combine(WindowUtility.MainForm.CurrentProjectDir, Path.Combine("Resource", GetFullPath(SelectedNode)));
            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                using (var image = System.Drawing.Image.FromFile(path))
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
                                val = val.Trim(['\0']);
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
                MessageBox.Show(Window.GetWindow(this), e.Message);
            }
        }

        private void AddInfo(object key, object value)
        {
            infoRows.Add(new(key, value));
        }

        private static string GetImageFormatString(ImageFormat format)
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
            var path = Path.Combine(WindowUtility.MainForm.CurrentProjectDir, Path.Combine("Resource", GetFullPath(SelectedNode)));
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
                MessageBox.Show(Window.GetWindow(this), e.Message);
            }
        }

        private void AddSoundInfo()
        {
            var path = Path.Combine(WindowUtility.MainForm.CurrentProjectDir, Path.Combine("Resource", GetFullPath(SelectedNode)));
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
                MessageBox.Show(Window.GetWindow(this), e.Message);
            }
        }

        private void AddOthersInfo()
        {
            var path = Path.Combine(WindowUtility.MainForm.CurrentProjectDir, Path.Combine("Resource", GetFullPath(SelectedNode)));
            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                using FileStream fs = File.Open(path, FileMode.Open);
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
            catch (Exception e)
            {
                MessageBox.Show(Window.GetWindow(this), e.Message);
            }
        }

        private void ChangePanel(UIElement control)
        {
            previewHost.Content = control;
        }

        private void TreeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C)
            {
                if (SelectedNode != null)
                {
                    Clipboard.SetText(GetNode(SelectedNode).Name);
                }
                e.Handled = true;
            }
        }
    }
}
