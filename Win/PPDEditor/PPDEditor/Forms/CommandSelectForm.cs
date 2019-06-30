using System;
using System.IO;
using System.Windows.Forms;

namespace PPDEditor.Forms
{
    public partial class CommandSelectForm : Form
    {
        public string SelectedScriptPath
        {
            get
            {
                return Path.Combine("Commands", treeView1.SelectedNode.FullPath);
            }
        }

        public CommandSelectForm()
        {
            InitializeComponent();

            Load += CommandSelectForm_Load;
        }

        public void SetLang()
        {
            this.Text = Utility.Language["SelectCommand"];
        }

        private void ReadScript()
        {
            if (!Directory.Exists("Commands"))
            {
                Directory.CreateDirectory("Command");
            }

            ReadScript(treeView1.Nodes, "Commands");
            if (treeView1.Nodes.Count > 0)
            {
                treeView1.SelectedNode = treeView1.Nodes[0];
            }
        }

        private void ReadScript(TreeNodeCollection nodes, string dir)
        {
            foreach (var childDir in Directory.GetDirectories(dir))
            {
                var node = new TreeNode
                {
                    Text = Path.GetFileNameWithoutExtension(childDir),
                    ImageKey = "folder",
                    SelectedImageKey = "folder"
                };
                nodes.Add(node);
                ReadScript(node.Nodes, childDir);
            }
            foreach (var childFile in Directory.GetFiles(dir))
            {
                if (Path.GetExtension(childFile).ToLower() != ".fsml")
                {
                    continue;
                }

                var node = new TreeNode
                {
                    Text = Path.GetFileNameWithoutExtension(childFile),
                    ImageKey = "script",
                    SelectedImageKey = "script"
                };
                nodes.Add(node);
            }
        }

        void CommandSelectForm_Load(object sender, EventArgs e)
        {
            treeView1.ImageList = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit
            };
            treeView1.ImageList.Images.Add("folder", PPDEditor.Properties.Resources.folder);
            treeView1.ImageList.Images.Add("script", PPDEditor.Properties.Resources.document);
            ReadScript();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            okButton.Enabled = e.Node.ImageKey == "script";
        }
    }
}
