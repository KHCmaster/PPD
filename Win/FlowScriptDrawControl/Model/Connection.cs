using CommunityToolkit.Mvvm.ComponentModel;

namespace FlowScriptDrawControl.Model
{
    public class Connection : ObservableObject
    {
        private Item target;

        public Item Target
        {
            get { return target; }
            set
            {
                if (target != value)
                {
                    target = value;
                    OnPropertyChanged("Target");
                }
            }
        }
    }
}
