using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FlowScriptControl.Classes
{
    /// <summary>
    /// ノードの一覧（ノードの追加のメニューとノードのツリー）の 1 項目です。フォルダーか、ノードの種類を表します。
    /// </summary>
    public sealed class FlowNodeEntry : INotifyPropertyChanged
    {
        private bool isVisible = true;
        private bool isExpanded;

        public event PropertyChangedEventHandler PropertyChanged;

        internal FlowNodeEntry(string name, string text, IToolTipText toolTip, FlowSourceDumper dumper)
        {
            Name = name;
            Text = text;
            ToolTip = toolTip;
            Dumper = dumper;
        }

        /// <summary>
        /// 名前（ノードの名前の、ドットで区切られた 1 つ分）です。
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 表示する文字列です。
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// ノードの種類です。フォルダーのときは null です。
        /// </summary>
        public FlowSourceDumper Dumper { get; }

        internal IToolTipText ToolTip { get; }

        /// <summary>
        /// フォルダーかどうかを取得します。
        /// </summary>
        public bool IsFolder
        {
            get { return Dumper == null; }
        }

        /// <summary>
        /// ツールチップの文字列（言語の設定に合わせたもの）です。
        /// </summary>
        public string ToolTipText
        {
            get { return ToolTip?.ToolTipText ?? ""; }
        }

        /// <summary>
        /// 子の項目です。
        /// </summary>
        public ObservableCollection<FlowNodeEntry> Children { get; } = [];

        /// <summary>
        /// 表示するかどうか（フィルターの結果）です。
        /// </summary>
        public bool IsVisible
        {
            get { return isVisible; }
            set
            {
                if (isVisible != value)
                {
                    isVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// ツリーで展開しているかどうかです。
        /// </summary>
        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                if (isExpanded != value)
                {
                    isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 言語が変わったときに、ツールチップの表示を更新します。
        /// </summary>
        internal void NotifyToolTipChanged()
        {
            OnPropertyChanged(nameof(ToolTipText));
            foreach (var child in Children)
            {
                child.NotifyToolTipChanged();
            }
        }

        /// <summary>
        /// 同じ構造の項目を作ります（メニューとツリーで、表示の状態を別々に持つため）。
        /// </summary>
        internal FlowNodeEntry Clone()
        {
            var ret = new FlowNodeEntry(Name, Text, ToolTip, Dumper);
            foreach (var child in Children)
            {
                ret.Children.Add(child.Clone());
            }
            return ret;
        }

        internal IEnumerable<FlowNodeEntry> Descendants()
        {
            foreach (var child in Children)
            {
                yield return child;
                foreach (var descendant in child.Descendants())
                {
                    yield return descendant;
                }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
