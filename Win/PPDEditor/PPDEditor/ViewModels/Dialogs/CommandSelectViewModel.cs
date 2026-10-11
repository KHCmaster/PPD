using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;

namespace PPDEditor.ViewModels.Dialogs
{
    /// <summary>
    /// Commands フォルダーのスクリプト（.fsml）から、コマンドを 1 つ選ぶダイアログです。
    /// </summary>
    public partial class CommandSelectViewModel : DialogViewModel
    {
        const string CommandsDirectory = "Commands";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OkCommand))]
        public partial CommandNode SelectedNode { get; set; }

        public CommandSelectViewModel()
        {
            if (!Directory.Exists(CommandsDirectory))
            {
                // 以前は「Command」という違う名前のフォルダーを作っていた
                Directory.CreateDirectory(CommandsDirectory);
                return;
            }
            ReadScript(Nodes, CommandsDirectory, "");
        }

        public List<CommandNode> Nodes { get; } = [];

        public string SelectedScriptPath
        {
            get { return Path.Combine(CommandsDirectory, SelectedNode.FullPath); }
        }

        protected override bool CanOk()
        {
            return SelectedNode != null && SelectedNode.IsScript;
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
    }

    /// <summary>
    /// コマンドの一覧の項目（フォルダーかスクリプト）です。パスは以前の TreeNode.FullPath と同じく、拡張子のない名前を「\\」でつないだものです。
    /// </summary>
    public class CommandNode(string name, string fullPath, bool isScript)
    {
        public string Name { get; } = name;
        public string FullPath { get; } = fullPath;
        public bool IsScript { get; } = isScript;
        public List<CommandNode> Children { get; } = [];
    }
}
