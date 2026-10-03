using System.Windows;

namespace PPDEditor.Forms
{
    public partial class SwapLineWindow : Window
    {
        static readonly string[] defaultLines = ["スクエア", "クロス", "サークル", "トライアングル", "レフト", "ダウン", "ライト", "アップ", "アール", "エル"];

        public void SetLang()
        {
            this.Title = Utility.Language["SwapLine"];
            this.label1.Text = Utility.Language["SwapLineLabel1"];
            this.label2.Text = Utility.Language["SwapLineLabel2"];
            this.label3.Text = Utility.Language["SwapLineLabel3"];
            var lines = new string[10];
            for (var i = 0; i < lines.Length; i++)
            {
                lines[i] = Utility.Language["SwapLineBox" + (i + 1)];
            }
            SetLines(lines);
        }
        public int First
        {
            get
            {
                return this.comboBox1.SelectedIndex;
            }
        }
        public int Second
        {
            get
            {
                return this.comboBox2.SelectedIndex;
            }
        }
        public SwapLineWindow()
        {
            InitializeComponent();
            SetLines(defaultLines);
        }

        private void SetLines(string[] lines)
        {
            this.comboBox1.Items.Clear();
            this.comboBox2.Items.Clear();
            foreach (var line in lines)
            {
                this.comboBox1.Items.Add(line);
                this.comboBox2.Items.Add(line);
            }
            this.comboBox1.SelectedIndex = 0;
            this.comboBox2.SelectedIndex = 0;
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
