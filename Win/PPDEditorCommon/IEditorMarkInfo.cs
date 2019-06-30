using PPDCoreModel;

namespace PPDEditorCommon
{
    public interface IEditorMarkInfo : IMarkInfo
    {
        ILayer Layer
        {
            get;
        }
    }
}
