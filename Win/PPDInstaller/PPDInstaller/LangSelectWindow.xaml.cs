using System;
using System.Collections.Generic;
using System.Windows;

namespace PPDInstaller
{
    /// <summary>
    /// インストーラーの言語を選ぶウィンドウです。
    /// </summary>
    public partial class LangSelectWindow : Window
    {
        public LangSelectWindow(IReadOnlyList<string> languages)
        {
            Languages = languages;
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// 言語の名前です（Lang\LangList.ini の行）。
        /// </summary>
        public IReadOnlyList<string> Languages { get; }

        /// <summary>
        /// 選んだ言語の番号です（Lang\{番号}.ini）。
        /// </summary>
        public int SelectedIndex { get; set; }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            NativeMethods.HideSystemMenu(this);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
