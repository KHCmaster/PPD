using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PPDEditor.Forms
{
    public partial class CommandSelectWindow : Window
    {
        static readonly ImageSource folderIcon = LoadIcon("folder.png");
        static readonly ImageSource scriptIcon = LoadIcon("document.png");

        readonly List<CommandNode> nodes = [];

        public string SelectedScriptPath
        {
            get
            {
                return Path.Combine("Commands", ((CommandNode)treeView1.SelectedItem).FullPath);
            }
        }

        public CommandSelectWindow()
        {
            InitializeComponent();
            ReadScript();
        }

        private static BitmapImage LoadIcon(string fileName)
        {
            var image = new BitmapImage(new Uri("pack://application:,,,/PPDEditor;component/img/" + fileName));
            image.Freeze();
            return image;
        }

        public void SetLang()
        {
            this.Title = Utility.Language["SelectCommand"];
        }

        private void ReadScript()
        {
            if (!Directory.Exists("Commands"))
            {
                Directory.CreateDirectory("Command");
                return;
            }

            ReadScript(nodes, "Commands", "");
            treeView1.ItemsSource = nodes;
        }

        private static void ReadScript(List<CommandNode> nodes, string dir, string parentPath)
        {
            foreach (var childDir in Directory.GetDirectories(dir))
            {
                var name = Path.GetFileNameWithoutExtension(childDir);
                var node = new CommandNode(name, Path.Combine(parentPath, name), false);
                nodes.Add(node);
                ReadScript(node.Children, childDir, node.FullPath);
            }
            foreach (var childFile in Directory.GetFiles(dir))
            {
                if (!Path.GetExtension(childFile).Equals(".fsml", StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                var name = Path.GetFileNameWithoutExtension(childFile);
                nodes.Add(new CommandNode(name, Path.Combine(parentPath, name), true));
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void TreeView1_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            okButton.IsEnabled = e.NewValue is CommandNode node && node.IsScript;
        }

        private void TreeView1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (treeView1.SelectedItem is CommandNode node && node.IsScript)
            {
                DialogResult = true;
            }
        }

        /// <summary>
        /// コマンドの一覧の項目です。パスは以前の TreeNode.FullPath と同じく、拡張子のない名前を「\」でつないだものです。
        /// </summary>
        class CommandNode(string name, string fullPath, bool isScript)
        {
            public string Name { get; } = name;
            public string FullPath { get; } = fullPath;
            public bool IsScript { get; } = isScript;
            public ImageSource Icon { get; } = isScript ? scriptIcon : folderIcon;
            public List<CommandNode> Children { get; } = [];
        }
    }
}
