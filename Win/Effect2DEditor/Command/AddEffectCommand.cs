using Effect2D;
using System;

namespace Effect2DEditor.Command
{
    class AddEffectCommand(EffectManager manager, string exp, int insertindex, string filename) : CommandBase(manager, exp)
    {
        readonly int insertindex = insertindex;
        readonly string filename = filename;

        public override void Undo()
        {
            manager.Effects.RemoveAt(insertindex);
        }
        public override void Execute()
        {
            try
            {
                Utility.ValidateImage(filename);
                var be = new BaseEffect
                {
                    Filename = filename
                };
                be.SetDefault();
                manager.Effects.Insert(insertindex, be);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to load image", e);
            }
        }
    }
}
