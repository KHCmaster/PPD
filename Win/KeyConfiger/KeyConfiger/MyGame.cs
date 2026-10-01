using PPDFramework;
using PPDInput;
using Vortice.DirectInput;

namespace KeyConfiger
{
    public class MyGame : Game
    {
        Input input;
        readonly Form1 form;


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

            if (input.GetPressedKey(out Key k))
            {
                form.SetNumber((int)k, true);
            }

            if (input.GetPressedButton(out int a))
            {
                form.SetNumber(a, false);
            }
        }

        protected override void DisposeResource()
        {
            input?.Dispose();
            input = null;
            base.DisposeResource();
        }
    }
}