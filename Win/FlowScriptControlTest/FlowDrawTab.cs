using FlowScriptControl.Controls;
using System;
using System.IO;
using System.Windows.Controls;

namespace FlowScriptControlTest
{
    /// <summary>
    /// スクリプトを 1 つ編集するタブです。見出しの右クリックで閉じられます。
    /// </summary>
    public class FlowDrawTab : TabItem
    {
        private string filePath;

        public event EventHandler CloseRequested;

        public FlowDrawTab()
        {
            FlowDrawPanel = new FlowDrawPanel();
            Content = FlowDrawPanel;
            var closeMenuItem = new MenuItem { Header = "タブを閉じる" };
            closeMenuItem.Click += (sender, e) => CloseRequested?.Invoke(this, EventArgs.Empty);
            var headerText = new TextBlock { ContextMenu = new ContextMenu { Items = { closeMenuItem } } };
            Header = headerText;
            FilePath = Guid.NewGuid().ToString();
        }

        public FlowDrawPanel FlowDrawPanel { get; }

        public string FilePath
        {
            get { return filePath; }
            set
            {
                if (filePath != value)
                {
                    filePath = value;
                    ((TextBlock)Header).Text = Path.GetFileName(filePath);
                }
            }
        }
    }
}
