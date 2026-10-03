using Effect2D;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Effect2DEditor.DockForm
{
    public partial class PropertyDock : UserControl
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
            ignoreInput = true;
            InitializeComponent();
            ignoreInput = false;
            foreach (var blendMode in Enum.GetValues<BlendMode>())
            {
                comboBox1.Items.Add(blendMode);
            }
        }

        public void SetEnables(bool IsBezierPosition)
        {
            if (IsBezierPosition)
            {
                button1.IsEnabled = false;
                button2.IsEnabled = false;
                button7.IsEnabled = true;
                textBox1.IsEnabled = false;
                textBox2.IsEnabled = false;
            }
            else
            {
                button1.IsEnabled = true;
                button2.IsEnabled = true;
                button7.IsEnabled = false;
                textBox1.IsEnabled = true;
                textBox2.IsEnabled = true;
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
            this.button1.IsEnabled = IsRatioMakerAvailable;
            this.button2.IsEnabled = IsRatioMakerAvailable;
            this.button3.IsEnabled = IsRatioMakerAvailable;
            this.button4.IsEnabled = IsRatioMakerAvailable;
            this.button5.IsEnabled = IsRatioMakerAvailable;
            this.button6.IsEnabled = IsRatioMakerAvailable;
        }

        private void Label1_ValueChange(object sender, ValueChangeEventArgs e)
        {
            if (float.TryParse(this.textBox1.Text, out float val))
            {
                this.textBox1.Text = (val + e.ChangedValue).ToString();
            }
        }

        private void Label2_ValueChange(object sender, ValueChangeEventArgs e)
        {
            if (float.TryParse(this.textBox2.Text, out float val))
            {
                this.textBox2.Text = (val + e.ChangedValue).ToString();
            }
        }

        private void Label3_ValueChange(object sender, ValueChangeEventArgs e)
        {
            if (float.TryParse(this.textBox3.Text, out float val))
            {
                this.textBox3.Text = (val + (float)e.ChangedValue / 100).ToString();
            }
        }

        private void Label4_ValueChange(object sender, ValueChangeEventArgs e)
        {
            if (float.TryParse(this.textBox4.Text, out float val))
            {
                this.textBox4.Text = (val + e.ChangedValue).ToString();
            }
        }

        private void Label5_ValueChange(object sender, ValueChangeEventArgs e)
        {
            if (float.TryParse(this.textBox5.Text, out float val))
            {
                this.textBox5.Text = (val + (float)e.ChangedValue / 100).ToString();
            }
        }

        private void Label6_ValueChange(object sender, ValueChangeEventArgs e)
        {
            if (float.TryParse(this.textBox6.Text, out float val))
            {
                this.textBox6.Text = (val + (float)e.ChangedValue / 100).ToString();
            }
        }

        private void ComboBox1_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ignoreInput) return;
            if (this.comboBox1.SelectedItem is BlendMode mode)
            {
                OnBlendModeChanged(mode);
            }
        }

        private void TextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ignoreInput) return;
            if (float.TryParse(this.textBox1.Text, out float val))
            {
                OnStateValueChanged(RatioType.X, val);
            }
        }

        private void TextBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ignoreInput) return;
            if (float.TryParse(this.textBox2.Text, out float val))
            {
                OnStateValueChanged(RatioType.Y, val);
            }
        }

        private void TextBox3_TextChanged(object sender, TextChangedEventArgs e)
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

        private void TextBox4_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ignoreInput) return;
            if (float.TryParse(this.textBox4.Text, out float val))
            {
                OnStateValueChanged(RatioType.Rotation, val);
            }
        }

        private void TextBox5_TextChanged(object sender, TextChangedEventArgs e)
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

        private void TextBox6_TextChanged(object sender, TextChangedEventArgs e)
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
            StateValueChanged?.Invoke(ratioType, value);
        }

        private void OnBlendModeChanged(BlendMode blendMode)
        {
            BlendModeChanged?.Invoke(blendMode);
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            OnSetRatio(RatioType.X);
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            OnSetRatio(RatioType.Y);
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            OnSetRatio(RatioType.Alpha);
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            OnSetRatio(RatioType.Rotation);
        }

        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            OnSetRatio(RatioType.ScaleX);
        }

        private void Button6_Click(object sender, RoutedEventArgs e)
        {
            OnSetRatio(RatioType.ScaleY);
        }

        private void Button7_Click(object sender, RoutedEventArgs e)
        {
            OnSetRatio(RatioType.BezierPosition);
        }

        private void OnSetRatio(RatioType type)
        {
            SetRatio?.Invoke(type);
        }
    }
}
