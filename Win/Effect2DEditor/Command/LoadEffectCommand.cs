using Effect2D;

namespace Effect2DEditor.Command
{
    class LoadEffectCommand(EffectManager manager, string exp, int insertindex, string filename) : CommandBase(manager, exp)
    {
        readonly int insertindex = insertindex;
        readonly string filename = filename;

        public override void Undo()
        {
            manager.Effects.RemoveAt(insertindex);
        }

        public override void Execute()
        {
            var em = EffectLoader.Load(filename, Utility.ValidateImage);
            var be = new BaseEffect
            {
                Filename = filename
            };
            be.SetDefault();
            be.Effects = em.Effects;
            be.FPS = em.FPS;
            manager.Effects.Insert(insertindex, be);
        }
    }
}
