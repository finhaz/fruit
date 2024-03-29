﻿using System;
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
using System.Collections.Concurrent;

namespace fruit
{

    public partial class Form1 : Form
    {
        SerialPort serialPort1=new SerialPort();
        bool brun = false;
        bool bshow =false;
        bool bmodify = false;
        bool Initialized = true;//调试参数与修正系数核验标志位
        bool flag_uncon = false;//上下参数不一致标志位
        bool flag_get_runvalue=false;//读取运行数据标志位

        public bool flag_under_first = false;
        public bool flag_upper_first = false;

        bool sopen=false;
        float old_value;
        float new_value;
        public int sn=0; 
        int mrow;       
        int Num_time = 0;
        int Num_DSP=0;
        int Protocol_num=0;
        static int Set_Num_DSP = 2;//代表有机子数

        
        Form3 f3 = new Form3();
        private TestBind model = new TestBind();
        PSO_analysis PSO_v = new PSO_analysis();
        //IPSO_analysis IPSO_v = new IPSO_analysis();
        //Fish_Solution Fish_v = new Fish_Solution();
        Message NYS_com= new Message();
        Message_modbus FCOM2= new Message_modbus();
        DataBase_Interface DB_Com = new DataBase_Interface();

        //针对数据协议：
        byte[] gbuffer = new byte[4096];
        int gb_index = 0;//缓冲区注入位置
        int get_index = 0;// 缓冲区捕捉位置

        /// <summary>
        /// 保持读取开关
        /// </summary>
        bool _keepReading=false;

        /// <summary>
        /// 检测频率【检测等待时间，毫秒】【按行读取，可以不用】
        /// </summary>
        int _jcpl = 1;

        /// <summary>
        /// 字符串队列【.NET Framework 4.0以上】
        /// </summary>
        ConcurrentQueue<string> _cq = new ConcurrentQueue<string>();

        /// <summary>
        /// 字节数据队列
        /// </summary>
        ConcurrentQueue<byte[]> _cqBZ = new ConcurrentQueue<byte[]>();

        public Form1()
        {
            InitializeComponent();
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            string[] item = { "FE协议", "Modbus协议" };
            foreach (string a in item)
            {
                comboBox1.Items.Add(a);
            }
            comboBox1.SelectedItem = comboBox1.Items[1];    //默认为列表第二个变量  
            Protocol_num = 1;

            model.NoticeHandler += new PropertyNoticeHandler(PropertyNoticeHandler);
            Binding bind2 = new Binding("Text", model, "Name");
            richTextBox1.DataBindings.Add(bind2);

            serialPort1.DataReceived += new SerialDataReceivedEventHandler(this.SerialPort1_DataReceived);

            //基本存储、参数设置初始化
            try
            {
                DB_Com.DataBase_PARAMETER_RUN_Init();


                for (int i = DB_Com.u; i <= DB_Com.j; i++)
                {
                    int index = this.dataGridView1.Rows.Add();
                    this.dataGridView1.Rows[index].Cells[0].Value = DB_Com.data[i].SN;
                    this.dataGridView1.Rows[index].Cells[1].Value = DB_Com.data[i].NAME;
                    this.dataGridView1.Rows[index].Cells[2].Value = DB_Com.data[i].VALUE;
                    if (DB_Com.data[i].FACTOR > 0.001)
                    {
                        //dataGridView1.Rows[index].Cells[3].Value = format('%.2f',[datas[j].FACTOR]) + data[j].UNITor;      //名称
                        dataGridView1.Rows[index].Cells[3].Value = Math.Round(DB_Com.data[i].FACTOR, 2) + DB_Com.data[i].UNITor;
                    }
                    else
                    {
                        dataGridView1.Rows[index].Cells[3].Value = DB_Com.data[i].UNITor;
                    }
                    DB_Com.runnum++;
                }

                DB_Com.DataBase_PARAMETER_SET_Init();

                for (int i = DB_Com.u; i <=DB_Com.j; i++)
                {
                    if (DB_Com.data[i].SN != 0)
                    {
                        int index = this.dataGridView2.Rows.Add();
                        this.dataGridView2.Rows[index].Cells[0].Value = DB_Com.data[i].SN;
                        this.dataGridView2.Rows[index].Cells[1].Value = DB_Com.data[i].NAME;
                        this.dataGridView2.Rows[index].Cells[2].Value = DB_Com.data[i].VALUE;
                        if (DB_Com.data[i].FACTOR > 0.001)
                        {

                            //dataGridView2.Rows[index].Cells[3].Value = format('%.2f',[datas[j].FACTOR]) + data[j].UNITor;      //名称
                            dataGridView2.Rows[index].Cells[3].Value = Math.Round(DB_Com.data[i].FACTOR, 2) + DB_Com.data[i].UNITor;
                        }
                        else
                        {
                            dataGridView2.Rows[index].Cells[3].Value = DB_Com.data[i].UNITor;
                        }
                    }
                }


                DB_Com.DataBase_PARAMETER_FACTOR_Init();

                for (int i = DB_Com.u; i < DB_Com.j; i++)
                {
                    int index = this.dataGridView3.Rows.Add();
                    this.dataGridView3.Rows[index].Cells[0].Value = DB_Com.data[i].SN;
                    this.dataGridView3.Rows[index].Cells[1].Value = DB_Com.data[i].NAME;
                    this.dataGridView3.Rows[index].Cells[2].Value = DB_Com.data[i].VALUE;
                }


                DB_Com.DataBase_ERROR_Table_Init();

            }
            catch
            {
                MessageBox.Show("缺少MOON.mdb");
            }

            //数据存储初始化
            try
            {
                DB_Com.DataBase_PRUN_create();

                DB_Com.DataBase_UG_create();
            }
            catch
            {
                MessageBox.Show("缺少fruit.mdb");
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

        private void SerialPort1_DataReceived1(object sender, SerialDataReceivedEventArgs e)
        {
            throw new NotImplementedException();
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
                    serialPort1.Open();
                    button2.Text = "关闭串口";
                    button3.Enabled = true;
                    button1.Enabled = false;
                    sopen = true;
                    _keepReading = true;



                }
                catch
                {
                    MessageBox.Show("请插入串口设备！！");
                    sopen = false;
                }
                
            }
            else
            {
                serialPort1.Close();
                button2.Text = "打开串口";
                button3.Enabled = false;
                button1.Enabled = true;
                sopen = false;

                _keepReading = false;

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
            if (Protocol_num == 0)
            {
                Thread.Sleep(80);//照顾当前粒子群的非环形缓冲读取法
            }
            //该事件函数在新的线程执行
            //没使用数据绑定前，此代码不可注释
            Control.CheckForIllegalCrossThreadCalls = false;
            //throw new NotImplementedException();
            
            byte[] buffer = new byte[200];
            int i = 0;
            int buffer_len = 0;
            
            string str = "";
            int n_dsp = 0;
            int check_result=0;
            int gb_last=gb_index;//记录上次的位置


            try
            {
                if (Protocol_num == 0)
                {
                    buffer_len = serialPort1.Read(gbuffer, 0, gbuffer.Length);
                }
                else if (Protocol_num == 1)
                {
                    buffer_len = serialPort1.Read(gbuffer, gb_index, (gbuffer.Length - gb_index));
                    gb_index = gb_index + buffer_len;
                    if (gb_index >= gbuffer.Length)
                        gb_index = gb_index - gbuffer.Length;
                }
            }
            catch
            {
                return;
            }

            
            for (i = 0; i < buffer_len; i++)
            {
                str += Convert.ToString(gbuffer[(gb_last + i) % gbuffer.Length], 16) + ' ';
            }
            str += '\r';
            //richTextBox1.Text += str;
            model.Name += str;


            if (Protocol_num == 0)
            {
                if (buffer_len < gbuffer[4] + 5) //数据区尚未接收完整
                {
                    return;
                }

                check_result = NYS_com.monitor_check(gbuffer);

                if (check_result == 1)
                {
                    n_dsp = gbuffer[7];
                    for (int i_k = 0; i_k < 9; i_k++)//字节重新拼接为浮点数
                    {
                        PSO_v.u_dsp[n_dsp - 1, i_k] = BitConverter.ToSingle(gbuffer, 8 + 4 * i_k);

                        //IPSO_v.u_dsp[n_dsp - 1, i_k] = BitConverter.ToSingle(gbuffer, 8 + 4 * i_k);

                        //Fish_v.u_dsp[n_dsp - 1, i_k] = BitConverter.ToSingle(gbuffer, 8 + 4 * i_k);


                    }
                    if (n_dsp == Set_Num_DSP)
                    {
                        
                        Thread th = new Thread(new ThreadStart(PSO_v.cale_pso)); //创建PSO线程
                        th.Start(); //启动线程                          
                        Thread th1 = new Thread(new ThreadStart(update_UI_PSO)); //创建UI线程
                        th1.Start(); //启动线程
                        

                        /*
                        Thread th = new Thread(new ThreadStart(IPSO_v.cale_pso)); //创建IPSO线程
                        th.Start(); //启动线程                          
                        Thread th1 = new Thread(new ThreadStart(update_UI_IPSO)); //创建UI线程
                        th1.Start(); //启动线程
                        */


                        /*
                        Thread th = new Thread(new ThreadStart(Fish_v.cale_fish)); //创建FISH线程
                        th.Start(); //启动线程                          
                        Thread th1 = new Thread(new ThreadStart(update_UI_FishAI)); //创建UI线程
                        th1.Start(); //启动线程
                        */

                    }
                }
                else if (check_result == 2)
                {
                    float temp_val;
                    temp_val = BitConverter.ToSingle(gbuffer, 8);

                    if (DB_Com.data[gbuffer[5]].SN == gbuffer[5])
                    {
                        if (gbuffer[5] < 44)
                        {
                            DB_Com.data[gbuffer[5]].VALUE = temp_val;
                            dataGridView1.Rows[DB_Com.data[gbuffer[5]].SN].Cells[2].Value = DB_Com.data[gbuffer[5]].VALUE;
                        }
                        else
                        {
                            float ovla;
                            int vindex;
                            if (gbuffer[5] < 90)
                            {
                                vindex = DB_Com.data[gbuffer[5]].SN - 44;
                                ovla = Convert.ToSingle(dataGridView2.Rows[vindex].Cells[2].Value);
                            }
                            else
                            {
                                vindex = DB_Com.data[gbuffer[5]].SN - 90;
                                ovla = Convert.ToSingle(dataGridView3.Rows[vindex].Cells[2].Value);
                            }
                            if (temp_val != ovla)
                            {
                                if (flag_under_first == false)
                                {
                                    flag_uncon = true;//说明出现上下参数不一致
                                }
                                else
                                {
                                    if (gbuffer[5] < 90)
                                    {
                                        dataGridView2.Rows[vindex].Cells[2].Value = temp_val;
                                        DB_Com.DataBase_SET_Save("PARAMETER_SET", temp_val, (byte)gbuffer[5]);
                                    }
                                    else
                                    {
                                        dataGridView3.Rows[vindex].Cells[2].Value = temp_val;
                                        DB_Com.DataBase_SET_Save("PARAMETER_FACTOR", temp_val, (byte)gbuffer[5]);
                                    }
                                }
                            }
                        }
                    }
                    DB_Com.data[gbuffer[5]].ACK = gbuffer[7];

                }

            }



            else if (Protocol_num == 1)
            {

                for (i = get_index; i < gbuffer.Length; i++)
                {
                    if (gbuffer[i] == 0x01)
                    {
                        int temp = (i + 1) % gbuffer.Length;
                        if (gbuffer[temp] == 0x03)
                        {
                            buffer[0] = 0x01;
                            buffer[1] = gbuffer[temp];
                            int j;
                            for (j = 0; j < gbuffer[(temp + 1) % gbuffer.Length] + 3; j++)
                            {
                                buffer[j + 2] = gbuffer[(temp + j + 1) % gbuffer.Length];
                            }
                        }
                        else if (gbuffer[temp] == 0x06 || gbuffer[temp] == 0x10)
                        {
                            buffer[0] = 0x01;
                            buffer[1] = gbuffer[temp];
                            int j;
                            for (j = 0; j < 6; j++)
                            {
                                buffer[j + 2] = gbuffer[(temp + j + 1) % gbuffer.Length];
                            }
                        }
                        check_result = FCOM2.monitor_check(buffer, buffer.Length);
                        break;
                    }
                }




                if (check_result == 1)
                {

                    switch (buffer[1])
                    {
                        case 0x03:
                            Int16 temp_val;
                            int snrun;
                            byte[] temp_i = new byte[2];
                            get_index = (get_index + buffer[2] + 5) % gbuffer.Length;
                            if (sn == 0)
                            {

                                for (i = 3; i < buffer[2] + 3; i = i + 2)
                                {
                                    temp_i[1] = buffer[i];
                                    temp_i[0] = buffer[i + 1];
                                    temp_val = BitConverter.ToInt16(temp_i, 0);
                                    snrun = (i - 3) / 2;
                                    DB_Com.data[snrun].VALUE = temp_val;
                                    dataGridView1.Rows[snrun].Cells[2].Value = DB_Com.data[snrun].VALUE;
                                }
                            }
                            else if (sn == 44)
                            {
                                for (i = 3; i < buffer[2] + 3; i = i + 2)
                                {
                                    temp_i[1] = buffer[i];
                                    temp_i[0] = buffer[i + 1];
                                    temp_val = BitConverter.ToInt16(temp_i, 0);
                                    snrun = sn + (i - 3) / 2;
                                    if (DB_Com.data[snrun].VALUE != temp_val)
                                    {
                                        MessageBox.Show("参数不一致");
                                        break;
                                    }
                                }
                            }
                            break;
                        case 0x06:
                            get_index = (get_index + 8) % gbuffer.Length;
                            break;
                        case 0x10:
                            get_index = (get_index + 8) % gbuffer.Length;
                            break;
                        default:
                            break;
                    }

                }
                else
                {
                    return;
                }
            }
        }

        private void update_UI_PSO()
        {
            Control.CheckForIllegalCrossThreadCalls = false;//没使用数据绑定前，此代码不可注释
            try
            {
                NYS_com.PSO_send(PSO_v.u_g);
                serialPort1.Write(NYS_com.sendbf, 0, NYS_com.sendbf[4] + 5);

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

        /*
        private void update_UI_IPSO()
        {
            Control.CheckForIllegalCrossThreadCalls = false;//没使用数据绑定前，此代码不可注释
            try
            {
                NYS_com.PSO_send(IPSO_v.u_g);
                serialPort1.Write(NYS_com.sendbf, 0, NYS_com.sendbf[4] + 5);

                DB_Com.DataBase_UG_Save(IPSO_v.u_g);

                f3.Text1 = IPSO_v.Icbd1.ToString();
                f3.Text2 = IPSO_v.Icbq1.ToString();
                f3.Text3 = IPSO_v.Udp1.ToString();
                f3.Text4 = IPSO_v.Uqp1.ToString();
                f3.Text5 = IPSO_v.Uodp1.ToString();
                f3.Text6 = IPSO_v.Uoqp1.ToString();
                f3.Text7 = IPSO_v.w.ToString();

                f3.Text8 = IPSO_v.Icbd2.ToString();
                f3.Text9 = IPSO_v.Icbq2.ToString();
                f3.Text10 = IPSO_v.Udp2.ToString();
                f3.Text11 = IPSO_v.Uqp2.ToString();
                f3.Text12 = IPSO_v.Uodp2.ToString();
                f3.Text13 = IPSO_v.Uoqp2.ToString();
                f3.Text14 = IPSO_v.w.ToString();

                f3.Text15 = IPSO_v.Vd_VCM.ToString();
                f3.Text16 = IPSO_v.Vq_VCM.ToString();
                f3.Text17 = IPSO_v.Id_CCM.ToString();
                f3.Text18 = IPSO_v.Iq_CCM.ToString();
            }
            catch
            {
                MessageBox.Show("只是停快了，小场面！！！");
            }

        }
        */

        /*
        private void update_UI_FishAI()
        {
            Control.CheckForIllegalCrossThreadCalls = false;//没使用数据绑定前，此代码不可注释
            try
            {
                NYS_com.PSO_send(Fish_v.u_g);
                serialPort1.Write(NYS_com.sendbf, 0, NYS_com.sendbf[4] + 5);

                f3.Text1 = Fish_v.Icbd1.ToString();
                f3.Text2 = Fish_v.Icbq1.ToString();
                f3.Text3 = Fish_v.Udp1.ToString();
                f3.Text4 = Fish_v.Uqp1.ToString();
                f3.Text5 = Fish_v.Uodp1.ToString();
                f3.Text6 = Fish_v.Uoqp1.ToString();
                f3.Text7 = Fish_v.w.ToString();

                f3.Text8 = Fish_v.Icbd2.ToString();
                f3.Text9 = Fish_v.Icbq2.ToString();
                f3.Text10 = Fish_v.Udp2.ToString();
                f3.Text11 = Fish_v.Uqp2.ToString();
                f3.Text12 = Fish_v.Uodp2.ToString();
                f3.Text13 = Fish_v.Uoqp2.ToString();
                f3.Text14 = Fish_v.w.ToString();

                f3.Text15 = Fish_v.Vd1.ToString();
                f3.Text16 = Fish_v.Vq1.ToString();
                f3.Text17 = Fish_v.Vd2.ToString();
                f3.Text18 = Fish_v.Vq2.ToString();
            }
            catch
            {
                MessageBox.Show("只是停快了，小场面！！！");
            }

        }
        */


        private void button3_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                Initialized = false;
                flag_uncon = false;
                sn = 44;
                flag_get_runvalue = false;
                button5.Text = "数据采集";
                
                timer1.Enabled = false;

                if (Protocol_num == 1)
                {
                    FCOM2.Monitor_Get_03(44,50);
                    serialPort1.Write(FCOM2.sendbf, 0, 8);
                }

                timer1.Enabled = true;
            }
            else
            {
                MessageBox.Show("串口未打开");
                
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //Thread th = new Thread(new ThreadStart(test)); //创建线程
            //th.Start(); //启动线程

            PSO_v.pso_init = false;
            //IPSO_v.pso_init = false;
            int send_num=0;

            if (!serialPort1.IsOpen)
            {
                MessageBox.Show("请打开串口！");
                return;
            }
            Num_time = 0;

            brun =!brun;
            if(brun)
            {
                textBox1.Text = "系统正在运行";
                button4.Text = "停止";               
            }
            else
            {
                textBox1.Text = "系统停止运行";
                button4.Text = "运行";
                timer3.Enabled = false;
            }

            if (Protocol_num == 0)//FE协议
            {
                NYS_com.Monitor_Run(brun);
                send_num = NYS_com.sendbf[4]+5;
                serialPort1.Write(NYS_com.sendbf, 0, send_num);
            }
            else if (Protocol_num == 1)//modbus
            {
                //1号机1通道
                FCOM2.Monitor_Run(1, 128, brun);
                send_num = 8;
                serialPort1.Write(FCOM2.sendbf, 0, send_num);
            }

            for (int i = 0; i < send_num; i++)
            {
                try
                {
                    if (Protocol_num == 0)
                    {
                        model.Name += Convert.ToString(NYS_com.sendbf[i], 16);
                    }
                    else if (Protocol_num == 1)
                    {
                        model.Name += Convert.ToString(FCOM2.sendbf[i], 16);
                    }
                    model.Name += ' ';
                }
                catch
                {
                    MessageBox.Show("停机冲突");
                }
            }

            model.Name+= '\r';
            model.Name += '\n';
        }

        private void test()
        {
            if(brun)
            { 
                MessageBox.Show("厉害,发现了这个彩蛋"); 
            }
            else
            {
                richTextBox1.Text = "";
                richTextBox1.Text = "开始.net core了";
            }
            
            return;
        }
     


        private void timer1_Tick(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {

                if (flag_uncon == true && Initialized == false )//比对上下设置参数和修正系数是否一致
                {
                    Initialized = true;

                    Form4 f4 = new Form4(this);
                    f4.Show();
                    this.Hide();
                }

                if (!Initialized)//比对未完成
                {
                    if (Protocol_num == 0)
                    {
                        NYS_com.Monitor_Get((byte)sn, (byte)DB_Com.data[0].COMMAND);
                        sn = sn + 1;
                        serialPort1.Write(NYS_com.sendbf, 0, NYS_com.sendbf[4] + 5);

                        if (sn == 118)
                        {
                            sn = 0;
                            Initialized = true;
                            MessageBox.Show("参数一致");
                        }
                    }

                }

                if(flag_get_runvalue==true)
                {
                    if (Protocol_num == 0)
                    {
                        if (sn == DB_Com.runnum)
                        {
                            sn = 0;

                            DB_Com.DataBase_RUN_Save();
                        }

                        NYS_com.Monitor_Get((byte)sn, (byte)DB_Com.data[sn].COMMAND);

                        serialPort1.Write(NYS_com.sendbf, 0, NYS_com.sendbf[4] + 5);

                        sn = sn + 1;

                    }
                    else if (Protocol_num == 1)
                    {
                        DB_Com.DataBase_RUN_Save();

                        FCOM2.Monitor_Get_03(0, DB_Com.runnum);

                        serialPort1.Write(FCOM2.sendbf, 0, 8);
                    }

                }

                if(flag_under_first==true)
                {
                    if (Protocol_num == 0)
                    {
                        if (sn < 118)
                        {

                            NYS_com.Monitor_Get((byte)sn, (byte)DB_Com.data[0].COMMAND);
                            sn = sn + 1;
                        }
                        else
                        {
                            flag_under_first = false;
                            MessageBox.Show("参数初始化完成！！");
                        }

                        serialPort1.Write(NYS_com.sendbf, 0, NYS_com.sendbf[4] + 5);
                    }
                    else if (Protocol_num == 1)
                    {
                        MessageBox.Show("modbus的参数初始化功能缺失！！");
                    }
                }

                if (flag_upper_first == true)
                {
                    if (Protocol_num == 0)
                    {
                        if (sn < 118)
                        {
                            NYS_com.Monitor_Set((byte)sn, (byte)DB_Com.data[sn].COMMAND, DB_Com.data[sn].VALUE);
                            sn = sn + 1;
                        }
                        else
                        {
                            flag_upper_first = false;
                            MessageBox.Show("参数初始化完成！！");
                        }
                        serialPort1.Write(NYS_com.sendbf, 0, NYS_com.sendbf[4] + 5);
                    }
                    else if (Protocol_num == 1)
                    {
                        if (sn < 118)
                        {
                            FCOM2.Monitor_Set_06(sn, DB_Com.data[sn].VALUE);
                            sn = sn + 1;
                        }
                        else
                        {
                            flag_upper_first = false;
                            MessageBox.Show("参数初始化完成！！");
                        }
                        serialPort1.Write(FCOM2.sendbf, 0, 8);
                    }
                }

            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            timer2.Enabled = false;
        }


        private void dataGridView2_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            //old_value = Convert.ToInt32(dataGridView2.Rows[e.RowIndex].Cells[2].Value);//旧值一定是对的，不对的话也不是这里处理
            old_value = Convert.ToSingle(dataGridView2.Rows[e.RowIndex].Cells[2].Value);//旧值一定是对的，不对的话也不是这里处理
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
                dataGridView2.Rows[e.RowIndex].Cells[4].Value = "修改";
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


        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            byte tempsn;
            float i;
            float new_v;
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
                        //Array.Clear(sendbf, 0, sendbf.Length);

                        new_v = i = Convert.ToSingle(dataGridView2.Rows[e.RowIndex].Cells[2].Value);
                        tempsn = Convert.ToByte(dataGridView2.Rows[e.RowIndex].Cells[0].Value);

                        if (Protocol_num==0)
                        {
                            NYS_com.Monitor_Set(tempsn, (byte)(DB_Com.data[tempsn].COMMAND), i);
                            serialPort1.Write(NYS_com.sendbf, 0, NYS_com.sendbf[4] + 5);
                        }
                        else if (Protocol_num==1)
                        {
                            FCOM2.Monitor_Set_06(tempsn, i);
                            serialPort1.Write(FCOM2.sendbf, 0, 8);
                        }

                        DB_Com.data[tempsn].VALUE = new_v;
                        DB_Com.DataBase_SET_Save("PARAMETER_SET", i, tempsn);

                        //Column10.DefaultCellStyle.NullValue = "";
                        dataGridView2.Rows[e.RowIndex].Cells[4].Value = "";
                        bmodify = false;
                    }
                }
            }
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
                        //i = Convert.ToInt16(dataGridView3.Rows[e.RowIndex].Cells[2].Value);
                        i = Convert.ToSingle(dataGridView3.Rows[e.RowIndex].Cells[2].Value);
                        tempsn = Convert.ToByte(dataGridView3.Rows[e.RowIndex].Cells[0].Value);

                        if (Protocol_num == 0)
                        {
                            NYS_com.Monitor_Set(tempsn, (byte)(DB_Com.data[tempsn].COMMAND), i);
                            serialPort1.Write(NYS_com.sendbf, 0, NYS_com.sendbf[4] + 5);
                        }
                        else if (Protocol_num == 1)
                        {
                            FCOM2.Monitor_Set_06(tempsn, i);
                            serialPort1.Write(FCOM2.sendbf, 0, 8);
                        }

                        DB_Com.data[tempsn].VALUE = i;
                        DB_Com.DataBase_SET_Save("PARAMETER_FACTOR",i,tempsn);

                        dataGridView3.Rows[e.RowIndex].Cells[4].Value = "";
                        bmodify = false;
                    }
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            bshow = !bshow;
            if(bshow)
            {
                flag_get_runvalue = true;
                button5.Text = "停止采集";
                sn = 0;
            }
            else
            {
                flag_get_runvalue = false;
                button5.Text = "数据采集";
                sn = 0;
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
            MessageBox.Show("FE/M 2.0版本！");
            test();
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            //DB_Com.DataBase_UG_Save(IPSO_v.u_g);
            if (serialPort1.IsOpen)
            {             
                Num_time++;             
                if (Num_time == 3)//起始第一段数据发送的时间
                {
                    
                    Num_time = 0;

                    Num_DSP = Num_DSP+ 1;
                    if (Num_DSP > 2)
                        Num_DSP = 1;
                    NYS_com.PSO_request((byte)Num_DSP);
                    serialPort1.Write(NYS_com.sendbf, 0, NYS_com.sendbf[4] + 5);
                }         

            }
            
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (brun)
            {
                if(Protocol_num!=0)
                {
                    MessageBox.Show("FE协议才行！");
                    return;
                }
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

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedItem.ToString())
            {
                case "FE协议": Protocol_num = 0; break;
                case "Modbus协议": Protocol_num = 1; break;
                default: Protocol_num = 0; break;
            }
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


