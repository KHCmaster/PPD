using FlowScriptEngine;
using PPDFramework;

namespace PPDCoreModel
{
    public class CreateMarkManager : TemplateManager<ICreateMark>
    {
        public CreateMarkManager(Engine engine)
            : base(engine)
        {

        }

        public bool CreateMark(IMarkInfo markInfo, out GameComponent mark, out GameComponent colorMark,
            out GameComponent axis, out GameComponent slideMark, out GameComponent slideColorMark, out PictureObject trace)
        {
            mark = colorMark = axis = slideMark = slideColorMark = trace = null;
            var handled = false;
            foreach (ICreateMark create in list)
            {
                if (!handled || (handled && create.IsEvaluateRequired()))
                {
                    create.Create(markInfo);
                    engine.Update();
                }

                if (!handled && create.EvaluateHandled)
                {
                    mark = create.CreatedMark;
                    colorMark = create.CreatedColorMark;
                    axis = create.CreatedAxis;
                    slideMark = create.CreatedSlideMark;
                    slideColorMark = create.CreatedSlideColorMark;
                    trace = create.CreatedTrace;
                    handled = true;
                }
            }
            return handled;
        }
    }
}
