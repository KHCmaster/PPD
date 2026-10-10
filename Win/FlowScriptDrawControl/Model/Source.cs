using FlowScriptEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlowScriptDrawControl.Model
{
    public class Source(AssemblyAndType currentAsm) : ScopeChild
    {
        private int id;
        private string name;
        private string fullName;
        private readonly List<Item> items = [];
        private bool isCollapsed;
        private string toolTip;
        private string warning;
        private Comment comment;
        private bool canChange;
        private bool isBreakPointSet;
        private readonly List<AssemblyAndType> sameNameSources = [];
        private readonly AssemblyAndType currentAsm = currentAsm;

        public string ToolTip
        {
            get { return toolTip; }
            set
            {
                if (toolTip != value)
                {
                    toolTip = value;
                    OnPropertyChanged(nameof(ToolTip));
                }
            }
        }

        public string Warning
        {
            get { return warning; }
            set
            {
                if (warning != value)
                {
                    warning = value;
                    OnPropertyChanged(nameof(Warning));
                }
            }
        }

        public int Id
        {
            get { return id; }
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string FullName
        {
            get { return fullName; }
            set
            {
                if (fullName != value)
                {
                    fullName = value;
                    OnPropertyChanged(nameof(FullName));
                }
            }
        }

        public bool IsCollapsed
        {
            get { return isCollapsed; }
            set
            {
                if (isCollapsed != value)
                {
                    isCollapsed = value;
                    OnPropertyChanged(nameof(IsCollapsed));
                }
            }
        }

        public bool IsBreakPointSet
        {
            get { return isBreakPointSet; }
            set
            {
                if (isBreakPointSet != value)
                {
                    isBreakPointSet = value;
                    OnPropertyChanged(nameof(IsBreakPointSet));
                }
            }
        }

        public Item[] Items
        {
            get
            {
                return [.. items];
            }
        }

        public Item[] InEvents
        {
            get
            {
                return [.. items.Where(i => i.Type == "event" && !i.IsOut)];
            }
        }

        public Item[] OutEvents
        {
            get
            {
                return [.. items.Where(i => i.Type == "event" && i.IsOut)];
            }
        }

        public Item[] InProperties
        {
            get
            {
                return [.. items.Where(i => i.Type != "event" && !i.IsOut)];
            }
        }

        public Item[] OutProperties
        {
            get
            {
                return [.. items.Where(i => i.Type != "event" && i.IsOut)];
            }
        }

        public Item[] InItems
        {
            get
            {
                return [.. items.Where(i => !i.IsOut)];
            }
        }

        public Item[] OutItems
        {
            get
            {
                return [.. items.Where(i => i.IsOut)];
            }
        }

        public bool HasBoth
        {
            get
            {
                return items.Any(item => item.IsOut) && items.Any(item => !item.IsOut);
            }
        }

        public bool CanChange
        {
            get { return canChange; }
            set
            {
                if (canChange != value)
                {
                    canChange = value;
                    OnPropertyChanged(nameof(CanChange));
                }
            }
        }

        internal Comment Comment
        {
            get { return comment; }
            set
            {
                if (comment != value)
                {
                    comment = value;
                    OnPropertyChanged(nameof(Comment));
                }
            }
        }

        public void AddItem(Item item)
        {
            item.Source = this;
            items.Add(item);
            item.ConnectionChanged += Item_ConnectionChanged;
            item.PropertyChanged += Item_PropertyChanged;
        }

        void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsPropertyValueShown":
                    UpdateCanChange();
                    break;
            }
        }

        void Item_ConnectionChanged(object sender, EventArgs e)
        {
            UpdateCanChange();
        }

        public void AddSameNameSource(AssemblyAndType asmAndType)
        {
            sameNameSources.Add(asmAndType);
            UpdateCanChange();
        }

        public Item GetInPropertyByName(string name)
        {
            return InProperties.FirstOrDefault(i => i.Name == name);
        }

        private void UpdateCanChange()
        {
            bool hasConnection = InProperties.Concat(InEvents).Any(i => i.InConnection != null) || OutProperties.Concat(OutEvents).Any(o => o.OutConnections.Length > 0);
            var hasPropertiedItem = InProperties.Any(i => i.IsPropertyValueShown);
            CanChange = sameNameSources.Count > 1 && !hasConnection && !hasPropertiedItem;
        }

        public AssemblyAndType GetNextAsm()
        {
            var index = sameNameSources.IndexOf(currentAsm);
            index++;
            if (index >= sameNameSources.Count)
            {
                index = 0;
            }
            return sameNameSources[index];
        }

        public AssemblyAndType GetPreviousAsm()
        {
            var index = sameNameSources.IndexOf(currentAsm);
            index--;
            if (index < 0)
            {
                index = sameNameSources.Count - 1;
            }
            return sameNameSources[index];
        }
    }
}
