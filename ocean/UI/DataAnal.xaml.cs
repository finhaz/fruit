using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
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
using SomeNameSpace;

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
        //bool bshow = false;
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
        int Protocol_num = 0;
        static int Set_Num_DSP = 2;//代表有机子数


        //通讯协议
        Message NYS_com = new Message();
        Message_modbus FCOM2 = new Message_modbus();

        public DataAnal()
        {
            InitializeComponent();
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
                send_num = FCOM2.sendbf[0] + 1;
                CommonRes.mySerialPort.Write(FCOM2.sendbf, 0, send_num);
            }

            for (int i = 0; i < send_num; i++)
            {
                try
                {
                    //if (Protocol_num == 0)
                    {
                       // model.Name += Convert.ToString(NYS_com.sendbf[i], 16);
                    }
                    //else if (Protocol_num == 1)
                    {
                      //  model.Name += Convert.ToString(FCOM2.sendbf[i], 16);
                    }
                    //model.Name += ' ';
                }
                catch
                {
                    MessageBox.Show("停机冲突");
                }
            }
            //model.Name += '\r';
            //model.Name += '\n';

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
                send_num = FCOM2.sendbf[0] + 1;
                CommonRes.mySerialPort.Write(FCOM2.sendbf, 0, send_num);
            }
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

            //col1.Header = "操作";
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
        }


    }
}
