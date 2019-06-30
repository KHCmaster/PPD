using System;
using System.Collections.Generic;
using System.Text;
using SlimDX.Direct3D9;

namespace testgame
{
    interface ISceneBase
    {
        SceneManager SceneManager { get; set; }
        Device Device { get; set; }
        Sprite Sprite { get; set; }
        ExSound Sound { get; set; }
        Dictionary<string, object> Param { get; set; }
        Dictionary<string, object> PreviousParam { get; set; }
        void Load();
        void Update(int[] presscount, bool[] released);
        void Draw();
        void Dispose();
    }
}
