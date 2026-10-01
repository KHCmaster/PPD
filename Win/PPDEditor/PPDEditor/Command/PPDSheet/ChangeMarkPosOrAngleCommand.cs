using System.Numerics;
using PPDMath;

namespace PPDEditor.Command.PPDSheet
{
    class ChangeMarkPosOrAngleCommand(Mark mk, ChangeMarkPosOrAngleCommand.TransType transType, Vector2 position, float rotation) : Command
    {
        public enum TransType
        {
            Position = 0,
            Rotation = 1
        }

        private readonly Mark mk = mk;
        private readonly TransType transType = transType;
        private Vector2 position = position;
        private float rotation = rotation;
        private Vector2 lastPosition = mk.Position;
        private readonly float lastRotation = mk.Rotation;

        public Mark Mark
        {
            get
            {
                return mk;
            }
        }

        public TransType TType
        {
            get
            {
                return transType;
            }
        }

        public Vector2 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }

        public float Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;
            }
        }

        public override void Execute()
        {
            switch (transType)
            {
                case TransType.Position:
                    mk.Position = position;
                    break;
                case TransType.Rotation:
                    mk.Rotation = rotation;
                    break;
            }
        }

        public override void Undo()
        {
            switch (transType)
            {
                case TransType.Position:
                    mk.Position = lastPosition;
                    break;
                case TransType.Rotation:
                    mk.Rotation = lastRotation;
                    break;
            }
        }

        public override CommandType CommandType
        {
            get { return CommandType.Pos; }
        }
    }
}
