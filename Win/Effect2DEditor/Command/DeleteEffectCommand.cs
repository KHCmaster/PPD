using Effect2D;
using System;

namespace Effect2DEditor.Command
{
    class DeleteEffectCommand(EffectManager manager, string exp, int deleteindex) : CommandBase(manager, exp)
    {
        readonly int deleteindex = deleteindex;
        IEffect ie;

        public override void Undo()
        {
            manager.Effects.Insert(deleteindex, ie);
        }
        public override void Execute()
        {
            try
            {
                ie = manager.Effects[deleteindex];
                manager.Effects.RemoveAt(deleteindex);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to delete effect", e);
            }
        }
    }
}
