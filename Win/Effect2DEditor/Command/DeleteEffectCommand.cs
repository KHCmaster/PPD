using Effect2D;
using System;

namespace Effect2DEditor.Command
{
    class DeleteEffectCommand : CommandBase
    {
        int deleteindex;
        IEffect ie;
        public DeleteEffectCommand(EffectManager manager, string exp, int deleteindex)
            : base(manager, exp)
        {
            this.deleteindex = deleteindex;
        }
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
