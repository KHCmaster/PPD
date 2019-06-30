using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WeifenLuo.WinFormsUI.Docking
{
    public class DockWindowCollection : ReadOnlyCollection<DockWindow>
    {
        internal DockWindowCollection(DockPanel dockPanel)
            : base(new List<DockWindow>())
        {
            Items.Add(new DockWindow(dockPanel, DockState.Document));
            Items.Add(new DockWindow(dockPanel, DockState.DockLeft));
            Items.Add(new DockWindow(dockPanel, DockState.DockRight));
            Items.Add(new DockWindow(dockPanel, DockState.DockTop));
            Items.Add(new DockWindow(dockPanel, DockState.DockBottom));
        }

        public DockWindow this[DockState dockState]
        {
            get
            {
                switch (dockState)
                {
                    case DockState.Document:
                        return Items[0];
                    case DockState.DockLeft:
                    case DockState.DockLeftAutoHide:
                        return Items[1];
                    case DockState.DockRight:
                    case DockState.DockRightAutoHide:
                        return Items[2];
                    case DockState.DockTop:
                    case DockState.DockTopAutoHide:
                        return Items[3];
                    case DockState.DockBottom:
                    case DockState.DockBottomAutoHide:
                        return Items[4];
                }

                throw (new ArgumentOutOfRangeException());
            }
        }
    }
}
