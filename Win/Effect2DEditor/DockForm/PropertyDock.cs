using Effect2D;
using System;
using WeifenLuo.WinFormsUI.Docking;

namespace Effect2DEditor.DockForm
{
    public partial class PropertyDock : DockContent
    {
        public event Action<RatioType, float> StateValueChanged;
        public event Action<RatioType> SetRatio;
        public event Action<BlendMode> BlendModeChanged;

        bool ignoreInput;

        public bool IsRatioMakerAvailable
        {
            get;
            set;
        }

        public PropertyDock()
        {
            InitializeComponent();
            foreach (var blendMode in (BlendMode[])Enum.GetValues(typeof(BlendMode)))
            {
                comboBox1.Items.Add(blendMode);
            }
        }

        public void SetEnables(bool IsBezierPosition)
        {
            if (IsBezierPosition)
            {
                button1.Enabled = false;
                button2.Enabled = false;
                button7.Enabled = true;
                textBox1.Enabled = false;
                textBox2.Enabled = false;
            }
            else
            {
                button1.Enabled = true;
                button2.Enabled = true;
                button7.Enabled = false;
                textBox1.Enabled = true;
                textBox2.Enabled = true;
            }
        }

        public void SetState(EffectStateStructure state)
        {
            ignoreInput = true;
            if (state != null)
            {
                this.comboBox1.SelectedItem = state.BlendMode;
                this.textBox1.Text = state.X.ToString();
                this.textBox2.Text = state.Y.ToString();
                this.textBox3.Text = state.Alpha.ToString();
                this.textBox4.Text = state.Rotation.ToString();
                this.textBox5.Text = state.ScaleX.ToString();
                this.textBox6.Text = state.ScaleY.ToString();
            }
            else
            {
                this.comboBox1.SelectedItem = BlendMode.None;
                this.textBox1.Text = "";
                this.textBox2.Text = "";
                this.textBox3.Text = "";
                this.textBox4.Text = "";
                this.textBox5.Text = "";
                this.textBox6.Text = "";
            }
            ignoreInput = false;
            this.button1.Enabled = IsRatioMakerAvailable;
            this.button2.Enabled = IsRatioMakerAvailable;
            this.button3.Enabled = IsRatioMakerAvailable;
            this.button4.Enabled = IsRatioMakerAvailable;
            this.button5.Enabled = IsRatioMakerAvailable;
            this.button6.Enabled = IsRatioMakerAvailable;
        }

        private void label1_ValueChange(object sender, ValueChangeEventArgs e)
        {
            if (float.TryParse(this.textBox1.Text, out float val))
            {
                this.textBox1.Text = (val + e.ChangedValue).ToString();
            }
        }

        private void label2_ValueChange(object sender, ValueChangeEventArgs e)
        {
            if (float.TryParse(this.textBox2.Text, out float val))
            {
                this.textBox2.Text = (val + e.ChangedValue).ToString();
            }
        }

        private void label3_ValueChange(object sender, ValueChangeEventArgs e)
        {
            if (float.TryParse(this.textBox3.Text, out float val))
            {
                this.textBox3.Text = (val + (float)e.ChangedValue / 100).ToString();
            }
        }

        private void label4_ValueChange(object sender, ValueChangeEventArgs e)
        {
            if (float.TryParse(this.textBox4.Text, out float val))
            {
                this.textBox4.Text = (val + e.ChangedValue).ToString();
            }
        }

        private void label5_ValueChange(object sender, ValueChangeEventArgs e)
        {
            if (float.TryParse(this.textBox5.Text, out float val))
            {
                this.textBox5.Text = (val + (float)e.ChangedValue / 100).ToString();
            }
        }

        private void label6_ValueChange(object sender, ValueChangeEventArgs e)
        {
            if (float.TryParse(this.textBox6.Text, out float val))
            {
                this.textBox6.Text = (val + (float)e.ChangedValue / 100).ToString();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ignoreInput) return;
            if (this.comboBox1.SelectedItem is BlendMode)
            {
                OnBlendModeChanged((BlendMode)this.comboBox1.SelectedItem);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (ignoreInput) return;
            if (float.TryParse(this.textBox1.Text, out float val))
            {
                OnStateValueChanged(RatioType.X, val);
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (ignoreInput) return;
            if (float.TryParse(this.textBox2.Text, out float val))
            {
                OnStateValueChanged(RatioType.Y, val);
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (ignoreInput) return;
            if (float.TryParse(this.textBox3.Text, out float val))
            {
                if (val < 0)
                {
                    val = 0;
                    ignoreInput = true;
                    this.textBox3.Text = "0";
                    ignoreInput = false;
                }
                else if (val > 1)
                {
                    val = 1;
                    ignoreInput = true;
                    this.textBox3.Text = "1";
                    ignoreInput = false;
                }
                OnStateValueChanged(RatioType.Alpha, val);
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            if (ignoreInput) return;
            if (float.TryParse(this.textBox4.Text, out float val))
            {
                OnStateValueChanged(RatioType.Rotation, val);
            }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            if (ignoreInput) return;
            if (float.TryParse(this.textBox5.Text, out float val))
            {
                if (val < 0)
                {
                    val = 0;
                    ignoreInput = true;
                    this.textBox5.Text = "0";
                    ignoreInput = false;
                }
                OnStateValueChanged(RatioType.ScaleX, val);
            }
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            if (ignoreInput) return;
            if (float.TryParse(this.textBox6.Text, out float val))
            {
                if (val < 0)
                {
                    val = 0;
                    ignoreInput = true;
                    this.textBox6.Text = "0";
                    ignoreInput = false;
                }
                OnStateValueChanged(RatioType.ScaleY, val);
            }
        }

        private void OnStateValueChanged(RatioType ratioType, float value)
        {
            if (StateValueChanged != null)
            {
                StateValueChanged.Invoke(ratioType, value);
            }
        }

        private void OnBlendModeChanged(BlendMode blendMode)
        {
            BlendModeChanged?.Invoke(blendMode);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OnSetRatio(RatioType.X);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OnSetRatio(RatioType.Y);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OnSetRatio(RatioType.Alpha);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OnSetRatio(RatioType.Rotation);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            OnSetRatio(RatioType.ScaleX);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            OnSetRatio(RatioType.ScaleY);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            OnSetRatio(RatioType.BezierPosition);
        }

        private void OnSetRatio(RatioType type)
        {
            if (SetRatio != null)
            {
                SetRatio.Invoke(type);
            }
        }
    }
}
