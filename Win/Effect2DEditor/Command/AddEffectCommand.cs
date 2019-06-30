using Effect2D;
using System;
using System.Collections.Generic;
using System.Drawing;


namespace Effect2DEditor.Command
{
    class AddEffectCommand : CommandBase
    {
        int insertindex;
        string filename;
        SortedList<string, Image> imagepool;
        public AddEffectCommand(EffectManager manager, string exp, int insertindex, string filename, SortedList<string, Image> imagepool)
            : base(manager, exp)
        {
            this.insertindex = insertindex;
            this.filename = filename;
            this.imagepool = imagepool;
        }
        public override void Undo()
        {
            manager.Effects.RemoveAt(insertindex);
        }
        public override void Execute()
        {
            try
            {
                var image = Image.FromFile(filename);
                if (!imagepool.ContainsKey(filename)) imagepool.Add(filename, image);
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
