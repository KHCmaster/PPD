using SharpDX;
using System;

namespace PPDEditor.Command.PPDSheet
{
    class SetDataInSelectionCommand : Command
    {
        float[][] lastInfo;
        Vector2[] positions;
        float[] angles;
        bool reverse;
        bool withAngle;

        Mark[] mks;

        public SetDataInSelectionCommand(Mark[] mks, Vector2[] positions, float[] angles, bool reverse, bool withAngle)
        {
            this.mks = mks;
            this.positions = positions;
            this.angles = angles;
            this.reverse = reverse;
            this.withAngle = withAngle;

            lastInfo = new float[Math.Min(mks.Length, positions.Length)][];
            int iter;
            if (!reverse)
            {
                iter = 0;
                for (int i = 0; i < positions.Length; i++)
                {
                    if (withAngle)
                    {
                        lastInfo[i] = new float[] { mks[iter].Position.X, mks[iter].Position.Y, mks[iter].Rotation };
                    }
                    else
                    {
                        lastInfo[i] = new float[] { mks[iter].Position.X, mks[iter].Position.Y };
                    }
                    iter++;
                    if (iter >= mks.Length) break;
                }
            }
            else
            {
                iter = mks.Length - 1;
                for (int i = 0; i < positions.Length; i++)
                {
                    if (withAngle)
                    {
                        lastInfo[i] = new float[] { mks[iter].Position.X, mks[iter].Position.Y, mks[iter].Rotation };
                    }
                    else
                    {
                        lastInfo[i] = new float[] { mks[iter].Position.X, mks[iter].Position.Y };
                    }
                    iter--;
                    if (iter < 0) break;
                }
            }
        }
        public override void Execute()
        {
            int iter;
            if (!reverse)
            {
                iter = 0;
                for (int i = 0; i < positions.Length; i++)
                {
                    mks[iter].Position = positions[i];
                    if (withAngle)
                    {
                        mks[iter].Rotation = angles[i];
                    }
                    iter++;
                    if (iter >= mks.Length) break;
                }
            }
            else
            {
                iter = mks.Length - 1;
                for (int i = 0; i < positions.Length; i++)
                {
                    mks[iter].Position = positions[i];
                    if (withAngle)
                    {
                        mks[iter].Rotation = angles[i];
                    }
                    iter--;
                    if (iter < 0) break;
                }
            }
        }

        public override void Undo()
        {
            int iter;
            if (!reverse)
            {
                iter = 0;
                for (int i = 0; i < positions.Length; i++)
                {
                    if (withAngle)
                    {
                        mks[iter].Rotation = lastInfo[i][2];
                    }
                    mks[iter].Position = new Vector2(lastInfo[i][0], lastInfo[i][1]);
                    iter++;
                    if (iter >= mks.Length) break;
                }
            }
            else
            {
                iter = mks.Length - 1;
                for (int i = 0; i < positions.Length; i++)
                {
                    if (withAngle)
                    {
                        mks[iter].Rotation = lastInfo[i][2];
                    }
                    mks[iter].Position = new Vector2(lastInfo[i][0], lastInfo[i][1]);
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
