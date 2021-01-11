using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;
using System.IO.Ports;
using System.Threading;

namespace fruit
{
    struct Data_r
    {
        public int SN, COMMAND, LENG, NO, TYP, ACK;
        public float  VALUE,FACTOR;
        public string NAME, UNITor;
    }
    public partial class Form1 : Form
    {
        private Data_r[] Data { get => data; set => data = value; }
        bool brun = false;
        bool bshow =false;
        bool bmodify = false;
        bool Initialized = false;
        bool sopen=false;
        string[]error1=new string[16];
        string[] error2= new string[3];
        Data_r[] data=new Data_r[200];
        float old_value, new_value;      
        byte[] sendbf = new byte[24];
        byte[] revbuffer=new byte[256];
        int sn;
        int sn1;
        int runnum;
        int mrow;
        int knum=0;
        int Num_time = 0;
        int Num_DSP=0;
        int n_dsp = 0;
        static int Set_Num_DSP = 2;//代表有机子数

        Form3 f3 = new Form3();
        private TestBind model = new TestBind();
        PSO_analysis PSO_v = new PSO_analysis();


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            model.NoticeHandler += new PropertyNoticeHandler(PropertyNoticeHandler);
            Binding bind2 = new Binding("Text", model, "Name");
            richTextBox1.DataBindings.Add(bind2);
            try
            {
                int u = 0;
                int j = 0;
                OleDbConnection conn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=MOON.mdb"); //Jet OLEDB:Database Password
                OleDbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select * from PARAMETER_RUN";
                conn.Open();
                u = j;
                OleDbDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    j = dr.GetInt32(dr.GetOrdinal("SN"));
                    data[j].SN = j;
                    try
                    {
                        data[j].NAME = dr.GetString(dr.GetOrdinal("NAME"));
                    }
                    catch
                    {
                        data[j].NAME = "";
                    }
                    data[j].LENG = dr.GetInt32(dr.GetOrdinal("leng"));
                    try
                    {
                        //data[j].VALUE = dr.GetInt16(dr.GetOrdinal("value"));
                        data[j].VALUE = dr.GetFloat(dr.GetOrdinal("value"));
                    }
                    catch
                    {
                        data[j].VALUE = 0;
                    }
                    data[j].COMMAND = dr.GetInt32(dr.GetOrdinal("COMMAND"));
                    data[j].NO = dr.GetInt32(dr.GetOrdinal("NO"));
                    try
                    {
                        data[j].UNITor = dr.GetString(dr.GetOrdinal("unit"));
                    }
                    catch
                    {
                        data[j].UNITor = "";
                    }
                    try
                    {
                        data[j].FACTOR = dr.GetFloat(dr.GetOrdinal("factor"));
                    }
                    catch
                    {
                        data[j].FACTOR = 0;
                    }
                }

                cmd.Dispose();
                conn.Close();


                for (int i = u; i <= j; i++)
                {
                    int index = this.dataGridView1.Rows.Add();
                    this.dataGridView1.Rows[index].Cells[0].Value = data[i].SN;
                    this.dataGridView1.Rows[index].Cells[1].Value = data[i].NAME;
                    this.dataGridView1.Rows[index].Cells[2].Value = data[i].VALUE;
                    if (data[i].FACTOR > 0.001)
                    {

                        //dataGridView1.Rows[index].Cells[3].Value = format('%.2f',[datas[j].FACTOR]) + data[j].UNITor;      //名称
                        dataGridView1.Rows[index].Cells[3].Value = Math.Round(data[i].FACTOR, 2) + data[i].UNITor;
                    }
                    else
                    {
                        dataGridView1.Rows[index].Cells[3].Value = data[i].UNITor;
                    }
                    runnum++;
                }
                u = j + 1;
                conn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=MOON.mdb"); //Jet OLEDB:Database Password
                cmd = conn.CreateCommand();
                cmd.CommandText = "select * from PARAMETER_SET";
                conn.Open();
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    j = dr.GetInt32(dr.GetOrdinal("SN"));
                    data[j].SN = j;
                    try
                    {
                        data[j].NAME = dr.GetString(dr.GetOrdinal("NAME"));
                    }
                    catch
                    {
                        data[j].NAME = "";
                    }
                    data[j].LENG = dr.GetInt32(dr.GetOrdinal("leng"));
                    try
                    {
                        //data[j].VALUE = dr.GetInt16(dr.GetOrdinal("value"));
                        data[j].VALUE = dr.GetFloat(dr.GetOrdinal("value"));
                    }
                    catch
                    {
                        data[j].VALUE = 0;
                    }
                    data[j].COMMAND = dr.GetInt32(dr.GetOrdinal("COMMAND"));
                    data[j].NO = dr.GetInt32(dr.GetOrdinal("NO"));
                    try
                    {
                        data[j].UNITor = dr.GetString(dr.GetOrdinal("unit"));
                    }
                    catch
                    {
                        data[j].UNITor = "";
                    }
                    try
                    {
                        data[j].FACTOR = dr.GetFloat(dr.GetOrdinal("factor"));
                    }
                    catch
                    {
                        data[j].FACTOR = 0;
                    }
                }

                cmd.Dispose();
                conn.Close();

                for (int i = u; i <= j; i++)
                {
                    if (data[i].SN != 0)
                    {
                        int index = this.dataGridView2.Rows.Add();
                        this.dataGridView2.Rows[index].Cells[0].Value = data[i].SN;
                        this.dataGridView2.Rows[index].Cells[1].Value = data[i].NAME;
                        this.dataGridView2.Rows[index].Cells[2].Value = data[i].VALUE;
                        if (data[i].FACTOR > 0.001)
                        {

                            //dataGridView2.Rows[index].Cells[3].Value = format('%.2f',[datas[j].FACTOR]) + data[j].UNITor;      //名称
                            dataGridView2.Rows[index].Cells[3].Value = Math.Round(data[i].FACTOR, 2) + data[i].UNITor;
                        }
                        else
                        {
                            dataGridView2.Rows[index].Cells[3].Value = data[i].UNITor;
                        }
                    }
                }


                u = j + 1;
                conn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=MOON.mdb"); //Jet OLEDB:Database Password
                cmd = conn.CreateCommand();
                cmd.CommandText = "select * from PARAMETER_FACTOR";
                conn.Open();
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    j = dr.GetInt32(dr.GetOrdinal("SN"));
                    data[j].SN = j;
                    try
                    {
                        data[j].NAME = dr.GetString(dr.GetOrdinal("NAME"));
                    }
                    catch
                    {
                        data[j].NAME = "";
                    }
                    try
                    {
                        //data[j].VALUE = dr.GetInt32(dr.GetOrdinal("value"));
                        data[j].VALUE = dr.GetFloat(dr.GetOrdinal("value"));
                    }
                    catch
                    {
                        data[j].VALUE = 0;
                    }
                    data[j].COMMAND = dr.GetInt32(dr.GetOrdinal("COMMAND"));

                }



                cmd.Dispose();
                conn.Close();

                for (int i = u; i < j; i++)
                {
                    int index = this.dataGridView3.Rows.Add();
                    this.dataGridView3.Rows[index].Cells[0].Value = data[i].SN;
                    this.dataGridView3.Rows[index].Cells[1].Value = data[i].NAME;
                    this.dataGridView3.Rows[index].Cells[2].Value = data[i].VALUE;
                }

                conn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=MOON.mdb"); //Jet OLEDB:Database Password
                cmd = conn.CreateCommand();
                cmd.CommandText = "select * from ERROR_1";
                conn.Open();
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    j = dr.GetInt32(dr.GetOrdinal("SN"));
                    try
                    {
                        error1[j] = dr.GetString(dr.GetOrdinal("name"));
                    }
                    catch
                    {
                        error1[j] = "";
                    }
                }

                cmd.Dispose();
                conn.Close();

                string query = "drop table PRUN";
                conn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=FRUIT.mdb"); //Jet OLEDB:Database Password
                cmd = conn.CreateCommand();
                cmd.CommandText = query;
                conn.Open();
                cmd.ExecuteNonQuery();

                query = "create table PRUN (id nvarchar(20) not null)";
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();

                for (int i = 0; i < runnum; i++)
                {
                    query = "alter table PRUN add column ";
                    query += data[i].NAME;
                    //query += " int";
                    query += " float";
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                }

                cmd.Dispose();
                conn.Close();
            }

            catch
            {
                MessageBox.Show("缺少MOON.mdb");
            }
            /*
            finally
            {
                CommonRes.serialPort1.DataReceived += SerialPort1_DataReceived;
            }
            */

            try
            {
                timer1.Interval = Convert.ToInt32(textBox2.Text);
            }
            catch
            {
                MessageBox.Show("非法的数值");
            }



            }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            if (ports.Length != 0)
            {
                Form2 f2 = new Form2();
                f2.Show();
            }
            else
            {
                MessageBox.Show("请插入串口设备！！");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            if (!sopen)
            {
                try
                {
                    //CommonRes.serialPort1.Open();
                    serialPort1.Open();
                    button2.Text = "关闭串口";
                    button3.Enabled = true;
                    button1.Enabled = false;
                    sopen = true;

                }
                catch
                {
                    MessageBox.Show("请插入串口设备！！");
                    sopen = false;
                }
                
            }
            else
            {
                //CommonRes.serialPort1.Close();
                serialPort1.Close();
                button2.Text = "打开串口";
                button3.Enabled = false;
                button1.Enabled = true;
                sopen = false;

                
            }

        }
       

        private void PropertyNoticeHandler(object handle, string proName)
        {
            try
            {
                BeginInvoke(
                    new Action(() => model.Bind(proName)));
            }
            catch
            {
            }
        }

        private void SerialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(10);
            Control.CheckForIllegalCrossThreadCalls = false;//没使用数据绑定前，此代码不可注释
            //throw new NotImplementedException();
            byte[] buffer = new byte[80];
            byte[] fbuffer = new byte[4];
            int i = 0, j = 0, k = 0;
            float s = 0;
            string str = "";
            Boolean found = false;

            try
            {
                j = serialPort1.Read(buffer, 0, buffer.Length);
            }
            catch
            {
                return;
            }


            for (i = 0; i < j; i++)
            {
                str += Convert.ToString(buffer[i], 16) + ' ';
            }
            str += '\r';
            //richTextBox1.Text += str;
            model.Name += str;


            if (j < buffer[4] + 5) //数据区尚未接收完整
            {
                return;
            }

            for (i = 0; i < 4; i++)
            {
                if (buffer[i] == 0xFE)
                    found = true;
                else
                    found = false;
            }

            
            if (found)
            {
                //j = buffer[4] + 5;//理论长度
                n_dsp = buffer[7];
                if (n_dsp == 2)
                {
                    n_dsp = 2;
                }
                //5个固定字节+4个格式字节，剩余20个字节代表5个浮点数

                if (buffer[5] == 200 && buffer[6] == 0xff && buffer[36] == 0xff)
                {
                    n_dsp = buffer[7];
                    if (n_dsp == 1 || n_dsp == 2)
                    {
                        for (int i_k = 0; i_k < 7; i_k++)//字节重新拼接为浮点数
                        {
                            fbuffer[0] = buffer[8 + 4 * i_k];
                            fbuffer[1] = buffer[9 + 4 * i_k];
                            fbuffer[2] = buffer[10 + 4 * i_k];
                            fbuffer[3] = buffer[11 + 4 * i_k];
                            PSO_v.u_dsp[n_dsp - 1, i_k] = BitConverter.ToSingle(fbuffer, 0);
                        }
                        //pso_data_flag = true;
                        if (n_dsp == Set_Num_DSP)
                        {

                            Thread th = new Thread(new ThreadStart(PSO_v.cale_pso)); //创建线程
                            th.Start(); //启动线程                          

                            Thread th1 = new Thread(new ThreadStart(update_UI)); //创建线程
                            th1.Start(); //启动线程                           

                        }
                    }
                }
                else
                {
                    //pso_data_flag = false;
                    //richTextBox1.Text += str;
                    j = buffer[4]+ 5;//理论长度
                    k = 0;
                    for (i = 0; i <= buffer[4]; i++)
                    {
                        k = buffer[4+i] + k;
                    }
                    k = (byte)(k);

                    if (k == 0)//校验通过
                    {
                        if (j == 13)//长度准确
                        {
                            fbuffer[0] = buffer[8];
                            fbuffer[1] = buffer[9];
                            fbuffer[2] = buffer[10];
                            fbuffer[3] = buffer[11];

                            s = BitConverter.ToSingle(fbuffer, 0);

                            data[buffer[5]].VALUE = s;

                            if (data[buffer[5]].SN ==buffer[5])
                            {
                                dataGridView1.Rows[data[buffer[5]].SN].Cells[2].Value = s;
                            }
                            data[buffer[5]].ACK = buffer[7];
                            if (buffer[5] == 29)//故障码1表
                            {
                                if (data[29].VALUE > 0)
                                {
                                    //richTextBox1.Text = "";
                                    model.Name = "";
                                    k = (int)data[29].VALUE;
                                    for (i = 0; i <= 15; i++)
                                    {
                                        j = k % 2;
                                        k = k >> 1;
                                        if (j > 0)
                                        {
                                            //richTextBox1.Text = "";
                                            //richTextBox1.Text = error1[i] + '\r';

                                            model.Name = "";
                                            model.Name = error1[i] + '\r';

                                            //memo1.Lines.Add(error1[i]);
                                        }
                                    }
                                }
                                else
                                {
                                    //richTextBox1.Text = "";
                                    model.Name = "";
                                }
                            }
                        }
                    }
                }

            }
        }

        private void update_UI()
        {
            try
            {
                PSO_send();

                f3.Text1 = PSO_v.Icbd1.ToString();
                f3.Text2 = PSO_v.Icbq1.ToString();
                f3.Text3 = PSO_v.Udp1.ToString();
                f3.Text4 = PSO_v.Uqp1.ToString();
                f3.Text5 = PSO_v.Uodp1.ToString();
                f3.Text6 = PSO_v.Uoqp1.ToString();
                f3.Text7 = PSO_v.w.ToString();

                f3.Text8 = PSO_v.Icbd2.ToString();
                f3.Text9 = PSO_v.Icbq2.ToString();
                f3.Text10 = PSO_v.Udp2.ToString();
                f3.Text11 = PSO_v.Uqp2.ToString();
                f3.Text12 = PSO_v.Uodp2.ToString();
                f3.Text13 = PSO_v.Uoqp2.ToString();
                f3.Text14 = PSO_v.w.ToString();

                f3.Text15 = PSO_v.Vd1.ToString();
                f3.Text16 = PSO_v.Vq1.ToString();
                f3.Text17 = PSO_v.Vd2.ToString();
                f3.Text18 = PSO_v.Vq2.ToString();
            }
            catch
            {
                MessageBox.Show("只是停快了，小场面！！！");
            }

        }

        private void PSO_send()
        {
            //将计算完成的数据存入发送数组中
            //回发数据为了保持两台机子同步接收，不区分逆变器，数据同时下发，逆变器侧自行根据位置进行接收

            sendbf[0] = 0xfe;
            sendbf[1] = 0xfe;
            sendbf[2] = 0xfe;
            sendbf[3] = 0xfe;
            sendbf[4] = 19;//4个浮点数，16个字节，此外还有序列号+命令码+校验码，共19字节
            sendbf[5] = 200;
            sendbf[6] = 0xff;

            for (int i_n = 0; i_n < 4; i_n++)
            {
                byte[] temp_i = BitConverter.GetBytes(PSO_v.u_g[i_n]);
                sendbf[7 + 4 * i_n] = temp_i[0];
                sendbf[8 + 4 * i_n] = temp_i[1];
                sendbf[9 + 4 * i_n] = temp_i[2];
                sendbf[10 + 4 * i_n] = temp_i[3];
            }
            sendbf[23] = 0xff;

            //加5是还有包头4字节，以及长度19字节
            serialPort1.Write(sendbf, 0, sendbf[4] + 5);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //if(CommonRes.serialPort1.IsOpen)
            if (serialPort1.IsOpen)
            {
                //timer1.Enabled = true;
                button3.Enabled = false;
                //bshow = true;
            }
            else
            {
                MessageBox.Show("串口未打开");
                //timer1.Enabled = false;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Thread th = new Thread(new ThreadStart(test)); //创建线程
            th.Start(); //启动线程
            PSO_v.pso_init = false;

            int crc;
            //if (!CommonRes.serialPort1.IsOpen)
            if (!serialPort1.IsOpen)
            {
                MessageBox.Show("请打开串口！");
                return;
            }
            //timer1.Enabled = false;
            
            sendbf[0] = 0xfe;
            sendbf[1] = 0xfe;
            sendbf[2] = 0xfe;
            sendbf[3] = 0xfe;
            sendbf[4] = 5;
            sendbf[5] = 2;
            sendbf[6] = 177;

            brun =!brun;
            if(brun)
            {
                textBox1.Text = "系统正在运行";
                button4.Text = "停止";
                sendbf[7] = 1;                
            }
            else
            {
                textBox1.Text = "系统停止运行";
                button4.Text = "运行";
                sendbf[7] = 0;
                timer3.Enabled = false;
            }
            Num_time = 0;
            //Num_time_first = 0;
            sendbf[8] = 0;
            crc = sendbf[4] + sendbf[5] + sendbf[6] + sendbf[7] + sendbf[8];
            crc = ~crc + 1;
            sendbf[9] =(byte)crc;
            //CommonRes.serialPort1.Write(sendbf, 0, 10);
            serialPort1.Write(sendbf, 0, 10);

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    //richTextBox1.Text += Convert.ToString(sendbf[i], 16);
                    //richTextBox1.Text += ' ';

                    model.Name+= Convert.ToString(sendbf[i], 16);
                    model.Name += ' ';

                }
                catch
                {
                    MessageBox.Show("停机冲突");
                }
            }
            //richTextBox1.Text += '\r';
            model.Name+= '\r';


            Array.Clear(sendbf, 0, 19);

            /*
            if(brun)
            {
                timer3.Enabled = true;
            }
            else
            {
                timer3.Enabled = false;
            }
            */
            //richTextBox1.Text += '\n';
            model.Name += '\n';
            //timer2.Enabled = true;
        }

        private void test()
        {
            if(brun)
            { 
                MessageBox.Show("徐师姐厉害"); 
            }
            else
            {
                MessageBox.Show("花钱才能看到源代码");
            }
            
            return;
        }

        private void MakeCommand(int sn, byte command, float data)
        {
            
            int crc;
            sendbf[0] = 0xfe;
            sendbf[1] = 0xfe;
            sendbf[2] = 0xfe;
            sendbf[3] = 0xfe;
            if (sn > 43)  //调试参数 + 修正系数 + 运行模式命令
            {
                if (command == 10)
                {
                    sendbf[4]= 3; crc= sendbf[4];
                    sendbf[5]= (byte)sn; crc= crc + sendbf[5];
                    sendbf[6]= command; crc= crc + sendbf[6];
                    sendbf[7]= (byte)(0 - crc);
                }
                else
                {
                    sendbf[4]= 7; crc= sendbf[4];
                    sendbf[5]= (byte)sn; crc= crc + sendbf[5];
                    sendbf[6]= command; crc= crc + sendbf[6];

                    //sendbf[7]= (byte)data; crc= crc + sendbf[7];
                    //sendbf[8]= (byte)(data>>8); crc= crc + sendbf[8];

                    byte[] temp_i = BitConverter.GetBytes(data);
                    sendbf[7] = temp_i[0]; crc = crc + sendbf[7];
                    sendbf[8] = temp_i[1]; crc = crc + sendbf[8];
                    sendbf[9] = temp_i[2]; crc = crc + sendbf[9];
                    sendbf[10] = temp_i[3]; crc = crc + sendbf[10];

                    sendbf[11]= (byte)(0 - crc);
                }
            }
            else  //运行参数if (sn > -1)
            {
                sendbf[4]= 3; crc= sendbf[4];
                sendbf[5]= (byte)sn; crc= crc + sendbf[5];
                sendbf[6]= command; crc= crc + sendbf[6];
                sendbf[7]= (byte)(0 - crc);
            }
        }

        private void RceCommand(int start)
        {
            int crc;

            sendbf[0]=0xfe; 
            sendbf[1]=0xfe; 
            sendbf[2]=0xfe;
            sendbf[3]=0xfe;
            sendbf[5]= revbuffer[(start + 5)%256]; crc= sendbf[5];      //序号
            sendbf[6]= revbuffer[(start + 6)%256]; crc= crc + sendbf[6];  //命令码
            sendbf[7]= 1; crc= crc + sendbf[7];  //确认码
            if (revbuffer[(start + 4)%256] == 3)
            {

                sendbf[4] = 6; crc= crc + sendbf[4];  //包长
                sendbf[8]= (byte)(data[(start + 5)%256].SN); crc= crc + sendbf[8];
                sendbf[9]= 0; crc= crc + sendbf[9];
                sendbf[10]= (byte)(0 - crc);
            }
            else if (revbuffer[(start + 4)%256] == 5)
            {
                sendbf[4]= 4;
                crc= crc + sendbf[4];  //包长
                sendbf[8]= (byte)(0 - crc);
            }
            try
            {
                //CommonRes.serialPort1.Write(sendbf, 0, sendbf[4] + 7);
                serialPort1.Write(sendbf, 0, sendbf[4] + 7);
            }
            catch
            {
                return;
            }
            
        }



        private void timer1_Tick(object sender, EventArgs e)
        {
            int crc = 0;
            float i = 0;
            float sdata;
            //Array.Clear(sendbf, 0, 19);
            //if (CommonRes.serialPort1.IsOpen)
            if (serialPort1.IsOpen)
            {
                if (!Initialized)
                {
                    Initialized = true;//若需要启用初始化检测，应注释这句话
                    if (sn > 43)
                    {
                        sdata = data[sn].VALUE;
                        MakeCommand(sn, 0, sdata);
                    }
                    if (data[sn].ACK == -2)
                        data[sn].ACK = -1;
                    else if (data[sn].ACK == -1)
                    {
                        data[sn].ACK = 0;
                        sn++;
                    }
                    else
                        sn++;

                    if (sn == 118)
                    {
                        sn = 0;
                        Initialized = true;
                        MessageBox.Show("参数初始化完成！！");
                    }
                }
                else
                {
                    if (sn1 > -1)
                    {
                        sendbf[0] = 0xfe;
                        sendbf[1] = 0xfe;
                        sendbf[2] = 0xfe;
                        sendbf[3] = 0xfe;
                        sendbf[4] = 7; crc = sendbf[4];
                        sendbf[5] = (byte)sn1; crc = crc + sendbf[5];
                        switch (sn1)
                        {
                            case 0: sendbf[6] = 175; break;
                            case 1: sendbf[6] = 176; break;
                            case 2: sendbf[6] = 177; break;
                            case 3: sendbf[6] = 178; break;
                            default: break;
                        }
                        crc = crc + sendbf[6];
                        i = data[sn1].VALUE;

                        //sendbf[7] = (byte)(i); crc = crc + sendbf[7];
                        //sendbf[8] = (byte)(i >> 8); crc = crc + sendbf[8];
                        byte[] temp_i = BitConverter.GetBytes(i);
                        sendbf[7] = temp_i[0]; crc = crc + sendbf[7];
                        sendbf[8]= temp_i[1]; crc = crc + sendbf[8];
                        sendbf[9] = temp_i[2]; crc = crc + sendbf[9];
                        sendbf[10] = temp_i[3]; crc = crc + sendbf[10];
                        sendbf[11] = (byte)(0 - crc);
                        sn1 = -1;
                    }

                    else
                    {
                        sendbf[0] = 0xfe;
                        sendbf[1] = 0xfe;
                        sendbf[2] = 0xfe;
                        sendbf[3] = 0xfe;
                        sendbf[4] = 3; crc = sendbf[4];
                        sendbf[5] = (byte)sn; crc = crc + sendbf[5];
                        sendbf[6] = (byte)data[sn].COMMAND; crc = crc + sendbf[6];
                        sendbf[7] = (byte)(0 - crc);
                        //sn=(sn+1) % 44;
                        //sn = (sn + 1) % runnum;
                        sn = sn + 1;

                    }
                }

                //if (sn <=runnum || sn > 43 || sn == 29 || sn == 30)
                {
                    //CommonRes.serialPort1.Write(sendbf, 0, sendbf[4] + 5);
                    serialPort1.Write(sendbf, 0, sendbf[4] + 5);
                }


                //if (sn == 0)
                if (sn == runnum)
                {
                    //CommonRes.serialPort1.Write(sendbf, 0, sendbf[4] + 5);
                    sn = 0;
                    OleDbConnection conn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=fruit.mdb"); //Jet OLEDB:Database Password
                    conn.Open();
                    
                    //string sql = "insert into PRUN(gspeed,id,iq,rspeed,rid,riq,ud,uq,us) values(";
                    string sql = "insert into PRUN values (";
                    sql += knum+",";
                    int a;
                    for (a = 0; a < runnum; a++)
                    {
                        sql += data[a].VALUE;
                        if (a !=runnum-1)
                        {
                            sql += ",";
                        }
                    }
                    sql += ")";
                    //string sql = "insert into PRUN(gspeed) values(" + data[2].VALUE + ")";

                    OleDbCommand cmd = new OleDbCommand(sql, conn);
                    cmd.ExecuteNonQuery();

                    conn.Close();
                    knum++;
                }
                //else
                ////if (sn <=runnum || sn > 43 || sn == 29 || sn == 30)
                //{
                //    CommonRes.serialPort1.Write(sendbf, 0, sendbf[4] + 5);
                //}
               


            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            timer2.Enabled = false;
        }



        private void dataGridView3_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            //old_value = Convert.ToInt32(dataGridView3.Rows[e.RowIndex].Cells[2].Value);//旧值一定是对的，不对的话也不是这里处理
            old_value = Convert.ToSingle(dataGridView3.Rows[e.RowIndex].Cells[2].Value);//旧值一定是对的，不对的话也不是这里处理
        }

        private void dataGridView3_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                //new_value = Convert.ToInt32(dataGridView3.Rows[e.RowIndex].Cells[2].Value);
                new_value = Convert.ToSingle(dataGridView3.Rows[e.RowIndex].Cells[2].Value);
                mrow = e.RowIndex;
            }

            catch (OverflowException)
            {
                MessageBox.Show("err:转化的不是一个float型数据");
                dataGridView3.Rows[e.RowIndex].Cells[2].Value = old_value;
                return;
            }
            catch (FormatException)
            {
                MessageBox.Show("err:格式错误");
                dataGridView3.Rows[e.RowIndex].Cells[2].Value = old_value;
                return;
            }
            catch (ArgumentNullException)
            {
                MessageBox.Show("err:null");
                dataGridView3.Rows[e.RowIndex].Cells[2].Value = old_value;
                return;
            }

            //if (new_value != old_value)
            //{
                //Column10.DefaultCellStyle.NullValue = "修改";
                bmodify = true;
                dataGridView3.Rows[e.RowIndex].Cells[4].Value = "修改";
            //}
            //else
            //{
            //    bmodify = false;
                //Column10.DefaultCellStyle.NullValue = "";
            //    dataGridView3.Rows[e.RowIndex].Cells[4].Value = "";
            //}
        }

        private void dataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            byte tempsn;
            float i;
            int crc;
            //if(!CommonRes.serialPort1.IsOpen)
            if (!serialPort1.IsOpen)
            {
                bmodify = false;
                dataGridView3.Rows[e.RowIndex].Cells[2].Value = old_value;
                dataGridView3.Rows[e.RowIndex].Cells[4].Value = "";
                return;
            }
            if (e.ColumnIndex == 4)
            {
                if (e.RowIndex == mrow)
                {
                    if (bmodify)
                    {
                        //timer1.Enabled = false;
                        Array.Clear(sendbf, 0, sendbf.Length);
                        sendbf[0] = 0xfe;
                        sendbf[1] = 0xfe;
                        sendbf[2] = 0xfe;
                        sendbf[3] = 0xfe;

                        //i = Convert.ToInt16(dataGridView3.Rows[e.RowIndex].Cells[2].Value);
                        i = Convert.ToSingle(dataGridView3.Rows[e.RowIndex].Cells[2].Value);

                        tempsn = Convert.ToByte(dataGridView3.Rows[e.RowIndex].Cells[0].Value);
                        sendbf[4] = 7; crc = sendbf[4];
                        sendbf[5] = tempsn; crc = crc + sendbf[5];
                        sendbf[6] = (byte)(data[tempsn].COMMAND); crc = crc + sendbf[6];

                        //sendbf[7] = (byte)(i); crc = crc + sendbf[7]; i = i - sendbf[7]; i = i >> 8;
                        //sendbf[8] = (byte)(i); crc = crc + sendbf[8];
                        //sendbf[9] = (byte)(0 - crc);

                        byte[] temp_i = BitConverter.GetBytes(i);
                        sendbf[7] = temp_i[0]; crc = crc + sendbf[7];
                        sendbf[8] = temp_i[1]; crc = crc + sendbf[8];
                        sendbf[9] = temp_i[2]; crc = crc + sendbf[9];
                        sendbf[10] = temp_i[3]; crc = crc + sendbf[10];
                        sendbf[11] = (byte)(0 - crc);


                        //CommonRes.serialPort1.Write(sendbf, 0, sendbf[4] + 5);
                        serialPort1.Write(sendbf, 0, sendbf[4] + 5);
                        Array.Clear(sendbf, 0, 19);
                        //Column10.DefaultCellStyle.NullValue = "";
                        dataGridView3.Rows[e.RowIndex].Cells[4].Value = "";
                        bmodify = false;
                        //timer2.Enabled = true;
                    }
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            bshow = !bshow;
            if(bshow)
            {
                timer1.Enabled = true;
                button5.Text = "停止采集";
            }
            else
            {
                timer1.Enabled = false;
                button5.Text = "数据采集";
            }
        }


        private void dataGridView2_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            //old_value = Convert.ToInt32(dataGridView2.Rows[e.RowIndex].Cells[2].Value);//旧值一定是对的，不对的话也不是这里处理
            old_value = Convert.ToSingle(dataGridView2.Rows[e.RowIndex].Cells[2].Value);//旧值一定是对的，不对的话也不是这里处理
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            byte tempsn;
            float i;
            float new_v;
            int crc;
            //if (!CommonRes.serialPort1.IsOpen)
            if (!serialPort1.IsOpen)
            {
                bmodify = false;
                dataGridView2.Rows[e.RowIndex].Cells[2].Value = old_value;
                dataGridView2.Rows[e.RowIndex].Cells[4].Value = "";
                return;
            }
            if (e.ColumnIndex == 4)
            {
                if (e.RowIndex == mrow)
                {
                    if (bmodify)
                    {
                        //timer1.Enabled = false;
                        Array.Clear(sendbf, 0, sendbf.Length);
                        sendbf[0] = 0xfe;
                        sendbf[1] = 0xfe;
                        sendbf[2] = 0xfe;
                        sendbf[3] = 0xfe;
                        new_v=i = Convert.ToSingle(dataGridView2.Rows[e.RowIndex].Cells[2].Value);
                        tempsn = Convert.ToByte(dataGridView2.Rows[e.RowIndex].Cells[0].Value);
                        sendbf[4] = 7; crc = sendbf[4];
                        sendbf[5] = tempsn; crc = crc + sendbf[5];
                        sendbf[6] = (byte)(data[tempsn].COMMAND); crc = crc + sendbf[6];

                        //sendbf[7] = (byte)(i); crc = crc + sendbf[7]; i = i - sendbf[7]; i = i >> 8;
                        //sendbf[8] = (byte)(i); crc = crc + sendbf[8];

                        //byte[] temp_i = BitConverter.GetBytes(i);
                        //sendbf[7] = temp_i[0]; crc = crc + sendbf[7];
                        //sendbf[8] = temp_i[1]; crc = crc + sendbf[8];
                        //sendbf[9] = (byte)(0 - crc);
                        byte[] temp_i = BitConverter.GetBytes(i);
                        sendbf[7] = temp_i[0]; crc = crc + sendbf[7];
                        sendbf[8] = temp_i[1]; crc = crc + sendbf[8];
                        sendbf[9] = temp_i[2]; crc = crc + sendbf[9];
                        sendbf[10] = temp_i[3]; crc = crc + sendbf[10];
                        sendbf[11] = (byte)(0 - crc);

                        //CommonRes.serialPort1.Write(sendbf, 0, sendbf[4] + 5);
                        serialPort1.Write(sendbf, 0, sendbf[4] + 5);


                        OleDbConnection conn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=MOON.mdb"); //Jet OLEDB:Database Password
                        conn.Open();

                        string sql = "update PARAMETER_SET set [VALUE]=" + new_v + " where SN=" + tempsn;


                        OleDbCommand cmd = new OleDbCommand(sql, conn);
                        cmd.ExecuteNonQuery();

                        conn.Close();

                        Array.Clear(sendbf, 0, 19);
                        //Column10.DefaultCellStyle.NullValue = "";
                        dataGridView2.Rows[e.RowIndex].Cells[4].Value = "";
                        bmodify = false;
                        //timer2.Enabled = true;
                    }
                }
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.SelectionStart = richTextBox1.Text.Length; //Set the current caret position at the end
            richTextBox1.ScrollToCaret(); //Now scroll it automatically
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                timer1.Interval = Convert.ToInt32(textBox2.Text);
            }
            catch
            {
                MessageBox.Show("非法的数值");
            }
        }

        private void Form1_DoubleClick(object sender, EventArgs e)
        {
            MessageBox.Show("PSO 1.0版本！");
        }

        private void timer3_Tick(object sender, EventArgs e)
        {

            //if (CommonRes.serialPort1.IsOpen)
            if (serialPort1.IsOpen)
            {             
                Num_time++;             
                if (Num_time == 2)//起始第一段数据发送的时间
                {
                    int crc = 0;
                    Num_time = 0;

                    Num_DSP = Num_DSP+ 1;
                    if (Num_DSP > 2)
                        Num_DSP = 1;
                    Array.Clear(sendbf, 0, 19);
                    sendbf[0] = 0xfe;
                    sendbf[1] = 0xfe;
                    sendbf[2] = 0xfe;
                    sendbf[3] = 0xfe;
                    sendbf[4] = 4; crc = sendbf[4];
                    sendbf[5] = 200; crc = crc + sendbf[5];
                    sendbf[6] = 0xff; crc = crc + sendbf[6];
                    sendbf[7] = (byte)Num_DSP; crc = crc + sendbf[7];
                    sendbf[8] = (byte)(0 - crc);
                    //CommonRes.serialPort1.Write(sendbf, 0, sendbf[4] + 5);
                    serialPort1.Write(sendbf, 0, sendbf[4] + 5);
                }         

            }

            //test_num++;         
            //f3.Text1 = "90";
            //f3.Text2 = "91";
            //f3.Text3 = "93";
            //f3.Text4 = "94";
            //f3.Text5 = test_num.ToString();
            
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (brun)
            {
                timer3.Enabled = !timer3.Enabled;
                if(timer3.Enabled==true)
                {
                    MessageBox.Show("PSO启动");
                    f3 = new Form3();
                    f3.Text1 = "hi";
                    f3.Show();
                }
                else
                {
                    MessageBox.Show("PSO结束");
                    
                }
            }
        }


        private void dataGridView2_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                //new_value = Convert.ToInt32(dataGridView2.Rows[e.RowIndex].Cells[2].Value);
                new_value = Convert.ToSingle(dataGridView2.Rows[e.RowIndex].Cells[2].Value);
                mrow = e.RowIndex;
            }

            catch (OverflowException)
            {
                MessageBox.Show("err:转化的不是一个float型数据");
                dataGridView2.Rows[e.RowIndex].Cells[2].Value = old_value;
                return;
            }
            catch (FormatException)
            {
                MessageBox.Show("err:格式错误");
                dataGridView2.Rows[e.RowIndex].Cells[2].Value = old_value;
                return;
            }
            catch (ArgumentNullException)
            {
                MessageBox.Show("err:null");
                dataGridView2.Rows[e.RowIndex].Cells[2].Value = old_value;
                return;
            }

            //if (new_value!=old_value)
            {
                //Column10.DefaultCellStyle.NullValue = "修改";
                bmodify = true;
                dataGridView2.Rows[e.RowIndex].Cells[4].Value ="修改";
            }
            /*
            else
            {
                bmodify = false;
                //Column10.DefaultCellStyle.NullValue = "";
                dataGridView2.Rows[e.RowIndex].Cells[4].Value = "";
            }
            */
        }

        public string Serial_Port
        {
            get { return this.serialPort1.PortName; }
            set { this.serialPort1.PortName  = value; }
        }

        public int Serial_Baud
        {
            get { return this.serialPort1.BaudRate; }
            set { this.serialPort1.BaudRate = value; }
        }

        public Parity Serial_Parity
        {
            get { return this.serialPort1.Parity; }
            set { this.serialPort1.Parity = value; }
        }

        public int Serial_DataBits
        {
            get { return this.serialPort1.DataBits; }
            set { this.serialPort1.DataBits = value; }
        }

        public StopBits Serial_StopBits
        {
            get { return this.serialPort1.StopBits; }
            set { this.serialPort1.StopBits = value; }
        }
    }

    public class TestBind : INotifyPropertyChanged
    {
        private string name;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                SendChangeInfo("Name");
            }
        }

        private int value;

        public int Value
        {
            get { return value; }
            set
            {
                this.value = value;
                SendChangeInfo("Value");
            }
        }

        private void SendChangeInfo(string propertyName)
        {
            if (NoticeHandler != null)
            {
                NoticeHandler(this, propertyName);
            }
        }

        public void Bind(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

        }
        public event PropertyNoticeHandler NoticeHandler;
        public event PropertyChangedEventHandler PropertyChanged;
    }

    public delegate void PropertyNoticeHandler(object handle, string proName);
}
