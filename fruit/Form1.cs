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

    public partial class Form1 : Form
    {
        bool brun = false;
        bool bshow =false;
        bool bmodify = false;
        bool Initialized = false;
        bool sopen=false;
        float old_value;
        float new_value;
        int sn;
        int sn1;       
        int mrow;       
        int Num_time = 0;
        int Num_DSP=0;       
        static int Set_Num_DSP = 2;//代表有机子数

        Form3 f3 = new Form3();
        private TestBind model = new TestBind();
        PSO_analysis PSO_v = new PSO_analysis();
        IPSO_analysis IPSO_v = new IPSO_analysis();
        Fish_Solution Fish_v = new Fish_Solution();
        Message NYS_com= new Message();
        DataBase_Interface DB_Com=new DataBase_Interface();

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

                DB_Com.DataBase_PRUN_create();

                DB_Com.DataBase_UG_create();
            }

            catch
            {
                MessageBox.Show("缺少MOON.mdb");
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
            //该事件函数在新的线程执行
            //没使用数据绑定前，此代码不可注释
            //Control.CheckForIllegalCrossThreadCalls = false;
            //throw new NotImplementedException();
            
            byte[] buffer = new byte[80];
            int i = 0;
            int j = 0;
            
            string str = "";
            int n_dsp = 0;
            int check_result=0;

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

            check_result=NYS_com.monitor_check(buffer);

            if (check_result == 1)
            {
                n_dsp = buffer[7];
                for (int i_k = 0; i_k < 9; i_k++)//字节重新拼接为浮点数
                {
                    //PSO_v.u_dsp[n_dsp - 1, i_k] = BitConverter.ToSingle(buffer, 8 + 4 * i_k);

                    IPSO_v.u_dsp[n_dsp - 1, i_k] = BitConverter.ToSingle(buffer, 8 + 4 * i_k);

                    //Fish_v.u_dsp[n_dsp - 1, i_k] = BitConverter.ToSingle(buffer, 8 + 4 * i_k);


                }
                if (n_dsp == Set_Num_DSP)
                {
                    /*
                    Thread th = new Thread(new ThreadStart(PSO_v.cale_pso)); //创建PSO线程
                    th.Start(); //启动线程                          
                    Thread th1 = new Thread(new ThreadStart(update_UI_PSO)); //创建UI线程
                    th1.Start(); //启动线程
                    */

                    
                    Thread th = new Thread(new ThreadStart(IPSO_v.cale_pso)); //创建IPSO线程
                    th.Start(); //启动线程                          
                    Thread th1 = new Thread(new ThreadStart(update_UI_IPSO)); //创建UI线程
                    th1.Start(); //启动线程
                    

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
                DB_Com.data[buffer[5]].VALUE = BitConverter.ToSingle(buffer, 8);

                if (DB_Com.data[buffer[5]].SN == buffer[5])
                {
                    dataGridView1.Rows[DB_Com.data[buffer[5]].SN].Cells[2].Value = DB_Com.data[buffer[5]].VALUE;
                }
                DB_Com.data[buffer[5]].ACK = buffer[7];

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


        private void button3_Click(object sender, EventArgs e)
        {
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
            //Thread th = new Thread(new ThreadStart(test)); //创建线程
            //th.Start(); //启动线程

            PSO_v.pso_init = false;
            IPSO_v.pso_init = false;

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

            NYS_com.Monitor_Run(brun);
            serialPort1.Write(NYS_com.sendbf, 0, 10);

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    //richTextBox1.Text += Convert.ToString(sendbf[i], 16);
                    //richTextBox1.Text += ' ';
                    model.Name+= Convert.ToString(NYS_com.sendbf[i], 16);
                    model.Name += ' ';
                }
                catch
                {
                    MessageBox.Show("停机冲突");
                }
            }
            model.Name+= '\r';


            Array.Clear(NYS_com.sendbf, 0, 19);

            //richTextBox1.Text += '\n';
            model.Name += '\n';
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
     


        private void timer1_Tick(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                if (!Initialized)
                {
                    Initialized = true;//若需要启用初始化检测，应注释这句话
                    if (sn > 43)
                    {
                        NYS_com.MakeCommand(sn, 0, DB_Com.data[sn].VALUE);
                    }
                    if (DB_Com.data[sn].ACK == -2)
                        DB_Com.data[sn].ACK = -1;
                    else if (DB_Com.data[sn].ACK == -1)
                    {
                        DB_Com.data[sn].ACK = 0;
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
                        NYS_com.Monitor_Initial(sn1, DB_Com.data[sn1].VALUE);
                        sn1 = -1;
                    }

                    else
                    {
                        NYS_com.Monitor_Get((byte)sn, (byte)DB_Com.data[sn].COMMAND);
                        sn = sn + 1;
                    }
                }


                serialPort1.Write(NYS_com.sendbf, 0, NYS_com.sendbf[4] + 5);


                if (sn == DB_Com.runnum)
                {
                    sn = 0;

                    DB_Com.DataBase_RUN_Save();
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
                        NYS_com.Monitor_Set(tempsn, (byte)(DB_Com.data[tempsn].COMMAND), i);
                        serialPort1.Write(NYS_com.sendbf, 0, NYS_com.sendbf[4] + 5);

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
                        NYS_com.Monitor_Set(tempsn, (byte)(DB_Com.data[tempsn].COMMAND), i);
                        serialPort1.Write(NYS_com.sendbf, 0, NYS_com.sendbf[4] + 5);

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
                timer1.Enabled = true;
                button5.Text = "停止采集";
            }
            else
            {
                timer1.Enabled = false;
                button5.Text = "数据采集";
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
