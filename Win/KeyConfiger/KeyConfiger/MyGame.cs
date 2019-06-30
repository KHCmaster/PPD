using PPDFramework;
using PPDInput;
using SharpDX.DirectInput;

namespace KeyConfiger
{
    public class MyGame : Game
    {
        Input input;
        Form1 form;


        public MyGame(PPDExecuteArg args) : base(args, new Form1())
        {
            form = Form as Form1;
        }

        protected override void Initialize()
        {
            base.Initialize();
            //Inputクラスの生成
            input = new Input(Form.MainForm);
            input.Load();
            form.MyGame = this;
        }

        public Input Input
        {
            get
            {
                return input;
            }
        }

        protected override void Update()
        {
            if (Form.IsCloseRequired)
            {
                Form.CloseAdmitted = true;
                Form.MainForm.Close();
            }

            Key k = Key.Unknown;
            if (input.GetPressedKey(out k))
            {
                form.SetNumber((int)k, true);
            }

            int a = -1;
            if (input.GetPressedButton(out a))
            {
                form.SetNumber(a, false);
            }
        }

        protected override void DisposeResource()
        {
            if (input != null)
            {
                input.Dispose();
                input = null;
            }
            base.DisposeResource();
        }
    }
}