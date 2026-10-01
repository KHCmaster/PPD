using PPDMath;
using PPDFramework;
using System.Globalization;
using System.IO;
using System.Xml;

namespace PPDSingle
{
    class BackGroundDisplay : GameComponent
    {
        private readonly PPDFramework.Resource.ResourceManager resourceManager;
        private readonly string filePath;
        private readonly string contentElementName;

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
                    Position = new System.Numerics.Vector2(ReadAttribute(reader, "X", 0), ReadAttribute(reader, "Y", 0)),
                    Alpha = ReadAttribute(reader, "Alpha", 1),
                    Scale = new System.Numerics.Vector2(ReadAttribute(reader, "ScaleX", 1), ReadAttribute(reader, "ScaleY", 1)),
                    Rotation = ReadAttribute(reader, "Rotation", 0)
                };
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
                    Position = new System.Numerics.Vector2(ReadAttribute(reader, "X", 0), ReadAttribute(reader, "Y", 0)),
                    Alignment = reader.GetAttribute("Center") == "1" ? EffectObject.EffectAlignment.Center : EffectObject.EffectAlignment.TopLeft,
                    Alpha = ReadAttribute(reader, "Alpha", 1),
                    Scale = new System.Numerics.Vector2(ReadAttribute(reader, "ScaleX", 1), ReadAttribute(reader, "ScaleY", 1)),
                    Rotation = ReadAttribute(reader, "Rotation", 0),
                    PlayType = ParsePlayType(reader.GetAttribute("PlayType"))
                };
                eo.Play();
                this.AddChild(eo);
            }
        }

        private Effect2D.EffectManager.PlayType ParsePlayType(string value)
        {
            return value switch
            {
                "Once" => Effect2D.EffectManager.PlayType.Once,
                "ReverseOnce" => Effect2D.EffectManager.PlayType.ReverseOnce,
                "Loop" => Effect2D.EffectManager.PlayType.Loop,
                "ReserseLoop" => Effect2D.EffectManager.PlayType.ReverseLoop,
                _ => Effect2D.EffectManager.PlayType.Loop,
            };
        }

        private float ReadAttribute(XmlReader reader, string attribute, float defaultValue)
        {
            var value = reader.GetAttribute(attribute);
            if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float ret))
            {
                ret = defaultValue;
            }
            return ret;
        }
    }
}
