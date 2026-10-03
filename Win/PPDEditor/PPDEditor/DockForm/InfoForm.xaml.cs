using Microsoft.Win32;
using PPDEditor.Forms;
using PPDEditorCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace PPDEditor
{
    public partial class InfoForm : DockView
    {
        const string PresetDir = "preset_parameters";

        readonly string[] infoKeys = ["幅(px)", "高さ(px)", "長さ", "位置", "回転角", "ID"];
        readonly string[] infoValues = ["", "", "", "", "", ""];
        List<KeyValuePair<string, string>> parameters = [];
        Dictionary<string, string> copiedParameters;

        public DateTime PresetsLoadTime
        {
            get;
            private set;
        }

        /// <summary>
        /// 読み込んだプリセットの一番上のフォルダーです。
        /// </summary>
        public ParameterPresetFolder PresetRoot
        {
            get;
            private set;
        }

        public ParameterPreset[] Presets
        {
            get
            {
                return [.. PresetRoot.GetAllPresets()];
            }
        }

        public InfoForm()
        {
            InitializeComponent();
            if (!Directory.Exists(PresetDir))
            {
                Directory.CreateDirectory(PresetDir);
            }
            UpdateInfo();
            LoadPresets();
        }
        public void SetLang()
        {
            infoKeys[0] = Utility.Language["Width"];
            infoKeys[1] = Utility.Language["Height"];
            infoKeys[2] = Utility.Language["Length"];
            infoKeys[3] = Utility.Language["Position"];
            infoKeys[4] = Utility.Language["Angle"];
            UpdateInfo();
            dataGridViewTextBoxColumn1.Header = Utility.Language["Key"];
            dataGridViewTextBoxColumn2.Header = Utility.Language["Value"];
            編集MenuItem.Header = Utility.Language["Edit"];
            パラメーターMenuItem.Header = Utility.Language["Parameter"];
            プリセットMenuItem.Header = Utility.Language["Preset"];
            読み込みMenuItem.Header = Utility.Language["Load"];
            保存MenuItem.Header = Utility.Language["Save"];
            コピーMenuItem.Header = Utility.Language["Copy"];
            貼り付けMenuItem.Header = Utility.Language["Paste"];
        }
        public void SetSkin()
        {
        }
        public void SetMovieInfo(string width, string height, string length)
        {
            infoValues[0] = width;
            infoValues[1] = height;
            infoValues[2] = length;
            UpdateInfo();
        }
        public void SetMarkInfo(string pos, string angle, string id)
        {
            infoValues[3] = pos;
            infoValues[4] = angle;
            infoValues[5] = id;
            UpdateInfo();
        }

        private void UpdateInfo()
        {
            dataGridView1.ItemsSource = infoKeys.Zip(infoValues, (key, value) => new KeyValuePair<string, string>(key, value)).ToList();
        }

        private void UpdateParameters()
        {
            var marks = Utility.GetSelectedMarks();
            UpdateParameters(marks);
        }

        public void UpdateParameters(Mark[] marks)
        {
            parameters = marks.Length == 0 ? [] : [.. Utility.GetCommonParameters(marks)];
            dataGridView2.ItemsSource = parameters;
        }

        private void パラメーターMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var sheet = WindowUtility.LayerManager.SelectedPpdSheet;
            if (sheet == null)
            {
                return;
            }
            var marks = Utility.GetSelectedMarks();
            if (marks.Length == 0)
            {
                MessageBox.Show(Window.GetWindow(this), Utility.Language["NoSelectedMarks"]);
                return;
            }

            var commanParameters = Utility.GetCommonParameters(marks, out string[] existKeys);
            var form = new EditParameterWindow();
            form.SetLang();
            form.Parameters = commanParameters;
            form.ExistKeys = existKeys;
            if (WpfDialog.ShowDialog(form))
            {
                var newParameters = form.Parameters;
                var removeParameters = new HashSet<string>();
                foreach (var key in commanParameters.Keys)
                {
                    if (!newParameters.ContainsKey(key))
                    {
                        removeParameters.Add(key);
                    }
                }
                sheet.StartGroupCommand();
                foreach (var key in removeParameters)
                {
                    sheet.RemoveParameter(key);
                }
                foreach (var pair in newParameters)
                {
                    sheet.ChangeParameter(pair.Key, pair.Value);
                }
                sheet.EndGroupCommand();
                UpdateParameters();
            }
        }

        private void LoadPresets()
        {
            PresetRoot = LoadPresets(PresetDir);
            PresetsLoadTime = DateTime.Now;
            読み込みMenuItem.Items.Clear();
            AddPresetMenuItems(読み込みMenuItem, PresetRoot);
        }

        private static ParameterPresetFolder LoadPresets(string dir)
        {
            var folder = new ParameterPresetFolder(Path.GetFileName(dir));
            foreach (var childDir in Directory.GetDirectories(dir))
            {
                folder.Folders.Add(LoadPresets(childDir));
            }

            foreach (var childFile in Directory.GetFiles(dir))
            {
                try
                {
                    var name = Path.Combine(Path.GetDirectoryName(childFile), Path.GetFileNameWithoutExtension(childFile))[dir.Length..].TrimStart(Path.DirectorySeparatorChar);
                    folder.Presets.Add(new ParameterPreset(childFile, name));
                }
                catch
                {
                    MessageBox.Show(Utility.Language["ErrorInLoadingParameterPreset"]);
                }
            }
            return folder;
        }

        private void AddPresetMenuItems(MenuItem parent, ParameterPresetFolder folder)
        {
            foreach (var childFolder in folder.Folders)
            {
                var childMenu = new MenuItem
                {
                    Header = ToMenuText(childFolder.Name),
                    ToolTip = childFolder.Name
                };
                parent.Items.Add(childMenu);
                AddPresetMenuItems(childMenu, childFolder);
            }

            foreach (var preset in folder.Presets)
            {
                var name = Path.GetFileNameWithoutExtension(preset.FilePath);
                var childMenu = new MenuItem
                {
                    Header = ToMenuText(name),
                    ToolTip = name,
                    Tag = preset
                };
                childMenu.Click += ChildMenu_Click;
                parent.Items.Add(childMenu);
            }
        }

        /// <summary>
        /// メニューでは「_」がアクセスキーの印になるので、そのまま出るようにする。
        /// </summary>
        private static string ToMenuText(string text)
        {
            return text.Replace("_", "__");
        }

        private static IEnumerable<MenuItem> GetPresetMenuItems(MenuItem parent)
        {
            foreach (var child in parent.Items.OfType<MenuItem>())
            {
                if (child.Tag is ParameterPreset)
                {
                    yield return child;
                }
                foreach (var descendant in GetPresetMenuItems(child))
                {
                    yield return descendant;
                }
            }
        }

        void ChildMenu_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var preset = (ParameterPreset)menuItem.Tag;
            if (menuItem.IsChecked)
            {
                UnapplyParameters(preset.Parameters);
            }
            else
            {
                ApplyParameters(preset.Parameters);
            }
        }

        public void ApplyParameters(KeyValuePair<string, string>[] parameters)
        {
            var marks = Utility.GetSelectedMarks();
            if (marks.Length == 0)
            {
                MessageBox.Show(Window.GetWindow(this), Utility.Language["NoSelectedMarks"]);
                return;
            }

            var sheet = WindowUtility.LayerManager.SelectedPpdSheet;
            sheet.StartGroupCommand();
            foreach (var pair in parameters)
            {
                sheet.ChangeParameter(pair.Key, pair.Value);
            }
            sheet.EndGroupCommand();
            UpdateParameters();
        }

        public void UnapplyParameters(KeyValuePair<string, string>[] parameters)
        {
            var marks = Utility.GetSelectedMarks();
            if (marks.Length == 0)
            {
                MessageBox.Show(Window.GetWindow(this), Utility.Language["NoSelectedMarks"]);
                return;
            }

            var sheet = WindowUtility.LayerManager.SelectedPpdSheet;
            sheet.StartGroupCommand();
            foreach (var mark in marks)
            {
                foreach (var parameter in parameters)
                {
                    sheet.RemoveParameter(mark, parameter.Key);
                }
            }
            sheet.EndGroupCommand();
            UpdateParameters();
        }

        private void 保存MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (parameters.Count == 0)
            {
                MessageBox.Show(Window.GetWindow(this), Utility.Language["NoParameters"]);
                return;
            }

            var saveFileDialog1 = new SaveFileDialog
            {
                InitialDirectory = Path.GetFullPath(PresetDir),
                FileName = "NewPresetParameter.xml"
            };
            if (saveFileDialog1.ShowDialog(Window.GetWindow(this)) == true)
            {
                var document = new XDocument(new XElement("Root"));
                foreach (var pair in parameters)
                {
                    var elem = new XElement("Parameter", new XAttribute("Key", pair.Key), new XAttribute("Value", pair.Value));
                    document.Root.Add(elem);
                }
                document.Save(saveFileDialog1.FileName);
                LoadPresets();
            }
        }

        private void コピーMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var marks = Utility.GetSelectedMarks();
            if (marks.Length == 0)
            {
                return;
            }

            copiedParameters = Utility.GetCommonParameters(marks);
            if (copiedParameters.Count == 0)
            {
                copiedParameters = null;
            }
        }

        private void 貼り付けMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (copiedParameters == null)
            {
                return;
            }

            var marks = Utility.GetSelectedMarks();
            if (marks.Length == 0)
            {
                return;
            }

            var sheet = WindowUtility.LayerManager.SelectedPpdSheet;
            sheet.StartGroupCommand();
            foreach (var pair in copiedParameters)
            {
                sheet.ChangeParameter(pair.Key, pair.Value);
            }
            sheet.EndGroupCommand();
            UpdateParameters();
        }

        private void 編集MenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource != sender)
            {
                return;
            }
            var marks = Utility.GetSelectedMarks();
            コピーMenuItem.IsEnabled = marks.Length > 0;
            貼り付けMenuItem.IsEnabled = copiedParameters != null && marks.Length > 0;
        }

        private void プリセットMenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource != sender)
            {
                return;
            }
            var marks = Utility.GetSelectedMarks();
            if (marks.Length == 0)
            {
                return;
            }

            var common = Utility.GetCommonParameters(marks);
            foreach (var menuItem in GetPresetMenuItems(読み込みMenuItem))
            {
                var state = Utility.GetPresetCheckState((ParameterPreset)menuItem.Tag, common);
                menuItem.IsChecked = state == true;
                // 一部だけ当てはまるときは、チェックの代わりに印を出す
                menuItem.Icon = state == null ? new TextBlock { Text = "－", HorizontalAlignment = HorizontalAlignment.Center } : null;
            }
        }
    }
}
