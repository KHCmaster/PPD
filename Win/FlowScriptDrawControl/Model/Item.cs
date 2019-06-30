using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace FlowScriptDrawControl.Model
{
    public class Item : ObservableObject
    {
        private string name;
        private string value;
        private string type;
        private string typeText;
        private Type propertyType;
        private Color typeColor = Colors.White;
        private bool isOut;
        private bool isCollapsed;
        private Source source;
        private string toolTip;
        private bool isConnectable;
        private bool isPropertyValueShown;
        private bool isMouseOver;

        private Connection inConnection;

        private List<Connection> outConnections;

        public event EventHandler ConnectionChanged;

        public Item(string[] replacedNames)
        {
            ReplacedNames = replacedNames;
            outConnections = new List<Connection>();
        }

        public bool IsMouseOver
        {
            get { return isMouseOver; }
            set
            {
                if (isMouseOver != value)
                {
                    isMouseOver = value;
                    RaisePropertyChanged("IsMouseOver");
                }
            }
        }

        public string[] ReplacedNames
        {
            get;
            private set;
        }

        public Connection InConnection
        {
            get { return inConnection; }
            set
            {
                inConnection = value;
                UpdateIsCollapsed();
                UpdateIsPropertyValueShown();
                OnConnectionChanged();
            }
        }

        public Connection[] OutConnections
        {
            get
            {
                return outConnections.ToArray();
            }
        }

        public string ToolTip
        {
            get { return toolTip; }
            set
            {
                if (toolTip != value)
                {
                    toolTip = value;
                    RaisePropertyChanged("ToolTip");
                }
            }
        }

        public Source Source
        {
            get { return source; }
            set
            {
                if (source != value)
                {
                    if (source != null)
                    {
                        source.PropertyChanged -= source_PropertyChanged;
                    }
                    source = value;
                    if (source != null)
                    {
                        source.PropertyChanged += source_PropertyChanged;
                    }
                    UpdateIsCollapsed();
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
                    RaisePropertyChanged("Name");
                }
            }
        }

        public string Value
        {
            get { return value; }
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    RaisePropertyChanged("Value");
                    UpdateIsPropertyValueShown();
                    UpdateIsCollapsed();
                }
            }
        }

        public string Type
        {
            get { return type; }
            set
            {
                if (type != value)
                {
                    type = value;
                    RaisePropertyChanged("Type");
                }
            }
        }

        public string TypeText
        {
            get { return typeText; }
            set
            {
                if (typeText != value)
                {
                    typeText = value;
                    RaisePropertyChanged("TypeText");
                }
            }
        }

        public Color TypeColor
        {
            get { return typeColor; }
            set
            {
                if (typeColor != value)
                {
                    typeColor = value;
                    RaisePropertyChanged("TypeColor");
                }
            }
        }

        public bool IsOut
        {
            get { return isOut; }
            set
            {
                if (isOut != value)
                {
                    isOut = value;
                    RaisePropertyChanged("IsOut");
                    UpdateIsCollapsed();
                }
            }
        }

        public bool IsCollapsed
        {
            get { return isCollapsed; }
            private set
            {
                if (isCollapsed != value)
                {
                    isCollapsed = value;
                    RaisePropertyChanged("IsCollapsed");
                }
            }
        }

        public bool IsConnectable
        {
            get { return isConnectable; }
            set
            {
                if (isConnectable != value)
                {
                    isConnectable = value;
                    RaisePropertyChanged("IsConnectable");
                }
            }
        }

        public Type PropertyType
        {
            get { return propertyType; }
            set
            {
                if (propertyType != value)
                {
                    propertyType = value;
                    RaisePropertyChanged("PropertyType");
                }
            }
        }

        public bool IsPropertyValueShown
        {
            get { return isPropertyValueShown; }
            private set
            {
                if (isPropertyValueShown != value)
                {
                    isPropertyValueShown = value;
                    RaisePropertyChanged("IsPropertyValueShown");
                }
            }
        }

        public void AddOutConnection(Connection connection)
        {
            outConnections.Add(connection);
            UpdateIsCollapsed();
            OnConnectionChanged();
        }

        public void RemoveOutConnection(Connection connection)
        {
            outConnections.Remove(connection);
            UpdateIsCollapsed();
            OnConnectionChanged();
        }

        public void RemoveOutConnection(Item destItem)
        {
            var connection = outConnections.FirstOrDefault(c => c.Target == destItem);
            outConnections.Remove(connection);
            OnConnectionChanged();
        }

        private void UpdateIsCollapsed()
        {
            if (IsOut)
            {
                IsCollapsed = Source != null && Source.IsCollapsed && outConnections.Count == 0;
            }
            else
            {
                IsCollapsed = Source != null && Source.IsCollapsed && Value == null && InConnection == null;
            }
        }

        private void UpdateIsPropertyValueShown()
        {
            IsPropertyValueShown = InConnection == null && Value != null;
        }

        void source_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsCollapsed")
            {
                UpdateIsCollapsed();
            }
        }

        protected void OnConnectionChanged()
        {
            ConnectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public Item Clone()
        {
            return new Item(ReplacedNames)
            {
                Name = Name,
                Value = Value,
                ToolTip = ToolTip,
                Type = Type,
                PropertyType = PropertyType,
                TypeText = TypeText,
                TypeColor = TypeColor,
                IsOut = IsOut
            };
        }
    }
}
