using GalaSoft.MvvmLight;
using System.ComponentModel;
using System.Windows.Media;

namespace FlowScriptDrawControl.Model
{
    class Arrow : ObservableObject
    {
        private PathGeometry geometry;
        private Brush stroke;
        private Item srcItem;
        private Item destItem;

        public PathGeometry Geometry
        {
            get { return geometry; }
            set
            {
                if (geometry != value)
                {
                    geometry = value;
                    RaisePropertyChanged("Geometry");
                }
            }
        }

        public Brush Stroke
        {
            get { return stroke; }
            private set
            {
                if (stroke != value)
                {
                    stroke = value;
                    RaisePropertyChanged("Stroke");
                }
            }
        }

        public Item SrcItem
        {
            get { return srcItem; }
            set
            {
                if (srcItem != value)
                {
                    if (srcItem != null)
                    {
                        srcItem.PropertyChanged -= srcItem_PropertyChanged;
                    }
                    srcItem = value;
                    if (srcItem != null)
                    {
                        srcItem.PropertyChanged += srcItem_PropertyChanged;
                    }
                    RaisePropertyChanged("SrcItem");
                    UpdateStroke();
                }
            }
        }

        public Item DestItem
        {
            get { return destItem; }
            set
            {
                if (destItem != value)
                {
                    if (destItem != null)
                    {
                        destItem.PropertyChanged -= destItem_PropertyChanged;
                    }
                    destItem = value;
                    if (destItem != null)
                    {
                        destItem.PropertyChanged += destItem_PropertyChanged;
                    }
                    RaisePropertyChanged("DestItem");
                }
            }
        }

        public void UpdateStroke()
        {
            bool isMouseOver = (false || (srcItem != null && srcItem.IsMouseOver)) || (destItem != null && destItem.IsMouseOver);
            Color color;
            if (isMouseOver)
            {
                color = (Color)ColorConverter.ConvertFromString("#FFC72F");
            }
            else
            {
                var type = srcItem != null ? srcItem.Type : (destItem?.Type);
                color = type == "event" ? (Color)ColorConverter.ConvertFromString("#ADFF2F") : (Color)ColorConverter.ConvertFromString("#87CEFA");
            }
            Stroke = new SolidColorBrush(color);
        }

        void srcItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsMouseOver")
            {
                UpdateStroke();
            }
        }

        void destItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsMouseOver")
            {
                UpdateStroke();
            }
        }
    }
}
