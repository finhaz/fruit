using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace fruit
{
    public partial class Form4 : Form
    {
        Form1 f1 = new Form1();
//        bool flag_under_first = false;
//        bool flag_upper_first = false;
        int sn=44;

        public Form4(Form1 F1)
        {
            InitializeComponent();
            f1 = F1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            f1.Show();
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            f1.flag_upper_first = true;
            f1.sn = 44;
        }


        private void button3_Click(object sender, EventArgs e)
        {
            f1.flag_under_first = true;
            f1.sn = 44;
        }


    }
}
