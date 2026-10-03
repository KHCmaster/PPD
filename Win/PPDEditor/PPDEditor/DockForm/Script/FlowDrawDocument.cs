using AvalonDock.Layout;
using FlowScriptControl.Controls;
using PPDConfiguration;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace PPDEditor.DockForm.Script
{
    /// <summary>
    /// スクリプトのファイルを 1 つ開いて編集するタブです（以前の FlowDrawDockForm）。
    /// </summary>
    public class FlowDrawDocument
    {
        private string filePath;
        private bool changed;
        // 保存するかもう聞いたか（同じ閉じる操作で 2 回聞かないため）
        private bool closeConfirmed;

        public LayoutDocument Document
        {
            get;
            private set;
        }

        public FlowDrawPanel FlowDrawPanel
        {
            get;
            private set;
        }

        public string FilePath
        {
            get { return filePath; }
            set
            {
                if (filePath != value)
                {
                    filePath = value;
                    Document.ToolTip = filePath;
                    UpdateTitle();
                }
            }
        }

        public bool IsContentChanged
        {
            get { return changed; }
        }

        public bool CanSave
        {
            get
            {
                if (String.IsNullOrEmpty(FilePath))
                {
                    return false;
                }

                if (Path.GetInvalidPathChars().Any(FilePath.Contains))
                {
                    return false;
                }

                var dir = Path.GetDirectoryName(FilePath);
                if (!Directory.Exists(dir))
                {
                    return false;
                }

                return true;
            }
        }

        public event EventHandler Modified;

        /// <summary>
        /// タブを閉じたときに発生します。
        /// </summary>
        public event EventHandler Closed;

        public FlowDrawDocument()
        {
            FlowDrawPanel = new FlowDrawPanel();
            Document = new LayoutDocument
            {
                Content = FlowDrawPanel
            };
            Document.Closing += Document_Closing;
            Document.Closed += Document_Closed;
            FlowDrawPanel.Modified += FlowDrawPanel_Modified;
        }

        private void UpdateTitle()
        {
            var title = Path.GetFileName(filePath);
            Document.Title = changed ? "*" + title : title;
        }

        public void ContentChanged()
        {
            changed = true;
            UpdateTitle();
        }

        public void ContentSaved()
        {
            changed = false;
            UpdateTitle();
        }

        void Document_Closing(object sender, CancelEventArgs e)
        {
            if (closeConfirmed)
            {
                return;
            }
            if (IsContentChanged)
            {
                var result = MessageBox.Show(Window.GetWindow(FlowDrawPanel), Utility.Language["AskSave"], Utility.Language["Confirm"], MessageBoxButton.YesNoCancel);
                e.Cancel = result == MessageBoxResult.Cancel;
                if (result == MessageBoxResult.Yes)
                {
                    Save();
                }
            }
        }

        void Document_Closed(object sender, EventArgs e)
        {
            FlowDrawPanel.Dispose();
            Closed?.Invoke(this, EventArgs.Empty);
        }

        void FlowDrawPanel_Modified(object sender, EventArgs e)
        {
            Modified?.Invoke(this, EventArgs.Empty);
            ContentChanged();
        }

        /// <summary>
        /// ファイルを読み込みます。
        /// </summary>
        public void Load()
        {
            if (File.Exists(FilePath))
            {
                using (Stream stream = File.Open(FilePath, FileMode.Open))
                {
                    FlowDrawPanel.LoadXML(stream);
                }

                ContentSaved();
            }
        }

        public void Save()
        {
            if (!CanSave)
            {
                return;
            }

            using (Stream stream = File.Open(FilePath, FileMode.Create))
            {
                FlowDrawPanel.SaveXML(stream);
            }
            ContentSaved();
        }

        /// <summary>
        /// タブを閉じます。保存していない変更があるときは、保存するか聞きます（以前のフォームを閉じたときと同じ）。
        /// </summary>
        public void Close()
        {
            var e = new CancelEventArgs();
            Document_Closing(this, e);
            if (e.Cancel)
            {
                return;
            }
            closeConfirmed = true;
            Document.Close();
        }

        public void Activate()
        {
            Document.IsActive = true;
        }

        public void SetLang()
        {
            var filePath = Path.Combine("Lang", String.Format("lang_FlowScript_{0}.ini", PPDStaticSetting.langFileISO));
            if (!File.Exists(filePath))
            {
                filePath = Path.Combine("Lang", String.Format("lang_FlowScript_{0}.ini", "en"));
                if (!File.Exists(filePath))
                {
                    Path.Combine("Lang", String.Format("lang_FlowScript_{0}.ini", "jp"));
                }
            }

            if (!File.Exists(filePath))
            {
                return;
            }

            var sr = new StreamReader(filePath);
            var flowLang = new SettingReader(sr.ReadToEnd());
            sr.Close();
            FlowDrawPanel.LanguageProvider = new FlowScriptLanguageProvider(flowLang);
        }
    }
}
