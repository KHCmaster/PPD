using PPDEditor.Command.PPDSheet;
using PPDEditorCommon;
using System.Collections.Generic;
using System.Linq;

namespace PPDEditor.FlowScript
{
    class Layer : ILayer
    {
        private readonly ChangeMarkPropertyManager manager;

        public Layer(PPDSheet sheet, IDProvider idProvider, bool isSelected)
        {
            var marks = sheet.GetSortedData().Select(m => new EditorMarkInfo(m, this)).ToArray();
            var selectedMarks = new HashSet<Mark>(sheet.GetAreaData());
            if (selectedMarks.Count == 0)
            {
                var selectedMark = sheet.SelectedMark;
                if (selectedMark == null)
                {
                    SelectedMark = null;
                }
                else
                {
                    SelectedMark = marks.FirstOrDefault(m => m.Mark == selectedMark);
                }
                SelectedMarks = [];
            }
            else
            {
                SelectedMarks = [.. marks.Where(m => selectedMarks.Contains(m.Mark))];
            }
            Marks = marks;

            manager = new ChangeMarkPropertyManager(sheet, this, idProvider);
            IsSelected = isSelected;
        }

        public void Execute()
        {
            manager.Execute();
        }

        #region ILayer メンバー

        public IEditorMarkInfo[] SelectedMarks
        {
            get;
            private set;
        }

        public IEditorMarkInfo[] Marks
        {
            get;
            private set;
        }

        public IEditorMarkInfo SelectedMark
        {
            get;
            private set;
        }

        public IChangeMarkPropertyManager ChangeMarkPropertyManager
        {
            get
            {
                return manager;
            }
        }

        public bool IsSelected
        {
            get;
            private set;
        }

        #endregion
    }
}
