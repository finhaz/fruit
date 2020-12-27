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
        //byte[] sendbuffer=new byte[256];
        int sn;
        int sn1;
        int runnum;
        int nill;
        int head;
        int mrow;
        int knum=0;
        int Num_time = 0;
        int Num_time_first = 0;
        int Num_DSP=0;
        int n_dsp = 0;
        static int Set_Num_DSP = 2;//代表有机子数
        //float [,]u_dsp=new float[Set_Num_DSP, 10];
        float[,] u_dsp = new float[2, 10];
        float[] u_g = new float[4];

        //PSO所需变量
        double c1 = 1.49445;     // 学习因子
        double c2 = 1.49445;     // 学习因子
        int maxgen = 300;    //粒子群迭代次数
        int sizepop = 20;     //粒子数目
        double popmax = 100;      //粒子位置的最大值，即解的最大允许值 
        double popmin = -100;     //粒子位置的最小值，解的最小允许值
        double Vmax = 2;      // 粒子最大速度，粒子速度的大小和精度有关
        double Vmin = -2;     //粒子最小速度
        //int t_n = 0;
        //int n_tri = 50000;//用来计数，每隔0.05s粒子群算法运行一次

        bool pso_init = false;//初始化完成标志
        bool pso_data_flag = false;

        double Icbd1 ;
        double Icbq1 ;
        double Icbd2 ;
        double Icbq2 ;
        double Udp1;
        double Uqp1;
        double Udp2;
        double Uqp2;
        double Uodp1;
        double Uoqp1;
        double Uodp2;
        double Uoqp2;

        double[] hbest = new double[13];


        double[] fitnessgbest = new double[20];          //个体粒子中达到最小值时的适应度值
        double[,] gbest = new double[20, 4];                 //个体粒子达到极值时对应的最优解位置
        double fitnesszbest = 0;          //群体粒子达到极值时的适应度值
        double[] zbest = new double[4];                 //群体粒子达到极值时对应解的位置
        double[] fitness = new double[20];  //矩阵，i个粒子的适应度值                      
        double[,] pop = new double[20, 4];//矩阵，用来存放粒子的位置，粒子的解
        double[,] V = new double[20, 4];  //矩阵，用来存在粒子的速度

        double[] u = new double[5];//代表输入参数
        //double pi = 3.1415926;

        Random ran = new Random();

/*
        //两台逆变器dq坐标系下的负序电流输入以及频率
        
        double Icbd1 = u[0];
        double Icbq1 = u[1];
        double Icbd2 = u[2];
        double Icbq2 = u[3];
        double w = 2 * pi * u[4];
        

        double Vd1;
        double Vq1;
        double Vd2;
        double Vq2;
*/



        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

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
            finally
            {
                CommonRes.serialPort1.DataReceived += SerialPort1_DataReceived;
            }

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
                    CommonRes.serialPort1.Open();
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
                CommonRes.serialPort1.Close();
                button2.Text = "打开串口";
                button3.Enabled = false;
                button1.Enabled = true;
                sopen = false;

                
            }

        }
        /*
        private void SerialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            //throw new NotImplementedException();
            byte[] buffer = new byte[80];
            byte[] fbuffer = new byte[4];
            int i=0, j=0, k=0;
            float s=0;
            string str="";
            Boolean found=false;
            
            try
            {
                j=CommonRes.serialPort1.Read(buffer, 0, buffer.Length);
            }
            catch
            {
                return;
            }

            for (i = 0; i < j; i++)
            {
                revbuffer[nill] = buffer[i];
                str += Convert.ToString(revbuffer[nill], 16) + ' ';
                nill = (nill + 1) % 256;
            }
            str += '\r';
            richTextBox1.Text += str;

            i = nill - head;//数据长度
            if( i<0)
                i = i + 256;
            //found= false;
            while ((i > 7) && !found)//为了确认数据头
            {
                if (revbuffer[head] == 0xFE)
                    if (revbuffer[(head + 1) % 256] ==0xFE)
                        if (revbuffer[(head + 2)%256] ==0xFE) 
                            if(revbuffer[(head + 3)% 256] ==0xFE) 
                                        found= true;
                if (!found)
                {
                    head= (head + 1) %256;
                    i = i - 1;
                }
            }
            if ((i > 7) && found)
            {
                //j = (revbuffer[(head + 4) % 256] + 5) % 256;//理论长度
                n_dsp = revbuffer[(head + 7) % 256];
                if(n_dsp==2)
                {
                    n_dsp = 2;
                }
                //5个固定字节+4个格式字节，剩余40个字节代表10个浮点数
                           
                if (revbuffer[(head + 5) % 256]==200&& revbuffer[(head + 6) % 256] == 0xff && revbuffer[(head + 48) % 256] == 0xff)
                {
                    n_dsp=revbuffer[(head + 7) % 256];
                    if (n_dsp ==1||n_dsp==2)
                    {
                        for (int i_k = 0; i_k < 10; i_k++)//字节重新拼接为浮点数
                        {
                            fbuffer[0] = revbuffer[(head + 8 + 4 * i_k) % 256];
                            fbuffer[1] = revbuffer[(head + 9 + 4 * i_k) % 256];
                            fbuffer[2] = revbuffer[(head + 10 + 4 * i_k) % 256];
                            fbuffer[3] = revbuffer[(head + 11 + 4 * i_k) % 256];
                            u_dsp[n_dsp - 1, i_k] = BitConverter.ToSingle(fbuffer, 0);
                        }
                        pso_data_flag = true;
                    }
                }
                else
                {
                    pso_data_flag = false;
                    //richTextBox1.Text += str;
                    j = (revbuffer[(head + 4) % 256] + 5) % 256;//理论长度
                    if (i >= j)
                        k = 0;
                    for (i = 0; i <= revbuffer[(head + 4)%256]; i++) 
                    {
                        k = revbuffer[(head + 4 + i) % 256] + k;
                    }
                    k = (byte)(k);


                    if (k == 0)//校验通过
                    {
                        if (j == 13)//长度准确
                        {
                            //s = revbuffer[(head + 9)%256];
                            //s = (short)((s << 8) + revbuffer[(head + 8)%256]);

                            //s = BitConverter.ToInt16(revbuffer, (head + 8) % 256);
                            //s = BitConverter.ToSingle(revbuffer, (head + 8) % 256);
                            fbuffer[0] = revbuffer[(head + 8) % 256];
                            fbuffer[1] = revbuffer[(head + 9) % 256];
                            fbuffer[2] = revbuffer[(head + 10) % 256];
                            fbuffer[3] = revbuffer[(head + 11) % 256];

                            s = BitConverter.ToSingle(fbuffer, 0);

                            data[revbuffer[(head + 5)%256]].VALUE = s;

                            if (data[revbuffer[(head + 5) % 256]].SN == revbuffer[(head + 5) % 256])
                            {
                                dataGridView1.Rows[data[revbuffer[(head + 5) % 256]].SN].Cells[2].Value = s;


                            }
                            data[revbuffer[(head + 5)%256]].ACK = revbuffer[(head + 7)%256];
                            if (revbuffer[(head + 5)%256] == 29)//故障码1表
                            { 
                                if (data[29].VALUE > 0)
                                {
                                    richTextBox1.Text = "";
                                    k = (int)data[29].VALUE;
                                    for (i = 0; i <= 15; i++)
                                    {
                                        j = k % 2;
                                        k = k >> 1;
                                        if( j > 0 )
                                        {
                                            richTextBox1.Text = "";
                                            richTextBox1.Text = error1[i]+'\r';
                                        
                                            //memo1.Lines.Add(error1[i]);
                                        }
                                    }
                                }
                                else
                                {
                                    richTextBox1.Text = "";
                                }
                            }
                        }
                        //RceCommand(head);
                        head = (head + j) % 256;
                        if (head == nill)
                        {
                            head = 0;
                            nill = 0;
                        }
                        else
                        {
                            head = (head + 1)%256;
                        }
                    }
                }

            }

           


        }

        */

        private void SerialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(10);
            Control.CheckForIllegalCrossThreadCalls = false;
            //throw new NotImplementedException();
            byte[] buffer = new byte[80];
            byte[] fbuffer = new byte[4];
            int i = 0, j = 0, k = 0;
            float s = 0;
            string str = "";
            Boolean found = false;

            try
            {
                j = CommonRes.serialPort1.Read(buffer, 0, buffer.Length);
            }
            catch
            {
                return;
            }


            if (j != buffer[4] + 5) //数据区尚未接收完整
            {
                return;
            }

            for (i = 0; i < j; i++)
            {
                str += Convert.ToString(buffer[i], 16) + ' ';
            }
            str += '\r';
            richTextBox1.Text += str;

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
                            u_dsp[n_dsp - 1, i_k] = BitConverter.ToSingle(fbuffer, 0);
                        }
                        pso_data_flag = true;
                    }
                }
                else
                {
                    pso_data_flag = false;
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
                                    richTextBox1.Text = "";
                                    k = (int)data[29].VALUE;
                                    for (i = 0; i <= 15; i++)
                                    {
                                        j = k % 2;
                                        k = k >> 1;
                                        if (j > 0)
                                        {
                                            richTextBox1.Text = "";
                                            richTextBox1.Text = error1[i] + '\r';

                                            //memo1.Lines.Add(error1[i]);
                                        }
                                    }
                                }
                                else
                                {
                                    richTextBox1.Text = "";
                                }
                            }
                        }
                    }
                }

            }
        }



        static void calculate(double[] Vsdq, double Icbd1, double Icbq1, double Icbd2, double Icbq2, double w, ref double Vcbd1, ref double Vcbq1, ref double Vcbd2, ref double Vcbq2)
        {
            //计算在不同端口电压下，PCC点电压的预测值
            Vcbd1 = Vsdq[0] - w * (0.5e-3) * Icbq1 - 0.5 * Icbd1;
            Vcbq1 = Vsdq[1] + w * (0.5e-3) * Icbd1 - 0.5 * Icbq1;
            Vcbd2 = Vsdq[2] - w * (0.5e-3) * Icbq2 - 0.5 * Icbd2;
            Vcbq2 = Vsdq[3] + w * (0.5e-3) * Icbd2 - 0.5 * Icbq2;
        }

        static double fun(double Vcbd1, double Vcbq1, double Vcbd2, double Vcbq2, double[] Vsdq, double U_fu1, double Uo_fu1, double U_fu2, double Uo_fu2)
        {
            double Vs1, Vs2, Vcb1, Vcb2, m1, m2, m3, m4, m5, m6, y;
            //目标函数，计算函数的适应度值
            Vs1 = Math.Sqrt(Vsdq[0] * Vsdq[0] + Vsdq[1] * Vsdq[1]);
            Vs2 = Math.Sqrt(Vsdq[2] * Vsdq[2] + Vsdq[3] * Vsdq[3]);
            Vcb1 = Math.Sqrt(Vcbd1 * Vcbd1 + Vcbq1 * Vcbq1);
            Vcb2 = Math.Sqrt(Vcbd2 * Vcbd2 + Vcbq2 * Vcbq2);
            double VUFout = Vs1 / U_fu1;
            double VUFpcc = Vcb1 / Uo_fu1;
            //if (Vs1 <= 3.11)//1%
            if (Vs1 <= U_fu1 * 0.02)//2%U_fu1*0.02
            //if (Vs1 <= 9.33)//3%
            {
                m1 = 0;
            }

            else
            {
                //m1 = Vs1 - 3.11;//1%
                m1 = Vs1 - U_fu1 * 0.02;//2% U_fu1 * 0.02
                //m1 = Vs1 - 9.33;//3%
            }

            //if (Vs2 <= 3.11)//1%
            if (Vs2 <= U_fu2 * 0.02)//2%U_fu2 * 0.02
            //if (Vs2 <= 9.33)//3%
            {
                m2 = 0;
            }
            else
            {
                //m2 = Vs2 - 3.11;//1%
                m2 = Vs2 - U_fu2 * 0.02;//2% U_fu2 * 0.02
                //m2 = Vs2 - 9.33;//3%
            }

           // m2 = 0;//第二台的权重为0

            m3 = Math.Abs(Vsdq[0] - Vsdq[2]);
            m4 = Math.Abs(Vsdq[1] - Vsdq[3]);

            //if (Vcb1 - 2.95 <= 0)//1%
            if (Vcb1 - Uo_fu1 * 0.02 <= 0)//2%Uo_fu1*0.01
            //if (Vcb1 - 8.85 <= 0)//3%
            {
                m5 = 0;
            }
            else
            {
                //m5 = Vcb1 - 2.95;//1%
                m5 = Vcb1 - Uo_fu1 * 0.02;//2%Uo_fu1 * 0.01
                //m5 = Vcb1 - 8.85;//3%
            }

            //if (Vcb2 - 2.95 <= 0)//1%
            if (Vcb2 - Uo_fu2 * 0.02 <= 0)//2%Uo_fu2 * 0.01
            //if (Vcb2 - 8.85 <= 0)//3%
            {
                m6 = 0;
            }
            else
            {
                //m6 = Vcb2 - 2.95;//1%
                m6 = Vcb2 - Uo_fu2 * 0.02;//2%Uo_fu2 * 0.01
                //m6 = Vcb2 - 8.85;//3%
            }

            //m6 = 0;//第二台的权重为0
            y = 5 * m1 + 5 * m2 + 50 * m3 + 50 * m4 + 5 * m5 + 5 * m6;    //考虑环流
            //y = 5 * m1 + 5 * m2 + 5 * m5 + 5 * m6;                  // 不考虑环流
            return y;
        }




        private void button3_Click(object sender, EventArgs e)
        {
            if(CommonRes.serialPort1.IsOpen)
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

            pso_init = false;

            int crc;
            if (!CommonRes.serialPort1.IsOpen)
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
            }
            Num_time = 0;
            Num_time_first = 0;
            sendbf[8] = 0;
            crc = sendbf[4] + sendbf[5] + sendbf[6] + sendbf[7] + sendbf[8];
            crc = ~crc + 1;
            sendbf[9] =(byte)crc;
            CommonRes.serialPort1.Write(sendbf, 0, 10);
            
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    richTextBox1.Text += Convert.ToString(sendbf[i], 16);
                    richTextBox1.Text += ' ';
                }
                catch
                {
                    MessageBox.Show("停机冲突");
                }
            }
            richTextBox1.Text += '\r';
            Array.Clear(sendbf, 0, 19);

            if(brun)
            {
                timer3.Enabled = true;
            }
            else
            {
                timer3.Enabled = false;
            }
            //richTextBox1.Text += '\n';
            //timer2.Enabled = true;
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
                CommonRes.serialPort1.Write(sendbf, 0, sendbf[4] + 7);
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
            if (CommonRes.serialPort1.IsOpen)
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
                    CommonRes.serialPort1.Write(sendbf, 0, sendbf[4] + 5);
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
            if(!CommonRes.serialPort1.IsOpen)
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


                        CommonRes.serialPort1.Write(sendbf, 0, sendbf[4] + 5);
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
            if (!CommonRes.serialPort1.IsOpen)
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

                        CommonRes.serialPort1.Write(sendbf, 0, sendbf[4] + 5);



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

            if (CommonRes.serialPort1.IsOpen)
            {
                if (Num_time_first == 0)
                {
                    Num_time++;
                    if(Num_time==2)//初始间隔
                    {
                        Num_time = 0;
                        Num_time_first = 1;
                        Array.Clear(sendbf, 0, 19);
                        int crc = 0;
                        Num_DSP = 1;
                        sendbf[0] = 0xfe;
                        sendbf[1] = 0xfe;
                        sendbf[2] = 0xfe;
                        sendbf[3] = 0xfe;
                        sendbf[4] = 4; crc = sendbf[4];
                        sendbf[5] = 200; crc = crc + sendbf[5];
                        sendbf[6] = 0xff; crc = crc + sendbf[6];
                        sendbf[7] = (byte)Num_DSP; crc = crc + sendbf[7];
                        sendbf[8] = (byte)(0 - crc);
                        CommonRes.serialPort1.Write(sendbf, 0, sendbf[4] + 5);
                    }
                }
                if (Num_time_first == 1)
                { 
                    Num_time++;
                    if (Num_time == 2)//起始第一段数据发送的时间
                    {
                        //Num_time = 0;
                        int crc = 0;
                        Array.Clear(sendbf, 0, 19);
                        /*
                        Num_DSP++;
                        if (Num_DSP > 2)
                        {
                            Num_DSP = 1;
                        }
                        */
                        Num_DSP = 2;
                        sendbf[0] = 0xfe;
                        sendbf[1] = 0xfe;
                        sendbf[2] = 0xfe;
                        sendbf[3] = 0xfe;
                        sendbf[4] = 4; crc = sendbf[4];
                        sendbf[5] = 200; crc = crc + sendbf[5];
                        sendbf[6] = 0xff; crc = crc + sendbf[6];
                        sendbf[7] = (byte)Num_DSP; crc = crc + sendbf[7];
                        sendbf[8] = (byte)(0 - crc);
                        CommonRes.serialPort1.Write(sendbf, 0, sendbf[4] + 5);
                    }
                    else if (Num_time == 4)//整段时间间隔
                    {
                        Num_time = 0;
                        int crc = 0;
                        Array.Clear(sendbf, 0, 19);
                        Num_DSP = 1;
                        sendbf[0] = 0xfe;
                        sendbf[1] = 0xfe;
                        sendbf[2] = 0xfe;
                        sendbf[3] = 0xfe;
                        sendbf[4] = 4; crc = sendbf[4];
                        sendbf[5] = 200; crc = crc + sendbf[5];
                        sendbf[6] = 0xff; crc = crc + sendbf[6];
                        sendbf[7] = (byte)Num_DSP; crc = crc + sendbf[7];
                        sendbf[8] = (byte)(0 - crc);
                        CommonRes.serialPort1.Write(sendbf, 0, sendbf[4] + 5);
                    }
                }
            }
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            if (pso_data_flag == true)
            {
                pso_data_flag = false;

                //timer3.Enabled = false;

                if (n_dsp == Set_Num_DSP)
                {
                    //接收的数据存入u_dsp中
                    //两台逆变器dq坐标系下的负序电流输入以及频率
                    //double Icbd1 = u_dsp[0, 0];
                    //double Icbq1 = u_dsp[0, 1];
                    //double Icbd2 = u_dsp[1, 0];
                    //double Icbq2 = u_dsp[1, 1];
                    double w = u_dsp[0, 2];

                    Icbd1 = u_dsp[0, 0];
                    Icbq1 = u_dsp[0, 1];
                    Icbd2 = u_dsp[1, 0];
                    Icbq2 = u_dsp[1, 1];
                    Udp1 = u_dsp[0, 3];
                    Uqp1 = u_dsp[0, 4];
                    Udp2 = u_dsp[1, 3];
                    Uqp2 = u_dsp[1, 4];
                    Uodp1 = u_dsp[0, 5];
                    Uoqp1 = u_dsp[0, 6];
                    Uodp2 = u_dsp[1, 5];
                    Uoqp2 = u_dsp[1, 6];

                    //Icbd1 = 0.80 * Icbd1+ 0.20 * u_dsp[0, 0];
                    //Icbq1 = 0.80 * Icbq1+ 0.20 * u_dsp[0, 1];
                    //Icbd2 = 0.80 * Icbd2+ 0.20 * u_dsp[1, 0];
                    //Icbq2 = 0.80 * Icbq2+ 0.20 * u_dsp[1, 1];
                    //Udp1 = 0.80 * Udp1 + 0.20 * u_dsp[0, 3];
                    //Uqp1 = 0.80 * Uqp1 + 0.20 * u_dsp[0, 4];
                    //Udp2 = 0.80 * Udp2 + 0.20 * u_dsp[1, 3];
                    //Uqp2 = 0.80 * Uqp2 + 0.20 * u_dsp[1, 4];
                    //Uodp1 = 0.80 * Uodp1 + 0.20 * u_dsp[0, 5];
                    //Uoqp1 = 0.80 * Uoqp1 + 0.20 * u_dsp[0, 6];
                    //Uodp2 = 0.80 * Uodp2 + 0.20 * u_dsp[1, 5];
                    //Uoqp2 = 0.80 * Uoqp2 + 0.20 * u_dsp[1, 6];

                    double U_fu1 = 0;
                    double Uo_fu1 = 0;
                    double U_fu2 = 0;
                    double Uo_fu2 = 0;
                    U_fu1 = Math.Sqrt(Udp1 * Udp1 + Uqp1 * Uqp1);
                    Uo_fu1 = Math.Sqrt(Uodp1 * Uodp1 + Uoqp1 * Uoqp1);
                    U_fu2 = Math.Sqrt(Udp2 * Udp2 + Uqp2 * Uqp2);
                    Uo_fu2 = Math.Sqrt(Uodp2 * Uodp2 + Uoqp2 * Uoqp2);



                    double Vd1 = 0;
                    double Vq1 = 0;
                    double Vd2 = 0;
                    double Vq2 = 0;
                    if (!pso_init)
                    {
                        for (int i_p = 0; i_p < sizepop; i_p++)
                        {
                            double[] find_pop = new double[4];
                            for (int j_p = 0; j_p < 4; j_p++)
                            {

                                //pop(i,:) = 100 * rands(1, 4);   % 第i个粒子的初始位置；矩阵的第i行是两个2乘0~1之间的随机数 pop(i,:)= 2 * rands(1, 2);
                                //V(i,:) = 2 * rands(1, 4);   % 第i个粒子的速度

                                pop[i_p, j_p] = 100 * (ran.NextDouble() * 2 - 1);//第i个粒子的初始位置；范围是0~1
                                                                                 //100rands（1,4）表示输出一个大小为1行4列的随机数，4个随机数分别代表4个自变量，即两台逆变器dq坐标下的负序电压参考值
                                V[i_p, j_p] = 2 * (ran.NextDouble() * 2 - 1); //第i个粒子的速度

                                //功能说明：限制粒子速度和粒子的位置（解）不超过最大允许值
                                //V[i, find(V(i, j) > Vmax)] = Vmax;
                                //V[i, find(V(i, j) < Vmin)] = Vmin;
                                //pop[i, find(pop(i, j) > popmax)] = popmax;
                                //pop[i, find(pop(i, j) < popmin)] = popmin;

                                if (pop[i_p, j_p] > popmax) pop[i_p, j_p] = popmax;
                                if (pop[i_p, j_p] < popmin) pop[i_p, j_p] = popmin;
                                if (V[i_p, j_p] > Vmax) V[i_p, j_p] = Vmax;
                                if (V[i_p, j_p] < Vmin) V[i_p, j_p] = Vmin;

                                find_pop[j_p] = pop[i_p, j_p];
                            }
                            //功能说明：基于第一次初始化的负序电压参考值，计算对应PCC点的负序电压值
                            double Vcbd1 = 0;
                            double Vcbq1 = 0;
                            double Vcbd2 = 0;
                            double Vcbq2 = 0;
                            //[Vcbd1, Vcbq1, Vcbd2, Vcbq2] = calculate(pop[i,4], Icbd1, Icbq1, Icbd2, Icbq2, w);
                            calculate(find_pop, Icbd1, Icbq1, Icbd2, Icbq2, w, ref Vcbd1, ref Vcbq1, ref Vcbd2, ref Vcbq2);
                            // 功能说明：计算第i个粒子的适应度值，评价此次负序电压参考值下，不平衡电压补偿的效果（端口电压和PCC点电压是否满足限制条件）
                            //fitness(j)=fun(Vcbd1,Vcbq1,Vcbd2,Vcbq2,pop(j,:));
                            fitness[i_p] = fun(Vcbd1, Vcbq1, Vcbd2, Vcbq2, find_pop, U_fu1, Uo_fu1, U_fu2, Uo_fu2);
                        }

                        double bestfitness;
                        int bestindex = 0;
                        //计算结束后，比较个体极值和全局极值
                        //寻找初始极值
                        //[bestfitness, bestindex]=min(fitness); //20个个体中找到最小值，以及对应的位置
                        bestfitness = fitness.Min();
                        for (int i_p = 0; i_p < sizepop; i_p++)
                        {
                            if (fitness[i_p] == bestfitness)
                            {
                                bestindex = i_p;
                            }
                        }
                        //zbest = pop(bestindex,:);         //群体极值位置 全局最优，一个点的位置，是个1行4列的矩阵
                        for (int j_p = 0; j_p < 4; j_p++)
                        {
                            zbest[j_p] = pop[bestindex, j_p];
                        }
                        //gbest = pop;                     //个体极值位置 个体最优，20个点的位置，假设初始化的点均为个体极值点，gbest是个矩阵，是个20行4列的矩阵
                        for (int i_p = 0; i_p < sizepop; i_p++)
                        {
                            for (int j_p = 0; j_p < 4; j_p++)
                            {
                                gbest[i_p, j_p] = pop[i_p, j_p];
                                //gbest[i_p][j_p]= pop[i_p][j_p];
                            }
                        }



                        //fitnessgbest = fitness;          //个体极值适应度值 fitness是个数组，包含20个个体极值
                        //fitness.CopyTo(fitnessgbest, 0);
                        for (int i_p = 0; i_p < sizepop; i_p++)
                        {
                            fitnessgbest[i_p] = fitness[i_p];
                        }

                        //fitnesszbest = bestfitness;      //群体极值适应度值 全局的最小值，只有1个数
                        fitnesszbest = bestfitness;

                        pso_init = true;//初始化完成

                    }
                    else
                    {
                        //粒子每0.05s迭代100次然后输出结果
                        for (int n_k = 0; n_k < maxgen; n_k++)
                        {
                            //20个粒子位置和速度更新，负序电压参考指令的改变
                            for (int i_p = 0; i_p < sizepop; i_p++)
                            {
                                double[] find_pop = new double[4];
                                for (int j_p = 0; j_p < 4; j_p++)
                                {
                                    //%%%%%%%%%%%%%% 速度更新
                                    //V(j,:) = V(j,:) + c1 * rand * (gbest(j,:) - pop(j,:)) + c2 * rand * (zbest - pop(j,:));   %
                                    //V(j, find(V(j,:) > Vmax)) = Vmax;            %%% 限幅 %%%%
                                    //V(j, find(V(j,:) < Vmin)) = Vmin;            %%% 限幅 %%%%        

                                    V[i_p, j_p] = V[i_p, j_p] + c1 * ran.NextDouble() * (gbest[i_p, j_p] - pop[i_p, j_p]) + c2 * ran.NextDouble() * (zbest[j_p] - pop[i_p, j_p]);
                                    if (V[i_p, j_p] > Vmax) V[i_p, j_p] = Vmax;
                                    if (V[i_p, j_p] < Vmin) V[i_p, j_p] = Vmin;

                                    //%%%%%%%%%%%%%%% 粒子更新
                                    //pop(j,:) = pop(j,:) + V(j,:);
                                    //pop(j, find(pop(j,:) > popmax)) = popmax;              %%% 限幅 %%%%
                                    //pop(j, find(pop(j,:) < popmin)) = popmin;              %%% 限幅 %%%%

                                    pop[i_p, j_p] = pop[i_p, j_p] + V[i_p, j_p];
                                    if (pop[i_p, j_p] > popmax) pop[i_p, j_p] = popmax;
                                    if (pop[i_p, j_p] < popmin) pop[i_p, j_p] = popmin;

                                    find_pop[j_p] = pop[i_p, j_p];

                                }

                                //新粒子适应度值    过程与初始化过程一致
                                double Vcbd1 = 0;
                                double Vcbq1 = 0;
                                double Vcbd2 = 0;
                                double Vcbq2 = 0;
                                //[Vcbd1, Vcbq1, Vcbd2, Vcbq2] = calculate(pop[i,4], Icbd1, Icbq1, Icbd2, Icbq2, w);
                                calculate(find_pop, Icbd1, Icbq1, Icbd2, Icbq2, w, ref Vcbd1, ref Vcbq1, ref Vcbd2, ref Vcbq2);
                                // 功能说明：计算第i个粒子的适应度值，评价此次负序电压参考值下，不平衡电压补偿的效果（端口电压和PCC点电压是否满足限制条件）
                                //fitness(j)=fun(Vcbd1,Vcbq1,Vcbd2,Vcbq2,pop(j,:));
                                fitness[i_p] = fun(Vcbd1, Vcbq1, Vcbd2, Vcbq2, find_pop, U_fu1, Uo_fu1, U_fu2, Uo_fu2);
                            }


                            //比较大小，个体极值和群体极值更新,取较小值
                            for (int i_p = 0; i_p < sizepop; i_p++)
                            {
                                //个体极值更新
                                //if fitness(j) < fitnessgbest(j) % 第j个粒子与它的历史最优极值进行比较
                                //gbest(j,:) = pop(j,:);                   % 历史最优个体极值的位置变成当前粒子的位置
                                //fitnessgbest(j) = fitness(j);             % 历史最优极值变成当前粒子极值
                                //end

                                if (fitness[i_p] < fitnessgbest[i_p])// 第i个粒子与它的历史最优极值进行比较
                                {
                                    //gbest(j,:) = pop(j,:);                   //历史最优个体极值的位置变成当前粒子的位置
                                    for (int j_p = 0; j_p < 4; j_p++)
                                    {
                                        gbest[i_p, j_p] = pop[i_p, j_p];
                                    }
                                    fitnessgbest[i_p] = fitness[i_p];             //历史最优极值变成当前粒子极值

                                }

                                //群体极值更新
                                //if fitness(j) < fitnesszbest % 第j个粒子与群体的历史最优值进行比较
                                //zbest = pop(j,:);                  % 群体的历史最优值的位置变成当前粒子的位置
                                //fitnesszbest = fitness(j);            % 历史最优值变成当前粒子的极值
                                //end

                                if (fitness[i_p] < fitnesszbest) //第j个粒子与群体的历史最优值进行比较
                                {
                                    //zbest = pop(j,:);                  //群体的历史最优值的位置变成当前粒子的位置
                                    for (int j_p = 0; j_p < 4; j_p++)
                                    {
                                        zbest[j_p] = pop[i_p, j_p];
                                    }
                                    fitnesszbest = fitness[i_p];           //历史最优值变成当前粒子的极值
                                    hbest[0] = w;
                                    hbest[1] = Icbd1;
                                    hbest[2] = Icbq1;
                                    hbest[3] = Icbd2;
                                    hbest[4] = Icbq2;
                                    hbest[5] = Udp1;
                                    hbest[6] = Uqp1;
                                    hbest[7] = Udp2;
                                    hbest[8] = Uqp2;
                                    hbest[9] = Uodp1;
                                    hbest[10] = Uoqp1;
                                    hbest[11] = Uodp2;
                                    hbest[12] = Uoqp2;
                                }
                            }

                            // 输出结果这句话，逻辑上放的位置稍微有点问题
                            // 输出最终结果，适应度值，负序电压参考指令（4个）   
                            //x(300) = fitnesszbest;
                            //x(301) = fitnesszbest;
                            //x(302) = zbest(1);
                            //x(303) = zbest(2);
                            //x(304) = zbest(3);
                            //x(305) = zbest(4);
                            //显示计算得到的PCC点预测的电压
                            //Vd1 = zbest(1) - w * (0.5e-3) * Icbq1 - 0.5 * Icbd1;
                            //Vq1 = zbest(2) + w * (0.5e-3) * Icbd1 - 0.5 * Icbq1;
                            //Vd2 = zbest(3) - w * (0.5e-3) * Icbq2 - 0.5 * Icbd2;
                            //Vq2 = zbest(4) + w * (0.5e-3) * Icbd2 - 0.5 * Icbq2;
                            //x(306) = Vd1;
                            //x(307) = Vq1;
                            //x(308) = Vd2;
                            //x(309) = Vq2;

                            //Vd1 = zbest[0] - w * (0.5e-3) * Icbq1 - 0.5 * Icbd1;
                            //Vq1 = zbest[1] + w * (0.5e-3) * Icbd1 - 0.5 * Icbq1;
                            //Vd2 = zbest[2] - w * (0.5e-3) * Icbq2 - 0.5 * Icbd2;
                            //Vq2 = zbest[3] + w * (0.5e-3) * Icbd2 - 0.5 * Icbq2;

                            //Vd1 = zbest[0] - w * (0.5e-3) * Icbq1 - 0.5 * Icbd1;
                            //Vq1 = zbest[1] + w * (0.5e-3) * Icbd1 - 0.5 * Icbq1;
                            //Vd2 = zbest[2] - w * (0.5e-3) * Icbq2 - 0.5 * Icbd2;
                            //Vq2 = zbest[3] + w * (0.5e-3) * Icbd2 - 0.5 * Icbq2;
                            Vd1 = zbest[0];
                            Vq1 = zbest[1];
                            Vd2 = zbest[2];
                            Vq2 = zbest[3];

                            //   if (fitnesszbest <= 0.5)
                            //       break;

                        }


                    }







                    //将计算完成的数据存入发送数组中
                    //回发数据为了保持两台机子同步接收，不区分逆变器，数据同时下发，逆变器侧自行根据位置进行接收

                    sendbf[0] = 0xfe;
                    sendbf[1] = 0xfe;
                    sendbf[2] = 0xfe;
                    sendbf[3] = 0xfe;
                    sendbf[4] = 19;//4个浮点数，16个字节，此外还有序列号+命令码+校验码，共19字节
                    sendbf[5] = 200;
                    sendbf[6] = 0xff;


                    //Vd1 = 0.5;
                    //Vd2 = 0.7;
                    //Vq1 = 0.8;
                    //Vq2 = 0.9;

                    u_g[0] = Convert.ToSingle(Vd1);
                    u_g[1] = Convert.ToSingle(Vq1);
                    u_g[2] = Convert.ToSingle(Vd2);
                    u_g[3] = Convert.ToSingle(Vq2);

                    for (int i_n = 0; i_n < 4; i_n++)
                    {
                        byte[] temp_i = BitConverter.GetBytes(u_g[i_n]);
                        sendbf[7 + 4 * i_n] = temp_i[0];
                        sendbf[8 + 4 * i_n] = temp_i[1];
                        sendbf[9 + 4 * i_n] = temp_i[2];
                        sendbf[10 + 4 * i_n] = temp_i[3];
                    }
                    sendbf[23] = 0xff;

                    CommonRes.serialPort1.Write(sendbf, 0, sendbf[4] + 5);//加5是还有包头4字节，以及长度19字节
                }

                //timer3.Enabled = true;
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
    }
}
