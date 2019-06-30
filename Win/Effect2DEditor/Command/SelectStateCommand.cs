using Effect2D;

namespace Effect2DEditor.Command
{
    class SelectStateCommand : CommandBase
    {
        int lastitemindex;
        int itemindex;
        int laststateindex;
        int stateindex;
        SelectedManager selectedmanager;
        public SelectStateCommand(EffectManager manager, string exp, SelectedManager selectedmanager, int lastitemindex, int itemindex, int laststateindex, int stateindex)
            : base(manager, exp)
        {
            this.lastitemindex = lastitemindex;
            this.itemindex = itemindex;
            this.laststateindex = laststateindex;
            this.stateindex = stateindex;
            this.selectedmanager = selectedmanager;
        }
        public int ItemIndex
        {
            get
            {
                return itemindex;
            }
        }
        public int StateIndex
        {
            get
            {
                return stateindex;
            }
        }
        public override void Undo()
        {
            selectedmanager.IgnoreEvent = true;
            SetValues(lastitemindex, laststateindex);
            selectedmanager.IgnoreEvent = false;
        }

        public override void Execute()
        {
            selectedmanager.IgnoreEvent = true;
            SetValues(itemindex, stateindex);
            selectedmanager.IgnoreEvent = false;
        }

        private void SetValues(int itemindex, int stateindex)
        {
            if (itemindex == -1)
            {
                selectedmanager.Effect = null;
                selectedmanager.State = null;
                selectedmanager.Set = null;
            }
            else
            {
                selectedmanager.Effect = manager.Effects[itemindex];
                if (stateindex == -1)
                {
                    selectedmanager.State = null;
                    selectedmanager.Set = null;
                }
                else
                {
                    if (stateindex == 0)
                    {
                        selectedmanager.Set = manager.Effects[itemindex].Sets.Values[0];
                        selectedmanager.State = selectedmanager.Set.StartState;
                    }
                    else
                    {
                        selectedmanager.Set = manager.Effects[itemindex].Sets.Values[stateindex - 1];
                        selectedmanager.State = selectedmanager.Set.EndState;
                    }
                }
            }
        }
    }
}
