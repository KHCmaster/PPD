using SharpDX;
using System;
using System.Drawing;

namespace PPDEditor.Command.PPDSheet
{
    class ApplyTransCommand : Command
    {
        private float[][] lastState;
        private Mark[] mks;
        private Vector2[] poses;
        private Vector2[] dirs;
        private bool applyRotation;
        public ApplyTransCommand(Mark[] mks, Vector2[] poses, Vector2[] dirs, bool applyRotation)
        {
            this.mks = mks;
            this.poses = new Vector2[poses.Length];
            this.dirs = new Vector2[dirs.Length];
            this.applyRotation = applyRotation;
            Array.Copy(poses, this.poses, poses.Length);
            Array.Copy(dirs, this.dirs, dirs.Length);
            lastState = new float[mks.Length][];
            for (int i = 0; i < mks.Length; i++)
            {
                lastState[i] = new float[] { mks[i].Position.X, mks[i].Position.Y, mks[i].Rotation };
            }
        }


        public bool CheckSameData(Mark[] mks, bool applyRotation)
        {
            if (this.applyRotation != applyRotation) return false;
            if (this.mks.Length != mks.Length) return false;
            for (int i = 0; i < mks.Length; i++)
            {
                if (this.mks[i] != mks[i])
                {
                    return false;
                }
            }
            return true;
        }

        public Vector2[] Poses
        {
            set
            {
                poses = value;
            }
        }

        public Vector2[] Dirs
        {
            set
            {
                dirs = value;
            }
        }

        public override void Execute()
        {
            var minnum = Math.Min(Math.Min(mks.Length, poses.Length), dirs.Length);
            for (int i = 0; i < minnum; i++)
            {
                mks[i].Position = TransPos(poses[i]);
                if (applyRotation)
                {
                    mks[i].Rotation = GetRotation(new PointF(-dirs[i].X, dirs[i].Y));
                }
            }
        }

        public override void Undo()
        {
            for (int i = 0; i < mks.Length; i++)
            {
                mks[i].Position = new SharpDX.Vector2(lastState[i][0], lastState[i][1]);
                if (applyRotation)
                {
                    mks[i].Rotation = lastState[i][2];
                }
            }
        }


        private float GetRotation(PointF p)
        {
            float ret = 0;
            if (p.Y < 0)
            {
                ret += (float)Math.PI;
                ret += (float)(Math.PI - Math.Acos(p.X));
            }
            else
            {
                ret += (float)Math.Acos(p.X);
            }
            return ret;
        }

        private SharpDX.Vector2 TransPos(Vector2 pos)
        {
            //in this function to expand (400,225) to (800,450)
            //and also check is inside (800,450)
            float x = 2 * (pos.X - 100);
            float y = 2 * (pos.Y - 50);
            if (x <= 0) x = 0;
            if (x >= 800) x = 800;
            if (y <= 0) y = 0;
            if (y >= 450) y = 450;
            return new SharpDX.Vector2(x, y);
        }

        public override CommandType CommandType
        {
            get { return CommandType.None; }
        }
    }
}
