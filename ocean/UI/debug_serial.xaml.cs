using System;
using System.Collections.Generic;
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
using System.IO.Ports;
using System.Windows.Threading;

namespace ocean.UI
{
    /// <summary>
    /// debug_serial.xaml 的交互逻辑
    /// </summary>
    public partial class debug_serial : Page
    {
        delegate void HanderInterfaceUpdataDelegate(string mySendData);
        HanderInterfaceUpdataDelegate myUpdataHander;
        delegate void txtGotoEndDelegate();

        public bool ckHexState;


        public debug_serial()
        {
            InitializeComponent();
        }

        //SerialPort mySerialPort = new SerialPort();
        DispatcherTimer time1 = new DispatcherTimer();

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            #region//串口设置初始化
            //串口cbComName.Text
            string[] portsName = SerialPort.GetPortNames();
            Array.Sort(portsName);
            cbPortName.ItemsSource = portsName;
            cbPortName.Text = Convert.ToString(cbPortName.Items[0]);
            //波特率cbBaudRate.Text
            int[] baudRateData = { 4800, 9600, 19200, 38400, 43000, 56000 };
            cbBaudRate.ItemsSource = baudRateData;
            cbBaudRate.Text = Convert.ToString(cbBaudRate.Items[1]);
            //检验位cbParity.Text
            string[] parityBit = { "无", "奇校验", "偶校验" };
            cbParity.ItemsSource = parityBit;
            cbParity.Text = Convert.ToString(cbParity.Items[0]);
            //数据位cbDataBits.Text
            int[] dataBits = { 6, 7, 8 };
            cbDataBits.ItemsSource = dataBits;
            cbDataBits.Text = Convert.ToString(cbDataBits.Items[2]);
            //停止位cbStopBits.Text
            int[] stopBits = { 1, 2 };
            cbStopBits.ItemsSource = stopBits;
            cbStopBits.Text = Convert.ToString(cbStopBits.Items[0]);
            #endregion
            CommonRes.mySerialPort.DataReceived += new SerialDataReceivedEventHandler(this.mySerialPort_DataReceived);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            CommonRes.mySerialPort.Encoding = System.Text.Encoding.GetEncoding("GB2312");
            //mySerialPort.Encoding = System.Text.Encoding.GetEncoding("UTF8");
            ckHexState = (bool)ck16View.IsChecked;

            time1.Tick += new EventHandler(time1_Tick);
            tbkIntervalTime.Visibility = Visibility.Hidden;
            tbIntervalTime.Visibility = Visibility.Hidden;
            bdExpend.Visibility = Visibility.Hidden;


            if(CommonRes.mySerialPort.IsOpen)
            {
                cbBaudRate.IsEnabled = false;
                cbDataBits.IsEnabled = false;
                cbParity.IsEnabled = false;
                cbPortName.IsEnabled = false;
                cbStopBits.IsEnabled = false;

                btOpenCom.Content = "关闭串口";
                comState.Style = (Style)this.FindResource("EllipseStyleGreen");
            }

        }

        private void time1_Tick(object sender, EventArgs e)
        {
            if (CommonRes.mySerialPort.IsOpen)
            {
                btSend_Event(tbSend.Text, (bool)ck16Send.IsChecked);
            }
        }

        private void btOpenCom_Click(object sender, RoutedEventArgs e)
        {

            if (CommonRes.mySerialPort.IsOpen)
            {
                CommonRes.mySerialPort.Close();
                cbBaudRate.IsEnabled = true;
                cbDataBits.IsEnabled = true;
                cbParity.IsEnabled = true;
                cbPortName.IsEnabled = true;
                cbStopBits.IsEnabled = true;
                btOpenCom.Content = "打开串口";
                tbComState.Text = cbPortName.Text + "已关闭";
                comState.Style = (Style)this.FindResource("EllipseStyleRed");
            }
            else
            {
                CommonRes.mySerialPort.PortName = cbPortName.Text;
                CommonRes.mySerialPort.BaudRate = Convert.ToInt32(cbBaudRate.Text);
                switch (Convert.ToString(cbParity.Text))
                {
                    case "无":
                        CommonRes.mySerialPort.Parity = Parity.None;
                        break;
                    case "奇校验":
                        CommonRes.mySerialPort.Parity = Parity.Odd;
                        break;
                    case "偶校验":
                        CommonRes.mySerialPort.Parity = Parity.Even;
                        break;
                }
                switch (Convert.ToInt32(cbStopBits.Text))
                {
                    case 0:
                        CommonRes.mySerialPort.StopBits = StopBits.None;
                        break;
                    case 1:
                        CommonRes.mySerialPort.StopBits = StopBits.One;
                        break;
                    case 2:
                        CommonRes.mySerialPort.StopBits = StopBits.Two;
                        break;
                }
                try
                {
                    CommonRes.mySerialPort.Open();
                }
                catch
                {
                    tbComState.Text = cbPortName.Text + "串口被占用！";
                    MessageBox.Show("串口被占用！");
                    return;
                }
                cbBaudRate.IsEnabled = false;
                cbDataBits.IsEnabled = false;
                cbParity.IsEnabled = false;
                cbPortName.IsEnabled = false;
                cbStopBits.IsEnabled = false;
                btOpenCom.Content = "关闭串口";
                tbComState.Text = cbPortName.Text + "," + cbBaudRate.Text + "," +
                    cbParity.Text + "," + cbDataBits.Text + "," + cbStopBits.Text;
                comState.Style = (Style)this.FindResource("EllipseStyleGreen");
            }

        }

        private void btSend_Click(object sender, RoutedEventArgs e)
        {
            btSend_Event(tbSend.Text, (bool)ck16Send.IsChecked);
        }

        private void getControlState()
        {

        }

        private void getData(string sendData)
        {
            tbReceive.Text += sendData;
        }

        private void txtGotoEnd()
        {
            tbReceive.ScrollToEnd();
        }

        private void txtReciveEvent(string byteNum)
        {
            txtRecive.Text = Convert.ToString(Convert.ToInt32(txtRecive.Text) + Convert.ToInt32(byteNum));
        }

        private void mySerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {

            int n = CommonRes.mySerialPort.BytesToRead;
            byte[] buf = new byte[n];
            CommonRes.mySerialPort.Read(buf, 0, n);
            myUpdataHander = new HanderInterfaceUpdataDelegate(getData);
            txtGotoEndDelegate myGotoend = txtGotoEnd;
            HanderInterfaceUpdataDelegate myUpdata1 = new HanderInterfaceUpdataDelegate(txtReciveEvent);
            string abc, abc1;
            if (ckHexState == true)
            {
                abc = ByteArrayToHexString(buf);
                string hexStringView = "";
                for (int i = 0; i < abc.Length; i += 2)
                {
                    hexStringView += abc.Substring(i, 2) + " ";
                }
                abc = hexStringView;
                abc1 = abc.Replace(" ", "");
                if (abc1.Substring(abc1.Length - 2, 2) == "0D")
                {
                    abc = abc + "\n";
                }

            }
            else
            {
                abc = System.Text.Encoding.Default.GetString(buf);
            }
            Dispatcher.Invoke(myUpdataHander, new string[] { abc });
            Dispatcher.Invoke(myGotoend);
            Dispatcher.Invoke(myUpdata1, new string[] { n.ToString() });
        }



        public string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0'));
            return sb.ToString().ToUpper();
        }

        public byte[] HexStringToByteArray(string s)
        {
            //s=s.ToUpper();
            s = s.Replace(" ", "");
            if (s.Length % 2 != 0)
            {
                s = s.Substring(0, s.Length - 1) + "0" + s.Substring(s.Length - 1);
            }
            byte[] buffer = new byte[s.Length / 2];


            try
            {
                for (int i = 0; i < s.Length; i += 2)
                    buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
                return buffer;
            }
            catch
            {
                string errorString = "E4";
                byte[] errorData = new byte[errorString.Length / 2];
                errorData[0] = (byte)Convert.ToByte(errorString, 16);
                return errorData;
            }
        }

        public string StringToHexString(string s)
        {
            //s = s.ToUpper();
            s = s.Replace(" ", "");

            string buffer = "";
            char[] myChar;
            myChar = s.ToCharArray();
            for (int i = 0; i < s.Length; i++)
            {
                //buffer = buffer + Convert.ToInt32(myChar[i]);
                buffer = buffer + Convert.ToString(myChar[i], 16);
                buffer = buffer.ToUpper();
            }
            return buffer;
        }

        private void btClearView_Click(object sender, RoutedEventArgs e)
        {
            tbReceive.Text = "";
        }

        private void ck16View_Click(object sender, RoutedEventArgs e)
        {
            ckHexState = (bool)ck16View.IsChecked;
        }

        private void btSend_Event(string strSend, bool hexState)
        {
            if (CommonRes.mySerialPort.IsOpen)
            {
                if (hexState == false)
                {
                    //if (ckAdvantechCmd.IsChecked == true) { strSend = strSend.ToUpper(); }
                    byte[] sendData = System.Text.Encoding.Default.GetBytes(strSend);
                    CommonRes.mySerialPort.Write(sendData, 0, sendData.Length);
                    txtSend.Text = Convert.ToString(Convert.ToInt32(txtSend.Text) + Convert.ToInt32(sendData.Length));
                    if (ckAdvantechCmd.IsChecked == true)
                    {
                        byte[] sendAdvCmd = HexStringToByteArray("0D");
                        CommonRes.mySerialPort.Write(sendAdvCmd, 0, 1);
                        txtSend.Text = Convert.ToString(Convert.ToInt32(txtSend.Text) + Convert.ToInt32(sendData.Length));
                    }
                }
                else
                {
                    byte[] sendHexData = HexStringToByteArray(strSend);
                    CommonRes.mySerialPort.Write(sendHexData, 0, sendHexData.Length);
                }
            }
            else
            {
                tbComState.Text = "串口未开";
                MessageBox.Show("串口没有打开，请检查！");
            }
        }

        private void ckAutoSend_Click(object sender, RoutedEventArgs e)
        {
            if (CommonRes.mySerialPort.IsOpen == false)
            {
                MessageBox.Show("串口未开！");
                ckAutoSend.IsChecked = false;
                return;
            }
            if (ckAutoSend.IsChecked == true)
            {

                tbkIntervalTime.Visibility = Visibility.Visible;
                tbIntervalTime.Visibility = Visibility.Visible;
                time1.Interval = TimeSpan.FromSeconds(Convert.ToDouble(tbIntervalTime.Text));
                if (Convert.ToDouble(tbIntervalTime.Text) == 0)
                {
                    return;
                }
                else
                {
                    time1.Start();
                }
            }
            else
            {
                tbkIntervalTime.Visibility = Visibility.Hidden;
                tbIntervalTime.Visibility = Visibility.Hidden;
                time1.Stop();
            }
            tbReceive.ScrollToEnd();
        }

        private void ck16Send_Click(object sender, RoutedEventArgs e)
        {
            //get16View((bool)ck16Send.IsChecked);
        }

        private void get16View(bool isHex)
        {
            if (isHex == true)
            {
                //将字符器转为Ascii码
                string hexString, hexStringView = "";
                hexString = StringToHexString(tbSend.Text);
                for (int i = 0; i < hexString.Length; i += 2)
                {
                    hexStringView += hexString.Substring(i, 2) + " ";
                }
                if (ckAdvantechCmd.IsChecked == true) { hexStringView += "0D"; }

                TextBox myText = grdSend.FindName("tb16View") as TextBox;
                //如果已有tb16View这个控件，则进行显示，如没有则创建并进行显示
                if (myText != null)
                {
                    myText.Text = hexStringView;
                }
                else
                {
                    #region//创建一个文本框来显示字符串的Acsii码
                    TextBox myTextBox = new TextBox();
                    myTextBox.VerticalAlignment = VerticalAlignment.Top;
                    myTextBox.Height = 22;
                    myTextBox.Margin = new Thickness(3, 0, 0, 0);
                    myTextBox.IsReadOnly = true;
                    myTextBox.IsEnabled = true;
                    tbSend.Margin = new Thickness(3, 22, 0, 0);
                    grdSend.Children.Add(myTextBox);
                    grdSend.RegisterName("tb16View", myTextBox);
                    myTextBox.SetValue(Grid.ColumnProperty, 1);
                    myTextBox.Text = hexStringView;
                    #endregion
                }
            }
            else
            {
                //移除tb16View控件
                TextBox myTextBox = grdSend.FindName("tb16View") as TextBox;
                if (myTextBox != null)
                {
                    grdSend.Children.Remove(myTextBox);//移除对应按钮控件   
                    grdSend.UnregisterName("tb16View");
                    tbSend.Margin = new Thickness(3, 0, 0, 0);
                }
            }
        }

        private void ckAdvantechCmd_Click(object sender, RoutedEventArgs e)
        {
            ck16Send_Click(sender, e);
            if (ckAsciiView.IsChecked == true)
            {
                get16View((bool)ckAsciiView.IsChecked);
            }
        }

        private void tbIntervalTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ckAutoSend.IsChecked == true)
            {
                if (Convert.ToDouble(tbIntervalTime.Text) == 0)
                {
                    time1.Stop();
                }
                else
                {
                    time1.Interval = TimeSpan.FromSeconds(Convert.ToDouble(tbIntervalTime.Text));
                    time1.Start();
                }
            }
        }

        private void btExpend_Click(object sender, RoutedEventArgs e)
        {
            if (bdExpend.IsVisible == true)
            {
                bdExpend.Visibility = Visibility.Hidden;
                tbReceive.Margin = new Thickness(0, 1, 0, 0);
            }
            else
            {
                bdExpend.Visibility = Visibility.Visible;
                tbReceive.Margin = new Thickness(0, 1, bdExpend.Width + 5, 0);
            }
            CheckBox ckBox = gdExpend.FindName("ckExpend0") as CheckBox;
            if (ckBox == null)
            {
                bdExpend.Visibility = Visibility.Visible;
                for (int i = 0; i < 10; i++)
                {
                    CheckBox newCkBox = new CheckBox();
                    newCkBox.VerticalAlignment = VerticalAlignment.Center;
                    newCkBox.HorizontalAlignment = HorizontalAlignment.Center;
                    gdExpend.Children.Add(newCkBox);
                    newCkBox.SetValue(Grid.RowProperty, i + 1);
                    gdExpend.RegisterName("ckExpend" + i, newCkBox);

                    TextBox newTextBox = new TextBox();
                    gdExpend.Children.Add(newTextBox);
                    newTextBox.SetValue(Grid.RowProperty, i + 1);
                    newTextBox.SetValue(Grid.ColumnProperty, 1);
                    newTextBox.Margin = new Thickness(0, 3, 5, 3);
                    gdExpend.RegisterName("expendTextBox" + i, newTextBox);

                    Button newButton = new Button();
                    gdExpend.Children.Add(newButton);
                    newButton.SetValue(Grid.RowProperty, i + 1);
                    newButton.SetValue(Grid.ColumnProperty, 2);
                    newButton.Margin = new Thickness(0, 3, 0, 3);
                    newButton.Content = "发送" + i;
                    newButton.Click += new RoutedEventHandler(dynamicButton_Click);
                }
            }
        }

        private void dynamicButton_Click(object sender, RoutedEventArgs e)
        {
            Button bt1 = (Button)sender;
            string str = "";
            str = Convert.ToString(bt1.Content);
            TextBox tb1 = gdExpend.FindName("expendTextBox" + str.Substring(str.Length - 1, 1)) as TextBox;
            CheckBox ck1 = gdExpend.FindName("ckExpend" + str.Substring(str.Length - 1, 1)) as CheckBox;
            btSend_Event(tb1.Text, (bool)ck1.IsChecked);
            //MessageBox.Show("点了确定！" + str.Substring(str.Length - 1, 1));
        }

        private void tbSend_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ckAsciiView.IsChecked == true)
            {
                get16View((bool)ckAsciiView.IsChecked);
            }
        }

        private void ckAsciiView_Click(object sender, RoutedEventArgs e)
        {
            get16View((bool)ckAsciiView.IsChecked);
        }

    }
}
