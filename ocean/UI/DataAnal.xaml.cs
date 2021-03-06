using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
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
        int select_index;
       // DataGridTemplateColumn col1 = new DataGridTemplateColumn();

        public DataAnal()
        {
            InitializeComponent();
            //this.DataContext = this;
        }


        private void btRUN_Click(object sender, RoutedEventArgs e)
        {
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
            /*
            brun = !brun;
            if (brun)
            {
                btRUN.Text = "系统正在运行";
                btRUN.Content = "停止";
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
                send_num = NYS_com.sendbf[4] + 5;
                serialPort1.Write(NYS_com.sendbf, 0, send_num);
            }
            else if (Protocol_num == 1)//modbus
            {
                //1号机1通道
                FCOM2.Monitor_Run(1, 128, brun);
                send_num = FCOM2.sendbf[0] + 1;
                serialPort1.Write(FCOM2.sendbf, 0, send_num);
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
            */
        }

        private void btSTOP_Click(object sender, RoutedEventArgs e)
        {

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
            string oldValue = datashow.SelectedCells.ToString();

        }


        private void MButton_Click(object sender, RoutedEventArgs e)
        {
            DB_Access.UpdateDBTable(dtrun, "PARAMETER_RUN");
            //datashow.Columns.Remove(col1);
        }




        private void datashow_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            string newValue = (e.EditingElement as TextBox).Text;

        }


        private void dataset_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            select_index = datashow.SelectedIndex;
            //datashow.Columns.Remove(col1);
        }


        private void MButton2_Click(object sender, RoutedEventArgs e)
        {
            DB_Access.UpdateDBTable(dtset, "PARAMETER_SET");
            //dataset.Columns.Remove(col1);
        }

        private void dataset_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            string newValue = (e.EditingElement as TextBox).Text;

            /*
            FrameworkElementFactory factory1 = new FrameworkElementFactory(typeof(Button));
            //b1.Mode = BindingMode.TwoWay;
            Binding b1 = new Binding("count");
            //b1.Mode = BindingMode.

            factory1.SetValue(Button.ContentProperty, "修改");
            // factory1.SetValue(Button.DataContextProperty,b1);
            factory1.AddHandler(Button.ClickEvent, new RoutedEventHandler(MButton2_Click));
            DataTemplate cellTemplate1 = new DataTemplate();
            cellTemplate1.VisualTree = factory1;
            col1.CellTemplate = cellTemplate1;
            dataset.Columns.Add(col1);
            */
        }


        private void datafactor_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            select_index = datashow.SelectedIndex;

        }


        private void MButton3_Click(object sender, RoutedEventArgs e)
        {
            DB_Access.UpdateDBTable(dtfactor, "PARAMETER_FACTOR");
           // datafactor.Columns.Remove(col1);
        }

        private void datafactor_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            string newValue = (e.EditingElement as TextBox).Text;
           

        }


    }
}
