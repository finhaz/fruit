using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using SomeNameSpace;
using static System.Net.Mime.MediaTypeNames;

namespace ocean.UI
{
    /// <summary>
    /// DataAnal.xaml 的交互逻辑
    /// </summary>
    public partial class DataAnal : Page
    {
        public DataTable dtrun { get; set; }
        public DataTable dtset { get; set; }
        public DataTable dtfactor { get; set; }
        

        //数据库对接
        int select_index;
        DataBase_Interface DB_Com = new DataBase_Interface();
        string newValue;

        bool brun = false;
        bool bshow = false;
        //bool bmodify = false;
        //bool Initialized = true;//调试参数与修正系数核验标志位
        //bool flag_uncon = false;//上下参数不一致标志位
        //bool flag_get_runvalue = false;//读取运行数据标志位


        public bool flag_under_first = false;
        public bool flag_upper_first = false;

        bool sopen = false;
        float old_value;
        float new_value;
        public int sn = 0;
        int mrow;
        int Num_time = 0;
        int Num_DSP = 0;
        int Protocol_num = 1;
        static int Set_Num_DSP = 2;//代表有机子数


        //通讯协议
        Message NYS_com = new Message();
        Message_modbus FCOM2 = new Message_modbus();

        //定时器
        private DispatcherTimer mDataTimer = null; //定时器
        private long timerExeCount = 0; //定时器执行次数
        DateTime s1;
        DateTime s2;


        public DataAnal()
        {
            InitializeComponent();
        }


        private void InitTimer()
        {
            if (mDataTimer == null)
            {
                mDataTimer = new DispatcherTimer();
                mDataTimer.Tick += new EventHandler(DataTimer_Tick);
                mDataTimer.Interval = TimeSpan.FromSeconds(10);
            }
        }
        private void DataTimer_Tick(object sender, EventArgs e)
        {
            s2 = DateTime.Now;
            s1 = DateTime.Now;
            ++timerExeCount;

            if (bshow == true)
            {
                if (Protocol_num == 0)
                {
                    //if (sn == DB_Com.runnum)
                    //{
                    //    sn = 0;
                    //    DB_Com.DataBase_RUN_Save();
                    //}

                    NYS_com.Monitor_Get((byte)sn, (byte)DB_Com.data[sn].COMMAND);

                    CommonRes.mySerialPort.Write(NYS_com.sendbf, 0, NYS_com.sendbf[4] + 5);

                    sn = sn + 1;

                }
                else if (Protocol_num == 1)
                {
                    //DB_Com.DataBase_RUN_Save();

                    FCOM2.Monitor_Get_03(0, DB_Com.runnum);

                    CommonRes.mySerialPort.Write(FCOM2.sendbf, 0, 8);
                }

            }
        }

        public void StartTimer()
        {
            if (mDataTimer != null && mDataTimer.IsEnabled == false)
            {
                mDataTimer.Start();
                s1 = DateTime.Now;
            }
        }
        public void StopTimer()
        {
            if (mDataTimer != null && mDataTimer.IsEnabled == true)
            {
                mDataTimer.Stop();
            }
        }



        private void btRUN_Click(object sender, RoutedEventArgs e)
        {
            //bool brun;
            //Thread th = new Thread(new ThreadStart(test)); //创建线程
            //th.Start(); //启动线程

            //PSO_v.pso_init = false;
            //IPSO_v.pso_init = false;
            int send_num = 0;

            if (!CommonRes.mySerialPort.IsOpen)
            {
                MessageBox.Show("请打开串口！");
                return;
            }
            //Num_time = 0;

            brun = true;

            textBox1.Text= "系统正在运行";

            if (Protocol_num == 0)//FE协议
            {
                NYS_com.Monitor_Run(brun);
                send_num = NYS_com.sendbf[4] + 5;
                CommonRes.mySerialPort.Write(NYS_com.sendbf, 0, send_num);
            }
            else if (Protocol_num == 1)//modbus
            {
                //1号机1通道
                FCOM2.Monitor_Run(1, 128, brun);
                send_num = 8;
                CommonRes.mySerialPort.Write(FCOM2.sendbf, 0, send_num);
            }

            string txt = "TX:";
            for (int i = 0; i < send_num; i++)
            {  
                if (Protocol_num == 0)
                {
                    txt += Convert.ToString(NYS_com.sendbf[i], 16);                    
                }
                else if (Protocol_num == 1)
                {
                    txt += Convert.ToString(FCOM2.sendbf[i], 16);
                }
                txt += ' ';
            }
            txt += '\r';
            txt += '\n';
            show_text.Text+=txt;              
            

        }

        private void btSTOP_Click(object sender, RoutedEventArgs e)
        {
            //bool brun;
            int send_num = 0;
            brun =false;
            textBox1.Text = "系统停止运行";
            if (Protocol_num == 0)//FE协议
            {
                NYS_com.Monitor_Run(brun);
                send_num = NYS_com.sendbf[4] + 5;
                CommonRes.mySerialPort.Write(NYS_com.sendbf, 0, send_num);
            }
            else if (Protocol_num == 1)//modbus
            {
                //1号机1通道
                FCOM2.Monitor_Run(1, 128, brun);
                send_num = 8;
                CommonRes.mySerialPort.Write(FCOM2.sendbf, 0, send_num);
            }

            string txt = "TX:";
            for (int i = 0; i < send_num; i++)
            {
                if (Protocol_num == 0)
                {
                    txt += Convert.ToString(NYS_com.sendbf[i], 16);
                }
                else if (Protocol_num == 1)
                {
                    txt += Convert.ToString(FCOM2.sendbf[i], 16);
                }
                txt += ' ';
            }
            txt += '\r';
            txt += '\n';
            show_text.Text+=txt;
        }

        private void Page_Loaded(object sender, EventArgs e)
        {
            //dt1 = DB_Access.GetDBTable("PARAMETER_RUN");
            //datashow.ItemsSource = CommonRes.dt1.DefaultView;

            //dt2 = DB_Access.GetDBTable("PARAMETER_SET");
            //dataset.ItemsSource = CommonRes.dt2.DefaultView;

            //dt3 = DB_Access.GetDBTable("PARAMETER_FACTOR");
            //datafactor.ItemsSource = CommonRes.dt3.DefaultView;

            dtrun = CommonRes.dt1;

            dtset = CommonRes.dt2;

            dtfactor = CommonRes.dt3;

            InitTimer();
            CommonRes.mySerialPort.DataReceived += new SerialDataReceivedEventHandler(this.mySerialPort_DataReceived);
        }

        private void mySerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(99);
            int n = CommonRes.mySerialPort.BytesToRead;
            byte[] buf = new byte[n];
            CommonRes.mySerialPort.Read(buf, 0, n);

            string txt = "RX:";
            for (int i = 0; i < n; i++)
            {
                if (Protocol_num == 0)
                {
                    txt += Convert.ToString(buf[i], 16);
                }
                else if (Protocol_num == 1)
                {
                    txt += Convert.ToString(buf[i], 16);
                }
                txt += ' ';
            }
            txt += '\r';
            txt += '\n';
            output(txt);
        }


        private void datashow_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            select_index = datashow.SelectedIndex;
        }


        private void MButton_Click(object sender, RoutedEventArgs e)
        {
            DB_Access.UpdateDBTable(dtrun, "PARAMETER_RUN");
            //datashow.Columns.Remove(col1);
        }




        private void datashow_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            newValue = (e.EditingElement as TextBox).Text;
            
        }

        private void dataset_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            //读取选中行
            var x = dataset.SelectedIndex;
        }


        private void MButton2_Click(object sender, RoutedEventArgs e)
        {
            //读取选中行
            var x = dataset.SelectedIndex;
            string y = dtset.Rows[0][0].ToString();
            int z = Convert.ToInt32(y);
            int tempsn = x + z;
            string val = dtset.Rows[x][5].ToString();
            float value=Convert.ToSingle(val);
            DB_Com.DataBase_SET_Save("PARAMETER_SET", value, (byte)select_index);
            if (CommonRes.mySerialPort.IsOpen==true)
            {
                if (Protocol_num == 0)
                {
                    NYS_com.Monitor_Set((byte)tempsn, (byte)(DB_Com.data[tempsn].COMMAND), value);
                    CommonRes.mySerialPort.Write(NYS_com.sendbf, 0, NYS_com.sendbf[4] + 5);
                }
                else if (Protocol_num == 1)
                {
                    FCOM2.Monitor_Set_06(tempsn, value);
                    CommonRes.mySerialPort.Write(FCOM2.sendbf, 0, 8);
                }
            }
            else
            {
                MessageBox.Show("请打开串口！");
            }
        }


        private void dataset_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            newValue = (e.EditingElement as TextBox).Text;
            CommonRes.dt2 = dtset;
        }


        private void datafactor_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {            
            var x = datafactor.SelectedIndex;
        }


        private void MButton3_Click(object sender, RoutedEventArgs e)
        {
            //读取选中行
            var x = datafactor.SelectedIndex;           
            string y = dtfactor.Rows[0][0].ToString();
            int z = Convert.ToInt32(y);
            int tempsn = x + z;
            string val = dtfactor.Rows[x][2].ToString();
            float value = Convert.ToSingle(val);
            DB_Com.DataBase_SET_Save("PARAMETER_FACTOR", value, (byte)select_index);
            if (CommonRes.mySerialPort.IsOpen==true)
            {
                if (Protocol_num == 0)
                {
                    NYS_com.Monitor_Set((byte)tempsn, (byte)(DB_Com.data[tempsn].COMMAND), value);
                    CommonRes.mySerialPort.Write(NYS_com.sendbf, 0, NYS_com.sendbf[4] + 5);
                }
                else if (Protocol_num == 1)
                {
                    FCOM2.Monitor_Set_06(tempsn, value);
                    CommonRes.mySerialPort.Write(FCOM2.sendbf, 0, 8);
                }
            }
            else
            {
                MessageBox.Show("请打开串口！");
            }
        }

        private void datafactor_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            newValue = (e.EditingElement as TextBox).Text;
            CommonRes.dt3 = dtfactor;
        }

        private void btShow_Click(object sender, RoutedEventArgs e)
        {
            if (CommonRes.mySerialPort.IsOpen == true)
            {
                bshow = !bshow;
                if (bshow)
                {
                    btShow.Content = "停止采集";
                    //MessageBox.Show("数据采集开始");
                    StartTimer();
                }
                else
                {
                    btShow.Content = "开始采集";
                    StopTimer();
                }
            }
            else
            {
                MessageBox.Show("打开串口！");
            }

        }


        private void cbProcho_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch(cbProcho.SelectedIndex)
            {
                case 0: Protocol_num = 0; break;
                case 1: Protocol_num = 1; break;
                default: Protocol_num = 0; break;
            }
        }

        private delegate void outputDelegate(string para);
        private void output(string para)
        {
            this.show_text.Dispatcher.Invoke(new outputDelegate(outputAction), para);
        }
        private void outputAction(string para)
        {
            
            show_text.Text+=para;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            CommonRes.mySerialPort.DataReceived -= new SerialDataReceivedEventHandler(this.mySerialPort_DataReceived);
        }
    }
}
