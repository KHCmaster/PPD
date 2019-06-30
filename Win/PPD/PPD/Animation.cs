using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;

namespace testgame
{
    class Animation : PictureObject
    {
        public enum State
        {
            playing = 0,
            stop = 1
        }
        State state = State.playing;
        public event EventHandler Finish;
        //filenames
        string[] filenames;
        //animation
        float timeduration;
        int count = 0;
        //num of filename
        int pfilenames = 0;
        bool reverse;
        bool reversing = false;
        public Animation(string[] filenames,float timeduration, float x, float y, float z,bool reverse, Dictionary<string, Picture> resource, Device device, Sprite sprite)
            : base(filenames[0], x, y, z, resource, device, sprite)
        {
            this.reverse = reverse;
            this.filenames = filenames;
            this.timeduration = timeduration;
        }
        public Animation(string[] filenames, float timeduration, float x, float y, float z, bool reverse, bool center,Dictionary<string, Picture> resource, Device device, Sprite sprite)
            : base(filenames[0], x, y, z, center,resource, device, sprite)
        {
            this.reverse = reverse;
            this.filenames = filenames;
            this.timeduration = timeduration;
        }
        public State PlayState
        {
            get
            {
                return state;
            }
        }
        public void Play()
        {
            state = State.playing;
        }
        public void Stop()
        {
            state = State.stop;
        }
        public void StopAtLast()
        {
            state = State.stop;
            pfilenames = filenames.Length - 1;
            Filename = filenames[pfilenames];
        }
        public override void Update()
        {
            //base.Update();
            if (state == State.stop) return;
            count++;
            if (count / 60.0f * 1000 > timeduration)
            {
                if (!reverse)
                {
                    if (pfilenames >= filenames.Length - 1)
                    {
                        pfilenames = 0;
                        if (Finish != null)
                        {
                            Finish.Invoke(this, EventArgs.Empty);
                        }
                    }
                    else
                    {
                        pfilenames++;
                    }
                }
                else
                {
                    if (!reversing)
                    {
                        if (pfilenames >= filenames.Length - 1)
                        {
                            pfilenames--;
                            reversing = true;
                        }
                        else
                        {
                            pfilenames++;
                        }
                    }
                    else
                    {
                        if (pfilenames <= 0)
                        {
                            pfilenames++;
                            reversing = false;
                        }
                        else
                        {
                            pfilenames--;
                        }
                    }

                }

                Filename = filenames[pfilenames];
                count = 0;
            }
        }
        public override void Draw()
        {
            base.Draw();
        }
    }
}
