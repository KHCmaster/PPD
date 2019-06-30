using System.Windows.Media;

namespace FlowScriptDrawControl.Model
{
    public class Scope : Positionable
    {
        private int id;
        private Color color;
        private Brush brush;
        private Scope parent;
        private int parentScopeId;

        public int Id
        {
            get { return id; }
            set
            {
                if (id != value)
                {
                    id = value;
                    RaisePropertyChanged("Id");
                }
            }
        }

        public Color Color
        {
            get { return color; }
            set
            {
                if (color != value)
                {
                    color = value;
                    RaisePropertyChanged("Color");
                    Brush = new SolidColorBrush(Color);
                }
            }
        }

        public Brush Brush
        {
            get { return brush; }
            private set
            {
                if (brush != value)
                {
                    brush = value;
                    RaisePropertyChanged("Brush");
                }
            }
        }

        public Scope Parent
        {
            get { return parent; }
            set
            {
                if (parent != value)
                {
                    parent = value;
                    RaisePropertyChanged("Parent");
                }
            }
        }

        internal int ParentScopeId
        {
            get
            {
                if (parent == null)
                {
                    return parentScopeId;
                }
                return parent.Id;
            }
            set
            {
                parentScopeId = value;
            }
        }

        public bool IsParent(Scope scope)
        {
            Scope temp = parent;
            while (temp != null)
            {
                if (temp == scope)
                {
                    return true;
                }
                temp = temp.parent;
            }
            return false;
        }

        public Scope GetTop()
        {
            Scope temp = this;
            while (temp.parent != null)
            {
                temp = temp.parent;
            }
            return temp;
        }
    }
}
