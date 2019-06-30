using PPDEditorCommon;
using SharpDX;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace PPDEditor
{
    class PosAndAngleInfo : IPosAndAngle
    {
        enum Mode
        {
            X,
            Y,
            Angle,
        }

        public Vector2? Position
        {
            get;
            private set;
        }

        public float? Rotation
        {
            get;
            private set;
        }

        public PosAndAngleInfo(Vector2 position)
        {
            Position = position;
        }

        public PosAndAngleInfo(float angle)
        {
            Rotation = angle;
        }

        public PosAndAngleInfo(Vector2 position, float angle)
        {
            Position = position;
            Rotation = angle;
        }

        public static PosAndAngleInfo[] Load(string filePath)
        {
            var str = File.ReadAllText(filePath).Replace("\r\n", "\n").Replace("\r", "\n");
            var splits = str.Split('\n');
            if (splits.Length < 2)
            {
                throw new InvalidDataException("");
            }
            var header = splits[0].Split(',');
            Mode[] modes = new Mode[header.Length];
            var iter = 0;
            foreach (var h in header)
            {
                switch (h)
                {
                    case "x":
                        modes[iter] = Mode.X;
                        break;
                    case "y":
                        modes[iter] = Mode.Y;
                        break;
                    case "angle":
                        modes[iter] = Mode.Angle;
                        break;
                }
                iter++;
            }
            var infos = new List<PosAndAngleInfo>();
            foreach (var split in splits.Skip(1))
            {
                iter = 0;
                float? x = null, y = null, angle = null;
                foreach (var info in split.Split(','))
                {
                    if (!float.TryParse(info, NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
                    {
                        val = 0;
                    }
                    switch (modes[iter])
                    {
                        case Mode.X:
                            x = val;
                            break;
                        case Mode.Y:
                            y = val;
                            break;
                        case Mode.Angle:
                            angle = val;
                            break;
                    }
                    iter++;
                }
                bool hasPosition = x.HasValue && y.HasValue;
                if (hasPosition && angle.HasValue)
                {
                    infos.Add(new PosAndAngleInfo(new Vector2(x.Value, y.Value), angle.Value));
                }
                else if (hasPosition)
                {
                    infos.Add(new PosAndAngleInfo(new Vector2(x.Value, y.Value)));
                }
                else if (angle.HasValue)
                {
                    infos.Add(new PosAndAngleInfo(angle.Value));
                }
            }
            return infos.ToArray();
        }

        public static void Save(PosAndAngleInfo[] infos, string filePath)
        {
            var sb = new StringBuilder();
            var hasPosition = infos.Any(i => i.Position.HasValue);
            var hasAngle = infos.Any(i => i.Rotation.HasValue);
            if (hasPosition)
            {
                sb.Append("x,y");
                if (hasAngle)
                {
                    sb.Append(",angle");
                }
            }
            else
            {
                sb.Append("angle");
            }
            sb.AppendLine();
            foreach (var info in infos)
            {
                if (hasPosition)
                {
                    sb.AppendFormat("{0},{1}", info.Position.Value.X.ToString(CultureInfo.InvariantCulture),
                        info.Position.Value.Y.ToString(CultureInfo.InvariantCulture));
                    if (hasAngle)
                    {
                        sb.AppendFormat(",{0}", info.Rotation.Value.ToString(CultureInfo.InvariantCulture));
                    }
                }
                else
                {
                    sb.AppendFormat(",{0}", info.Rotation.Value.ToString(CultureInfo.InvariantCulture));
                }
                sb.AppendLine();
            }
            File.WriteAllText(filePath, sb.ToString());
        }
    }
}
