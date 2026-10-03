using Effect2DEditor.DockForm;
using Microsoft.Win32;
using PPDConfiguration;
using System.Windows;
using System.Windows.Media;
using Color = System.Drawing.Color;

namespace Effect2DEditor
{
    public partial class ChangeBackGroundWindow : Window
    {
        Color color = Color.White;

        public ChangeBackGroundWindow()
        {
            InitializeComponent();
            UpdateColorPreview();
        }
        public void SetLang(SettingReader setting)
        {
            if (setting != null)
            {
                this.Title = setting["CBGText"];
                this.radioButton1.Content = setting["CBGRadioButton1"];
                this.radioButton2.Content = setting["CBGRadioButton2"];
                this.radioButton3.Content = setting["CBGRadioButton3"];
            }
        }
        public BackGroundMode Mode
        {
            get
            {
                if (radioButton1.IsChecked == true) return BackGroundMode.Default;
                else if (radioButton2.IsChecked == true) return BackGroundMode.Color;
                else return BackGroundMode.Image;
            }
            set
            {
                switch (value)
                {
                    case BackGroundMode.Default:
                        radioButton1.IsChecked = true;
                        break;
                    case BackGroundMode.Color:
                        radioButton2.IsChecked = true;
                        break;
                    case BackGroundMode.Image:
                        radioButton3.IsChecked = true;
                        break;
                }
            }
        }
        public Color Color
        {
            get { return color; }
            set
            {
                color = value;
                UpdateColorPreview();
            }
        }
        public string Filename
        {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }

        private void UpdateColorPreview()
        {
            colorPreview.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(color.R, color.G, color.B));
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ColorPickWindow
            {
                Owner = this,
                SelectedColor = System.Windows.Media.Color.FromRgb(color.R, color.G, color.B)
            };
            if (dialog.ShowDialog() == true)
            {
                Color = Color.FromArgb(dialog.SelectedColor.R, dialog.SelectedColor.G, dialog.SelectedColor.B);
                radioButton2.IsChecked = true;
            }
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog(this) == true)
            {
                this.textBox1.Text = dialog.FileName;
                radioButton3.IsChecked = true;
            }
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
