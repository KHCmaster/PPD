using Effect2D;
using PPDCoreModel.Data;
using PPDFramework;
using PPDFramework.Resource;
using PPDFramework.Sprites;
using PPDFramework.Vertex;
using PPDPack;
using PPDSound;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;

namespace PPDCoreModel
{
    /// <summary>
    /// 
    /// </summary>
    public class ScriptResourceManager : DisposableComponent
    {
        private PPDDevice device;
        private ISound sound;
        private PPDFramework.Resource.SpriteResourceManager resourceManager;
        private PPDFramework.PathManager pathManager;
        private bool initialized;

        private Dictionary<string, byte[]> soundDict;
        private Dictionary<string, byte[]> othersDict;
        private int soundID;
        List<VertexInfo> createdVertices;

        public PPDDevice Device
        {
            get { return device; }
        }

        public ImageResourceBase[] ImageResources
        {
            get { return resourceManager?.ImageResources; }
        }

        public ScriptResourceManager(PPDDevice device, ISound sound, string resourceFilePath, string spriteDirName)
        {
            this.device = device;
            this.sound = sound;

            Initialize(resourceFilePath, spriteDirName);
        }

        private void Initialize(string resourceFilePath, string spriteDirName)
        {
            soundDict = new Dictionary<string, byte[]>();
            othersDict = new Dictionary<string, byte[]>();
            createdVertices = new List<VertexInfo>();
            pathManager = new PathManager("");

            if (File.Exists(resourceFilePath))
            {
                try
                {
                    using (PackReader packReader = new PackReader(resourceFilePath))
                    {
                        UnPackAll(packReader, spriteDirName, File.GetLastWriteTime(resourceFilePath));
                    }
                    initialized = true;
                }
                catch (Exception e)
                {
#if DEBUG
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
#endif
                }
            }
        }

        private void UnPackAll(PackReader packReader, string spriteDirName, DateTime lastWriteTime)
        {
            var imageList = new List<string>();
            var effectImageList = new Dictionary<string, List<string>>();
            foreach (var fileName in packReader.FileList)
            {
                if (!fileName.StartsWith("Effect", StringComparison.Ordinal))
                {
                    continue;
                }
                var split = fileName.Split('\\');
                if (split.Length < 3)
                {
                    continue;
                }
                var effectName = split[1];
                var effectImageName = split[2];
                var normalizedEffectImageName = SpriteDictionary.RemoveScale(effectImageName);
                var key = Path.Combine("Effect", effectName, normalizedEffectImageName);
                if (!effectImageList.TryGetValue(key, out List<string> effectImageNames))
                {
                    effectImageNames = new List<string>();
                    effectImageList.Add(key, effectImageNames);
                }
                effectImageNames.Add(fileName);
            }
            foreach (string resourceName in packReader.FileList)
            {
                var ppdpsr = packReader.Read(resourceName);
                var split = resourceName.Split('\\');
                if (split.Length == 2)
                {
                    switch (split[0])
                    {
                        case "Image":
                            imageList.Add(resourceName);
                            break;
                        case "Effect":
                            if (Path.GetExtension(resourceName) == ".etd")
                            {
                                EffectLoader.Load(ppdpsr, resourceName, (ReadResourceCallBack)((filename) =>
                                {
                                    if (effectImageList.ContainsKey(filename))
                                    {
                                        foreach (var imageFileName in effectImageList[filename])
                                        {
                                            imageList.Add(imageFileName);
                                        }
                                    }
                                }), f => packReader.Read(f));
                            }
                            break;
                        case "Sound":
                            byte[] data = new byte[ppdpsr.Length];
                            ppdpsr.Read(data, 0, data.Length);
                            soundDict.Add(resourceName, data);
                            break;
                        case "Others":
                            data = new byte[ppdpsr.Length];
                            ppdpsr.Read(data, 0, data.Length);
                            othersDict.Add(resourceName, data);
                            break;
                    }
                }
            }
            var spriteManager = new PPDPackSpriteManager(packReader, imageList.ToArray(), spriteDirName, lastWriteTime);
            spriteManager.Pack();
            resourceManager = new SpriteResourceManager(device, spriteManager);
        }

        public object GetResource(string resourceName, ResourceKind resourceKind, Dictionary<string, object> param)
        {
            if (initialized)
            {
                Vector2 position = param != null && param.ContainsKey("Position") ? (Vector2)param["Position"] : Vector2.Zero;
                switch (resourceKind)
                {
                    case ResourceKind.Effect:
                        var path = String.Format("Effect\\{0}", resourceName);
                        if (EffectLoader.IsCached(path))
                        {
                            return new EffectObject(device, resourceManager, EffectLoader.Load(null, path, f => { return; }, f => null))
                            {
                                Position = position
                            };
                        }
                        return null;
                    case ResourceKind.Image:
                        var isCenter = (bool)param["IsCenter"];
                        return new PictureObject(device, resourceManager, pathManager.Combine("Image", resourceName), isCenter)
                        {
                            Position = position
                        };
                    case ResourceKind.Sound:
                        soundID++;
                        var name = String.Format("Sound\\{0}_{1}", resourceName, soundID);
                        var soundResource = new SoundResource(sound, name, soundDict[String.Format("Sound\\{0}", resourceName)]);
                        resourceManager.Add(name, soundResource);
                        return soundResource;
                    case ResourceKind.Number:
                        var alignment = (Alignment)param["Alignment"];
                        var maxDigit = (int)param["MaxDigit"];
                        return new NumberPictureObject(device, resourceManager, pathManager.Combine("Image", resourceName))
                        {
                            Position = position,
                            Alignment = alignment,
                            MaxDigit = maxDigit
                        };
                    case ResourceKind.Others:
                        return new MemoryStream(othersDict[String.Format("Others\\{0}", resourceName)]);
                    case ResourceKind.Rectangle:
                        return new RectangleComponent(device, resourceManager, (Color4)param["Color"]);
                    case ResourceKind.VertexBuffer:
                        var vertexInfo = device.GetModule<ShaderCommon>().CreateVertex((int)param["VertexCount"]);
                        createdVertices.Add(vertexInfo);
                        return vertexInfo;
                    case ResourceKind.Polygon:
                        var filename = pathManager.Combine("Image", resourceName);
                        var imageResource = resourceManager.GetResource<ImageResourceBase>(filename);
                        if (imageResource == null)
                        {
                            imageResource = (ImageResourceBase)resourceManager.Add(filename, ImageResourceFactoryManager.Factory.Create(device, filename, false));
                        }
                        var primitiveType = (PrimitiveType)param["PrimitiveType"];
                        var primitiveCount = (int)param["PrimitiveCount"];
                        var startIndex = (int)param["StartIndex"];
                        var vertexCount = (int)param["VertexCount"];
                        return new PolygonObject(device, imageResource, (VertexInfo)param["Vertex"])
                        {
                            PrimitiveType = primitiveType,
                            PrimitiveCount = primitiveCount,
                            StartIndex = startIndex,
                            VertexCount = vertexCount,
                        };
                }
            }

            return null;
        }

        protected override void DisposeResource()
        {
            base.DisposeResource();
            if (resourceManager != null)
            {
                resourceManager.Dispose();
                resourceManager = null;
            }
            foreach (var vertex in createdVertices)
            {
                vertex.Dispose();
            }
            createdVertices.Clear();
        }
    }
}
