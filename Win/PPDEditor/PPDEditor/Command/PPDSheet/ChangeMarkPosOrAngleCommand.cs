using SharpDX;

namespace PPDEditor.Command.PPDSheet
{
    class ChangeMarkPosOrAngleCommand : Command
    {
        public enum TransType
        {
            Position = 0,
            Rotation = 1
        }

        private Mark mk;
        private TransType transType;
        private Vector2 position;
        private float rotation;
        private Vector2 lastPosition;
        private float lastRotation;

        public ChangeMarkPosOrAngleCommand(Mark mk, TransType transType, Vector2 position, float rotation)
        {
            this.mk = mk;
            this.transType = transType;
            this.position = position;
            this.rotation = rotation;
            lastPosition = mk.Position;
            lastRotation = mk.Rotation;
        }

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
