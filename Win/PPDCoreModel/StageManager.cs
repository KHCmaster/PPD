using PPDCoreModel.Data;
using PPDFramework;
using System;

namespace PPDCoreModel
{
    public class StageManager : GameComponent
    {
        SpriteObject[] sprites;

        public PPDFramework.Resource.ResourceManager ResourceManager
        {
            get;
            set;
        }

        public StageManager(PPDDevice device) : base(device)
        {
            sprites = new SpriteObject[Enum.GetValues(typeof(LayerType)).Length];
            for (int i = 0; i < sprites.Length; i++)
            {
                this.AddChild(sprites[i] = new SpriteObject(device));
            }
        }

        public GameComponent this[LayerType layerType]
        {
            get
            {
                return sprites[(int)layerType];
            }
        }

        public void Clear()
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                sprites[i].ClearChildren();
                sprites[i].SetDefault();
            }
            SetDefault();
        }

        public void Draw(LayerType layerType)
        {
            sprites[(int)layerType].Draw();
        }
    }
}
