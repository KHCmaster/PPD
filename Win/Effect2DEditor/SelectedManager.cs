using Effect2D;
using System;

namespace Effect2DEditor
{
    public class SelectedManager
    {
        public delegate void StateChangeEventHandler(object sender, StateChangeEventArgs e);
        public event StateChangeEventHandler StateChanged;
        public event EventHandler EffectChanged;
        public event EventHandler SetChanged;
        IEffect selectedeffect;
        EffectStateStructure selectedstate;
        EffectStateRatioSet selectedset;
        public SelectedManager(EffectManager effectManager)
        {
            EffectManager = effectManager;
        }
        public bool IgnoreEvent
        {
            get;
            set;
        }
        public EffectManager EffectManager
        {
            get;
            private set;
        }
        private IEffect LastEffect
        {
            get;
            set;
        }
        public IEffect Effect
        {
            get { return selectedeffect; }
            set
            {
                LastEffect = selectedeffect;
                selectedeffect = value;
                if (EffectChanged != null && !IgnoreEvent)
                {
                    EffectChanged.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public EffectStateStructure State
        {
            get { return selectedstate; }
            set
            {
                int lastitemindex = -1;
                int laststateindex = -1;
                int itemindex = -1;
                int stateindex = -1;
                GetItemAndStateIndex(LastEffect, out lastitemindex, out laststateindex);
                selectedstate = value;
                GetItemAndStateIndex(Effect, out itemindex, out stateindex);
                if (StateChanged != null && !IgnoreEvent)
                {
                    StateChanged.Invoke(this, new StateChangeEventArgs(lastitemindex, laststateindex, itemindex, stateindex));
                }
            }
        }
        public EffectStateRatioSet Set
        {
            get { return selectedset; }
            set
            {
                selectedset = value;
                if (SetChanged != null && !IgnoreEvent)
                {
                    SetChanged.Invoke(this, EventArgs.Empty);
                }
            }
        }
        private void GetItemAndStateIndex(IEffect effect, out int itemindex, out int stateindex)
        {
            itemindex = -1;
            stateindex = -1;
            if (effect != null && selectedstate != null)
            {
                itemindex = EffectManager.Effects.IndexOf(effect);
                if (itemindex < 0 || itemindex >= EffectManager.Effects.Count) return;
                for (int i = 0; i < EffectManager.Effects[itemindex].Sets.Count; i++)
                {
                    if (EffectManager.Effects[itemindex].Sets.Values[i].StartState == selectedstate)
                    {
                        stateindex = 0;
                        break;
                    }
                    else if (EffectManager.Effects[itemindex].Sets.Values[i].EndState == selectedstate)
                    {
                        stateindex = i + 1;
                        break;
                    }
                }
            }
        }
    }
    public class StateChangeEventArgs : EventArgs
    {
        public StateChangeEventArgs(int lastitemindex, int laststateindex, int itemindex, int stateindex)
        {
            LastItemIndex = lastitemindex;
            LastStateIndex = laststateindex;
            ItemIndex = itemindex;
            StateIndex = stateindex;
        }
        public int LastItemIndex
        {
            get;
            private set;
        }
        public int LastStateIndex
        {
            get;
            private set;
        }
        public int ItemIndex
        {
            get;
            private set;
        }
        public int StateIndex
        {
            get;
            private set;
        }
    }
}
