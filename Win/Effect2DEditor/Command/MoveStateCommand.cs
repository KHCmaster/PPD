using Effect2D;
using System.Collections.Generic;

namespace Effect2DEditor.Command
{
    class MoveStateCommand : CommandBase
    {
        int beforeframe;
        int newframe;
        TimeLine.KeyOperationMode opmode;
        SelectedManager selectedmanager;
        public MoveStateCommand(EffectManager manager, string exp, int beforeframe, int newframe, TimeLine.KeyOperationMode opmode, SelectedManager selectedmanager)
            : base(manager, exp)
        {
            this.beforeframe = beforeframe;
            this.newframe = newframe;
            this.opmode = opmode;
            this.selectedmanager = selectedmanager;
        }

        public override void Undo()
        {
            switch (opmode)
            {
                case TimeLine.KeyOperationMode.Move:
                    Move(beforeframe);
                    break;
                case TimeLine.KeyOperationMode.LeftMoveOnly:
                    LeftMoveOnly(beforeframe);
                    break;
                case TimeLine.KeyOperationMode.RightMoveOnly:
                    RightMoveOnly(beforeframe);
                    break;
            }
        }

        public override void Execute()
        {
            switch (opmode)
            {
                case TimeLine.KeyOperationMode.Move:
                    Move(newframe);
                    break;
                case TimeLine.KeyOperationMode.LeftMoveOnly:
                    LeftMoveOnly(newframe);
                    break;
                case TimeLine.KeyOperationMode.RightMoveOnly:
                    RightMoveOnly(newframe);
                    break;
            }
        }
        private void Move(int frame)
        {
            if (frame < 0) frame = 0;
            if (selectedmanager.Set.StartState == selectedmanager.State)
            {
                if (frame >= selectedmanager.Set.EndFrame) frame = selectedmanager.Set.EndFrame - 1;
                selectedmanager.Set.StartFrame = frame;
            }
            else
            {
                if (frame <= selectedmanager.Set.StartFrame) frame = selectedmanager.Set.StartFrame + 1;
                var selectedindex = selectedmanager.Effect.Sets.IndexOfValue(selectedmanager.Set);
                if (selectedindex >= 0 && selectedindex < selectedmanager.Effect.Sets.Count - 1)
                {
                    if (frame >= selectedmanager.Effect.Sets.Values[selectedindex + 1].EndFrame) frame = selectedmanager.Effect.Sets.Values[selectedindex + 1].EndFrame - 1;
                    EffectStateRatioSet temp = selectedmanager.Effect.Sets.Values[selectedindex + 1];
                    temp.StartFrame = frame;
                    selectedmanager.Effect.Sets.RemoveAt(selectedindex + 1);
                    selectedmanager.Effect.Sets.Add(temp.StartFrame, temp);
                }
                selectedmanager.Set.EndFrame = frame;
            }
            selectedmanager.Effect.CheckFrameLength();
        }
        private void LeftMoveOnly(int frame)
        {
            if (frame <= selectedmanager.Set.StartFrame) frame = selectedmanager.Set.StartFrame + 1;
            int diff = frame - selectedmanager.Set.EndFrame;
            if (diff != 0)
            {
                selectedmanager.Set.EndFrame += diff;
                var pool = new List<EffectStateRatioSet>();
                var removekey = new List<int>();
                bool found = false;
                foreach (KeyValuePair<int, EffectStateRatioSet> pair in selectedmanager.Effect.Sets)
                {
                    if (found)
                    {
                        pair.Value.StartFrame += diff;
                        pair.Value.EndFrame += diff;
                        pool.Add(pair.Value);
                        removekey.Add(pair.Key);
                    }
                    found |= pair.Value == selectedmanager.Set;
                }
                foreach (int val in removekey)
                {
                    selectedmanager.Effect.Sets.Remove(val);
                }
                foreach (EffectStateRatioSet set in pool)
                {
                    selectedmanager.Effect.Sets.Add(set.StartFrame, set);
                }
            }
            selectedmanager.Effect.CheckFrameLength();
        }
        private void RightMoveOnly(int frame)
        {
            if (frame < 0) frame = 0;
            int diff = frame - selectedmanager.Set.StartFrame; if (diff != 0)
            {
                var pool = new List<EffectStateRatioSet>(selectedmanager.Effect.Sets.Values);
                selectedmanager.Effect.Sets.Clear();
                foreach (EffectStateRatioSet set in pool)
                {
                    set.StartFrame += diff;
                    set.EndFrame += diff;
                }
                foreach (EffectStateRatioSet set in pool)
                {
                    selectedmanager.Effect.Sets.Add(set.StartFrame, set);
                }
            }
            selectedmanager.Effect.CheckFrameLength();
        }
    }
}
