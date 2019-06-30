namespace PPDEditorCommon
{
    public interface ILayer
    {
        IEditorMarkInfo[] SelectedMarks
        {
            get;
        }

        IEditorMarkInfo[] Marks
        {
            get;
        }

        IEditorMarkInfo SelectedMark
        {
            get;
        }

        IChangeMarkPropertyManager ChangeMarkPropertyManager
        {
            get;
        }

        bool IsSelected
        {
            get;
        }
    }
}
