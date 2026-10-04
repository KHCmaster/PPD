using FlowScriptControlTest.ViewModels;
using FlowScriptDrawControl.Model;
using System;
using System.IO;

namespace FlowScriptControlTest.Models
{
    /// <summary>
    /// 実行する 1 つのスクリプトです（保存した内容、ブレークポイント、元の文書）。
    /// </summary>
    class ExecuteInfo
    {
        public Stream Stream
        {
            get;
            private set;
        }

        public int[] BreakPoints
        {
            get;
            private set;
        }

        public FlowDocumentViewModel Document
        {
            get;
            private set;
        }

        public Source[] Sources
        {
            get;
            private set;
        }

        public event Action<ExecuteInfo, int> BreakPointAdded;
        public event Action<ExecuteInfo, int> BreakPointRemoved;

        public ExecuteInfo(Stream stream, int[] breakPoints, FlowDocumentViewModel document, Source[] sources)
        {
            Stream = stream;
            BreakPoints = breakPoints;
            Document = document;
            Sources = sources;

            foreach (var source in sources)
            {
                source.PropertyChanged += Source_PropertyChanged;
            }
        }

        public void RemoveEventHandler()
        {
            foreach (var source in Sources)
            {
                source.PropertyChanged -= Source_PropertyChanged;
            }
        }

        void Source_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsBreakPointSet")
            {
                // スクリプトを読み込む前（ハンドラーがないとき）に変えても、例外にしない
                var source = (Source)sender;
                if (source.IsBreakPointSet)
                {
                    BreakPointAdded?.Invoke(this, source.Id);
                }
                else
                {
                    BreakPointRemoved?.Invoke(this, source.Id);
                }
            }
        }
    }
}
