using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Summoning
{
    public partial class Launcher : Form
    {
        public Launcher()
        {
            InitializeComponent();

            String maps = new System.IO.FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).Directory + "\\Resources\\Maps";
            foreach (var file in System.IO.Directory.GetFiles(maps))
            {
                FileInfo fileInfo = new FileInfo(file);
                this.comboBox1.Items.Add(fileInfo.Name);
            }
        }

        private void Launcher_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(!String.IsNullOrEmpty(this.comboBox1.Text))
            {
                var gameFrame = new Form1(this.comboBox1.Text);
                //gameFrame.TopMost = true;
                //gameFrame.WindowState = FormWindowState.Maximized;
                //gameFrame.FormBorderStyle = FormBorderStyle.None;
                //gameFrame.StartPosition = FormStartPosition.CenterScreen;
                gameFrame.Show();
            }
            else
            {
                MessageBox.Show("Please select an level for playing");
            }
        }
    }
}
