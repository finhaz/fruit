using System;
using System.IO.Ports;
using System.Windows.Forms;

namespace fruit
{


    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {

            CommonRes.serialPort1.PortName = comboBox1.SelectedItem.ToString();
            CommonRes.serialPort1.BaudRate= Convert.ToInt32(comboBox2.SelectedItem.ToString());
            //CommonRes.serialPort1.Parity = (Parity)Convert.ToInt32(comboBox3.SelectedIndex.ToString());
            switch (comboBox3.SelectedItem.ToString())
            {
                case "无": CommonRes.serialPort1.Parity = Parity.None; break;
                case "奇校验": CommonRes.serialPort1.Parity = Parity.Even; break;
                case "偶校验": CommonRes.serialPort1.Parity = Parity.Odd; break;
                default: CommonRes.serialPort1.Parity = Parity.None; break;
            }
            CommonRes.serialPort1.DataBits = Convert.ToInt32(comboBox4.SelectedItem.ToString());
            CommonRes.serialPort1.StopBits = (StopBits)Convert.ToInt32(comboBox5.SelectedItem.ToString());
            
            this.Close();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            comboBox1.Items.AddRange(SerialPort.GetPortNames());
            comboBox1.SelectedItem = comboBox1.Items[0];

            int[] item = { 2400, 4800, 9600, 115200 };    //定义一个Item数组，遍历item中每一个变量a，增加到comboBox2的列表中
            foreach (int a in item)
            {
                comboBox2.Items.Add(a.ToString());
            }
            comboBox2.SelectedItem = comboBox2.Items[3];    //默认为列表第二个变量  

            string[] item1 = { "无", "奇校验","偶校验" };    //定义一个Item数组，遍历item中每一个变量a，增加到comboBox2的列表中
            foreach (string a in item1)
            {
                comboBox3.Items.Add(a);
            }
            comboBox3.SelectedItem = comboBox3.Items[0];    //默认为列表第二个变量  

            int[] item2 = { 8, 7, 6, 5 };    //定义一个Item数组，遍历item中每一个变量a，增加到comboBox2的列表中
            foreach (int a in item2)
            {
                comboBox4.Items.Add(a.ToString());
            }
            comboBox4.SelectedItem = comboBox4.Items[0];    //默认为列表第二个变量  


            double [] item3 = { 1,1.5,2 };    //定义一个Item数组，遍历item中每一个变量a，增加到comboBox2的列表中
            foreach (double a in item3)
            {
                comboBox5.Items.Add(a.ToString());
            }
            comboBox5.SelectedItem = comboBox5.Items[0];    //默认为列表第二个变量  
            
        }
    }
}
