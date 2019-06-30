using PPDFramework;
using PPDShareComponent;
using SharpDX;
using System.Collections.Generic;

namespace PPDSingle
{
    struct GraphData
    {
        public string XValue;
        public float YValue;
    }

    class GraphDrawer : GameComponent
    {
        int width = 350;
        int realWidth = 350;
        int height = 250;
        TextureString name;
        TextureString xname;
        TextureString[] ynames;
        RectangleComponent yAxis;
        RectangleComponent xAxis;
        LineComponent line;

        int lineWidth = 1;
        Color4 lineColor = PPDColors.Selection;

        private GraphData[] data;

        bool layoutUpdated = true;
        bool dataChanged;

        Vector2 drawStartPos;

        List<Vector2> poses;

        public GraphDrawer(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager) : base(device)
        {
            this.AddChild(name = new TextureString(device, "", 16, PPDColors.White));
            this.AddChild(xname = new TextureString(device, Utility.Language["Latest"], 12, PPDColors.White));
            ynames = new TextureString[5];
            for (int i = 0; i < ynames.Length; i++)
            {
                this.AddChild(ynames[i] = new TextureString(device, "", 12, PPDColors.White));
            }
            this.AddChild(yAxis = new RectangleComponent(device, resourceManager, PPDColors.White)
            {
                RectangleWidth = 2
            });
            this.AddChild(xAxis = new RectangleComponent(device, resourceManager, PPDColors.White)
            {
                RectangleHeight = 2
            });
            this.AddChild(line = new LineComponent(device, resourceManager, lineColor)
            {
                LineWidth = 2
            });

            poses = new List<Vector2>();
            Formatter = FloatToIntFormatter.Formatter;
        }

        public string Name
        {
            get
            {
                return name.Text;
            }
            set
            {
                name.Text = value;
            }
        }

        public Vector2 NamePositionCenter
        {
            get
            {
                return name.Position;
            }
            set
            {
                name.Position = new Vector2(value.X - name.Width / 2, value.Y - name.CharacterHeight / 2);
            }
        }

        public int GraphWidth
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
                realWidth = width;
                layoutUpdated = true;
            }
        }

        public int GraphHeight
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
                layoutUpdated = true;
            }
        }

        public override Vector2 Position
        {
            get
            {
                return base.Position;
            }
            set
            {
                base.Position = value;
                drawStartPos = Position;
                layoutUpdated = true;
            }
        }


        public GraphData[] Data
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
                dataChanged = true;
            }
        }

        public int LineWidth
        {
            get
            {
                return lineWidth;
            }
            set
            {
                lineWidth = value;
            }
        }

        public Color4 LineColor
        {
            get
            {
                return lineColor;
            }
            set
            {
                lineColor = value;
            }
        }

        public IFloatValueFormatter Formatter
        {
            set;
            get;
        }

        public List<Vector2> Poses
        {
            get
            {
                return poses;
            }
        }

        protected override void UpdateImpl()
        {
            if (layoutUpdated)
            {
                layoutUpdated = false;
                xname.Position = new Vector2(Position.X + GraphWidth - xname.Width, Position.Y + GraphHeight + 5);
                float areaHeight = GraphHeight - 20;
                for (int i = 0; i < ynames.Length; i++)
                {
                    ynames[i].Position = new Vector2(0, 10 + areaHeight / (ynames.Length - 1) * i - ynames[i].CharacterHeight / 2);
                }
            }
            if (dataChanged && data != null)
            {
                dataChanged = false;
                float minValue = 0, maxValue = 0;
                if (data.Length > 0)
                {
                    minValue = float.MaxValue;
                    maxValue = float.MinValue;
                }

                foreach (GraphData value in data)
                {
                    if (value.YValue < minValue)
                    {
                        minValue = value.YValue;
                    }
                    if (value.YValue > maxValue)
                    {
                        maxValue = value.YValue;
                    }
                }
                float diff = maxValue - minValue;
                if (diff > 0)
                {
                    for (int i = 0; i < ynames.Length; i++)
                    {
                        ynames[i].Text = Formatter.Format(maxValue - diff / (ynames.Length - 1) * i);
                    }
                    if (ynames[0].Text == ynames[1].Text || ynames[2].Text == ynames[1].Text)
                    {
                        ynames[1].Text = "";
                    }
                    if (ynames[4].Text == ynames[3].Text || ynames[2].Text == ynames[3].Text)
                    {
                        ynames[3].Text = "";
                    }
                    if (ynames[0].Text == ynames[2].Text || ynames[4].Text == ynames[2].Text)
                    {
                        ynames[2].Text = "";
                    }
                }
                else
                {
                    for (int i = 0; i < ynames.Length; i++)
                    {
                        ynames[i].Text = "";
                    }
                    ynames[2].Text = Formatter.Format(maxValue);
                }
                int max = 0;
                for (int i = 0; i < ynames.Length; i++)
                {
                    if (max < ynames[i].Width)
                    {
                        max = (int)ynames[i].Width;
                    }
                }
                realWidth = width - max;
                drawStartPos = new Vector2(max, 0);
                for (int i = 0; i < ynames.Length; i++)
                {
                    ynames[i].Position = new Vector2(drawStartPos.X - ynames[i].Width, ynames[i].Position.Y);
                }
                poses.Clear();

                if (data.Length > 1)
                {

                    float areaWidth = realWidth - 20, areaHeight = height - 20;

                    for (int i = 0; i < data.Length; i++)
                    {
                        float value = data[i].YValue;
                        if (diff > 0)
                        {
                            poses.Add(new Vector2(drawStartPos.X + 10 + areaWidth / (data.Length - 1) * i, drawStartPos.Y + 10 + areaHeight - (value - minValue) / diff * areaHeight));
                        }
                        else
                        {
                            poses.Add(new Vector2(drawStartPos.X + 10 + areaWidth / (data.Length - 1) * i, drawStartPos.Y + 10 + areaHeight / 2));
                        }
                    }
                }
                line.Points = poses.ToArray();

                yAxis.Position = new Vector2(drawStartPos.X + 5, drawStartPos.Y);
                yAxis.RectangleHeight = height;
                xAxis.Position = new Vector2(drawStartPos.X + 5, drawStartPos.Y + height);
                xAxis.RectangleWidth = realWidth;
            }
        }
    }
}
