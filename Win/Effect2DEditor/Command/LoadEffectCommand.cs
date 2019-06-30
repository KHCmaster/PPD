using Effect2D;
using System.Collections.Generic;
using System.Drawing;

namespace Effect2DEditor.Command
{
    class LoadEffectCommand : CommandBase
    {
        int insertindex;
        string filename;
        SortedList<string, Image> ImagePool;
        public LoadEffectCommand(EffectManager manager, string exp, int insertindex, string filename, SortedList<string, Image> ImagePool)
            : base(manager, exp)
        {
            this.filename = filename;
            this.insertindex = insertindex;
            this.ImagePool = ImagePool;
        }

        public override void Undo()
        {
            manager.Effects.RemoveAt(insertindex);
        }

        public override void Execute()
        {
            var em = EffectLoader.Load(filename, (fn) =>
                {
                    var image = Image.FromFile(fn);
                    if (!ImagePool.ContainsKey(fn)) ImagePool.Add(fn, image);
                }
            );
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
