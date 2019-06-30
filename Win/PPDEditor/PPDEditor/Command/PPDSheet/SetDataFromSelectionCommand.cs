using SharpDX;
using System;
using System.Collections.Generic;

namespace PPDEditor.Command.PPDSheet
{
    class SetDataFromSelectionCommand : Command
    {
        float[][] lastInfo;
        Vector2[] positions;
        float[] angles;
        bool reverse;
        bool withAngle;

        SortedList<float, Mark>[] data;
        Mark selectedMark;

        public SetDataFromSelectionCommand(SortedList<float, Mark>[] data, Vector2[] positions, float[] angles, bool reverse, bool withAngle, Mark selectedMark)
        {
            this.data = data;
            this.positions = positions;
            this.angles = angles;
            this.reverse = reverse;
            this.withAngle = withAngle;
            this.selectedMark = selectedMark;

            var iter = data[(int)selectedMark.Type].Values.IndexOf(selectedMark);
            if (!reverse)
            {
                lastInfo = new float[Math.Min(positions.Length, data[(int)selectedMark.Type].Count - iter)][];
                for (int i = 0; i < positions.Length; i++)
                {
                    Mark mk = data[(int)selectedMark.Type].Values[iter];
                    if (withAngle)
                    {
                        lastInfo[i] = new float[] { mk.Position.X, mk.Position.Y, mk.Rotation };
                    }
                    else
                    {
                        lastInfo[i] = new float[] { mk.Position.X, mk.Position.Y };
                    }
                    iter++;
                    if (iter >= data[(int)selectedMark.Type].Count) break;
                }
            }
            else
            {
                lastInfo = new float[Math.Min(positions.Length, iter + 1)][];
                for (int i = 0; i < positions.Length; i++)
                {
                    Mark mk = data[(int)selectedMark.Type].Values[iter];
                    if (withAngle)
                    {
                        lastInfo[i] = new float[] { mk.Position.X, mk.Position.Y, mk.Rotation };
                    }
                    else
                    {
                        lastInfo[i] = new float[] { mk.Position.X, mk.Position.Y };
                    }
                    iter--;
                    if (iter < 0) break;
                }
            }
        }

        public override void Execute()
        {
            var iter = data[(int)selectedMark.Type].Values.IndexOf(selectedMark);
            if (!reverse)
            {
                for (int i = 0; i < positions.Length; i++)
                {
                    Mark mk = data[(int)selectedMark.Type].Values[iter];
                    mk.Position = positions[i];
                    if (withAngle)
                    {
                        mk.Rotation = angles[i];
                    }
                    iter++;
                    if (iter >= data[(int)selectedMark.Type].Count) break;
                }
            }
            else
            {
                for (int i = 0; i < positions.Length; i++)
                {
                    Mark mk = data[(int)selectedMark.Type].Values[iter];
                    mk.Position = positions[i];
                    if (withAngle)
                    {
                        mk.Rotation = angles[i];
                    }
                    iter--;
                    if (iter < 0) break;
                }
            }
        }

        public override void Undo()
        {
            var iter = data[(int)selectedMark.Type].Values.IndexOf(selectedMark);
            if (!reverse)
            {
                for (int i = 0; i < positions.Length; i++)
                {
                    Mark mk = data[(int)selectedMark.Type].Values[iter];
                    if (withAngle)
                    {
                        mk.Rotation = lastInfo[i][2];
                    }
                    mk.Position = new Vector2(lastInfo[i][0], lastInfo[i][1]);
                    iter++;
                    if (iter >= data[(int)selectedMark.Type].Count) break;
                }
            }
            else
            {
                for (int i = 0; i < positions.Length; i++)
                {
                    Mark mk = data[(int)selectedMark.Type].Values[iter];
                    if (withAngle)
                    {
                        mk.Rotation = lastInfo[i][2];
                    }
                    mk.Position = new Vector2(lastInfo[i][0], lastInfo[i][1]);
                    iter--;
                    if (iter < 0) break;
                }
            }
        }

        public override CommandType CommandType
        {
            get { return CommandType.Pos; }
        }
    }
}
