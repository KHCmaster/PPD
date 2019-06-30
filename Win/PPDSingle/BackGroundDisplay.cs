using PPDFramework;
using System.Globalization;
using System.IO;
using System.Xml;

namespace PPDSingle
{
    class BackGroundDisplay : GameComponent
    {
        private PPDFramework.Resource.ResourceManager resourceManager;
        private string filePath;
        private string contentElementName;

        public BackGroundDisplay(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, string filePath, string contentElementName) : base(device)
        {
            this.resourceManager = resourceManager;
            this.filePath = filePath;
            this.contentElementName = contentElementName;
            Read();
        }

        private void Read()
        {
            if (File.Exists(filePath))
            {
                var reader = XmlReader.Create(filePath);
                try
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement(contentElementName))
                        {
                            ReadData(reader.ReadSubtree());
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    reader.Close();
                }
            }
        }

        private void ReadData(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    string element = reader.LocalName;
                    switch (element)
                    {
                        case "Picture":
                            ReadPicture(reader);
                            break;
                        case "Effect":
                            ReadEffect(reader);
                            break;
                    }
                }
            }
            reader.Close();
        }

        private void ReadPicture(XmlReader reader)
        {
            var filePath = Utility.Path.Combine(reader.GetAttribute("Path"));
            if (File.Exists(filePath))
            {
                var po = new PictureObject(device, resourceManager, filePath, reader.GetAttribute("Center") == "1")
                {
                    Position = new SharpDX.Vector2(ReadAttribute(reader, "X", 0), ReadAttribute(reader, "Y", 0))
                };
                po.Alpha = ReadAttribute(reader, "Alpha", 1);
                po.Scale = new SharpDX.Vector2(ReadAttribute(reader, "ScaleX", 1), ReadAttribute(reader, "ScaleY", 1));
                po.Rotation = ReadAttribute(reader, "Rotation", 0);
                this.AddChild(po);
            }
        }

        private void ReadEffect(XmlReader reader)
        {
            var filePath = Utility.Path.Combine(reader.GetAttribute("Path"));
            if (File.Exists(filePath))
            {
                var eo = new EffectObject(device, resourceManager, filePath)
                {
                    Position = new SharpDX.Vector2(ReadAttribute(reader, "X", 0), ReadAttribute(reader, "Y", 0))
                };
                eo.Alignment = reader.GetAttribute("Center") == "1" ? EffectObject.EffectAlignment.Center : EffectObject.EffectAlignment.TopLeft;
                eo.Alpha = ReadAttribute(reader, "Alpha", 1);
                eo.Scale = new SharpDX.Vector2(ReadAttribute(reader, "ScaleX", 1), ReadAttribute(reader, "ScaleY", 1));
                eo.Rotation = ReadAttribute(reader, "Rotation", 0);
                eo.PlayType = ParsePlayType(reader.GetAttribute("PlayType"));
                eo.Play();
                this.AddChild(eo);
            }
        }

        private Effect2D.EffectManager.PlayType ParsePlayType(string value)
        {
            switch (value)
            {
                case "Once":
                    return Effect2D.EffectManager.PlayType.Once;
                case "ReverseOnce":
                    return Effect2D.EffectManager.PlayType.ReverseOnce;
                case "Loop":
                    return Effect2D.EffectManager.PlayType.Loop;
                case "ReserseLoop":
                    return Effect2D.EffectManager.PlayType.ReverseLoop;
                default:
                    return Effect2D.EffectManager.PlayType.Loop;
            }
        }

        private float ReadAttribute(XmlReader reader, string attribute, float defaultValue)
        {
            float ret = defaultValue;
            var value = reader.GetAttribute(attribute);
            if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out ret))
            {
                ret = defaultValue;
            }
            return ret;
        }
    }
}
