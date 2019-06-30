using System;
using System.IO;
using System.Windows.Forms;

namespace BMSTOPPD
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            this.dataGridView1.RowCount = 10;
            this.dataGridView1.Rows[0].HeaderCell.Value = "square";
            this.dataGridView1.Rows[1].HeaderCell.Value = "cross";
            this.dataGridView1.Rows[2].HeaderCell.Value = "circle";
            this.dataGridView1.Rows[3].HeaderCell.Value = "triangle";
            this.dataGridView1.Rows[4].HeaderCell.Value = "left";
            this.dataGridView1.Rows[5].HeaderCell.Value = "down";
            this.dataGridView1.Rows[6].HeaderCell.Value = "right";
            this.dataGridView1.Rows[7].HeaderCell.Value = "up";
            this.dataGridView1.Rows[8].HeaderCell.Value = "R";
            this.dataGridView1.Rows[9].HeaderCell.Value = "L";
            if (!File.Exists("setting.ini"))
            {
                var sw = new StreamWriter("setting.ini");
                for (int i = 0; i < 10; i++)
                {
                    sw.Write("400,255,0\n");
                }
                sw.Close();
            }
            var sr = new StreamReader("setting.ini");
            for (int i = 0; i < 10; i++)
            {
                var str = sr.ReadLine();
                var strs = str.Split(',');
                this.dataGridView1[0, i].Value = strs[0];
                this.dataGridView1[1, i].Value = strs[1];
                this.dataGridView1[2, i].Value = strs[2];
            }
            sr.Close();
        }
        public void savedata()
        {
            var sw = new StreamWriter("setting.ini");
            for (int i = 0; i < 10; i++)
            {
                sw.Write(dataGridView1[0, i].Value + "," + dataGridView1[1, i].Value + "," + dataGridView1[2, i].Value + "\n");
            }
            sw.Close();
        }
        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
        public float X(int num)
        {
            return float.Parse(this.dataGridView1[0, num].Value as string);
        }
        public float Y(int num)
        {
            return float.Parse(this.dataGridView1[1, num].Value as string);
        }
        public float Angle(int num)
        {
            return (float)Math.PI * float.Parse(this.dataGridView1[2, num].Value as string) / 180;
        }
    }
}
