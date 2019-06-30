using System;
using System.Collections.Generic;

namespace FlowScriptDrawControl.Model
{
    public class SelectableManager
    {
        List<Selectable> selectedSelectables;

        public event Action SelectionChanged;

        public SelectableManager()
        {
            selectedSelectables = new List<Selectable>();
        }

        public Selectable[] SelectedSelectables
        {
            get
            {
                return selectedSelectables.ToArray();
            }
        }

        public void Select(Selectable selectable)
        {
            if (selectedSelectables.Count != 1 || selectedSelectables[0] != selectable)
            {
                ClearSelect();
                selectable.IsSelected = true;
                selectedSelectables.Add(selectable);
                OnSelectionChanged();
            }
        }

        public void AddSelect(Selectable selectable)
        {
            if (!selectedSelectables.Contains(selectable))
            {
                selectable.IsSelected = true;
                selectedSelectables.Add(selectable);
                OnSelectionChanged();
            }
        }

        public void RemoveSelect(Selectable selectable)
        {
            if (selectedSelectables.Contains(selectable))
            {
                selectedSelectables.Remove(selectable);
                selectable.IsSelected = false;
                OnSelectionChanged();
            }
        }

        public void ClearSelect()
        {
            if (selectedSelectables.Count > 0)
            {
                foreach (Selectable s in selectedSelectables)
                {
                    s.IsSelected = false;
                }
                selectedSelectables.Clear();
                OnSelectionChanged();
            }
        }

        private void OnSelectionChanged()
        {
            SelectionChanged?.Invoke();
        }
    }
}
