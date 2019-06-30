using PPDFramework;

namespace PPDCoreModel
{
    public interface ICreateMark : IEvaluate
    {
        void Create(IMarkInfo markInfo);
        GameComponent CreatedMark { get; }
        GameComponent CreatedColorMark { get; }
        GameComponent CreatedAxis { get; }
        GameComponent CreatedSlideMark { get; }
        GameComponent CreatedSlideColorMark { get; }
        PictureObject CreatedTrace { get; }
    }
}
