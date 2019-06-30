using FlowScriptDrawControl.Model;
using FlowScriptEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FlowScriptDrawControl.Control
{
    /// <summary>
    /// SourceControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SourceControl : SelectableControl
    {
        private Dictionary<string, SourceItemControl> inItems;
        private Dictionary<string, SourceItemControl> outItems;

        public Source CurrentSource
        {
            get
            {
                return (Source)DataContext;
            }
        }

        public SourceItemControl[] InItems
        {
            get
            {
                return leftStack.Children.Cast<SourceItemControl>().ToArray();
            }
        }

        public SourceItemControl[] OutItems
        {
            get
            {
                return rightStack.Children.Cast<SourceItemControl>().ToArray();
            }
        }

        public event EventHandler OutItemSelected;
        public event EventHandler InItemSelected;
        public event EventHandler TryConnected;
        public event EventHandler<ChangeSourceEventArgs> SourceChanged;

        public SourceControl()
        {
            InitializeComponent();

            DataContextChanged += SourceControl_DataContextChanged;
        }

        void SourceControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateItem();
        }

        private void UpdateItem()
        {
            leftStack.Children.Clear();
            rightStack.Children.Clear();
            foreach (Item item in CurrentSource.Items)
            {
                if (item.IsOut)
                {
                    var outItem = new SourceItemControl { DataContext = item };
                    outItem.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right;
                    outItem.Selected += outItem_Selected;
                    outItem.TryConnected += outItem_TryConnected;
                    rightStack.Children.Add(outItem);
                }
                else
                {
                    var inItem = new SourceItemControl { DataContext = item };
                    inItem.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
                    inItem.Selected += inItem_Selected;
                    inItem.TryConnected += inItem_TryConnected;
                    leftStack.Children.Add(inItem);
                }
            }
            inItems = null;
            outItems = null;
        }

        void outItem_TryConnected(object sender, EventArgs e)
        {
            OnTryConnected(sender as SourceItemControl);
        }

        void inItem_TryConnected(object sender, EventArgs e)
        {
            OnTryConnected(sender as SourceItemControl);
        }

        void inItem_Selected(object sender, EventArgs e)
        {
            OnInItemSelected(sender as SourceItemControl);
        }

        void outItem_Selected(object sender, EventArgs e)
        {
            OnOutItemSelected(sender as SourceItemControl);
        }

        public SourceItemControl GetItemControlByName(string name, bool isOut)
        {
            if (isOut)
            {
                if (outItems == null)
                {
                    outItems = GetDict(isOut);
                }
                return outItems[name];
            }
            else
            {
                if (inItems == null)
                {
                    inItems = GetDict(isOut);
                }
                return inItems[name];
            }
        }

        private Dictionary<string, SourceItemControl> GetDict(bool isOut)
        {
            var dict = new Dictionary<string, SourceItemControl>();
            foreach (var item in isOut ? OutItems : InItems)
            {
                dict.Add(item.CurrentItem.Name, item);
                foreach (var replacedName in item.CurrentItem.ReplacedNames)
                {
                    dict.Add(replacedName, item);
                }
            }
            return dict;
        }

        protected virtual void OnOutItemSelected(SourceItemControl itemControl)
        {
            OutItemSelected?.Invoke(itemControl, new EventArgs());
        }

        protected virtual void OnInItemSelected(SourceItemControl itemControl)
        {
            InItemSelected?.Invoke(itemControl, new EventArgs());
        }

        protected virtual void OnTryConnected(SourceItemControl itemControl)
        {
            TryConnected?.Invoke(itemControl, new EventArgs());
        }

        private void SourceChangeLeftArrowControl_Click(object sender, RoutedEventArgs e)
        {
            var prevAsm = CurrentSource.GetPreviousAsm();
            OnSourceChanged(prevAsm);
        }

        private void SourceChangeRightArrowControl_Click(object sender, RoutedEventArgs e)
        {
            var nextAsm = CurrentSource.GetNextAsm();
            OnSourceChanged(nextAsm);
        }

        protected void OnSourceChanged(AssemblyAndType asm)
        {
            SourceChanged?.Invoke(this, new ChangeSourceEventArgs(asm));
        }
    }
}
