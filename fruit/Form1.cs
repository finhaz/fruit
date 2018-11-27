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

namespace fruit
{
    struct Data_r
    {
        public int SN, COMMAND, LENG, NO, TYP, VALUE, ACK;
        public float FACTOR;
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
        int old_value, new_value;      
        byte[] sendbf = new byte[20];
        byte[] revbuffer=new byte[256];
        //byte[] sendbuffer=new byte[256];
        int sn;
        int sn1;
        int runnum;
        int nill;
        int head;
        int mrow;
        int knum=0;
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
                OleDbConnection conn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=NYSCOM.mdb"); //Jet OLEDB:Database Password
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
                        data[j].VALUE = dr.GetInt16(dr.GetOrdinal("value"));
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
                conn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=NYSCOM.mdb"); //Jet OLEDB:Database Password
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
                        data[j].VALUE = dr.GetInt16(dr.GetOrdinal("value"));
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
                conn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=NYSCOM.mdb"); //Jet OLEDB:Database Password
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
                        data[j].VALUE = dr.GetInt32(dr.GetOrdinal("value"));
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

                conn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=NYSCOM.mdb"); //Jet OLEDB:Database Password
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
                    query += " int";
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                }

                cmd.Dispose();
                conn.Close();
            }

            catch
            {
                MessageBox.Show("缺少NYSCOM.mdb");
            }
            finally
            {
                CommonRes.serialPort1.DataReceived += SerialPort1_DataReceived;
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

        private void SerialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            //throw new NotImplementedException();
            byte[] buffer = new byte[20];
            int i=0, j=0, k=0;
            short s=0;
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
                revbuffer[nill]= buffer[i];
                str+= Convert.ToString(revbuffer[nill], 16)+' ';
                nill= (nill + 1)%256;             
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
                    if (j == 11)//长度准确
                    {
                        s = revbuffer[(head + 9)%256];
                        s = (short)((s << 8) + revbuffer[(head + 8)%256]);
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
                                k = data[29].VALUE;
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
            sendbf[8] = 0;
            crc = sendbf[4] + sendbf[5] + sendbf[6] + sendbf[7] + sendbf[8];
            crc = ~crc + 1;
            sendbf[9] =(byte)crc;
            CommonRes.serialPort1.Write(sendbf, 0, 10);
            
            for (int i = 0; i < 10; i++)
            {
                richTextBox1.Text += Convert.ToString(sendbf[i],16); 
                richTextBox1.Text += ' ';
            }
            richTextBox1.Text += '\r';
            Array.Clear(sendbf, 0, 19);
            //richTextBox1.Text += '\n';
            //timer2.Enabled = true;
        }



        private void MakeCommand(int sn, byte command, int data)
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
                    sendbf[4]= 5; crc= sendbf[4];
                    sendbf[5]= (byte)sn; crc= crc + sendbf[5];
                    sendbf[6]= command; crc= crc + sendbf[6];
                    sendbf[7]= (byte)data; crc= crc + sendbf[7];
                    sendbf[8]= (byte)(data>>8); crc= crc + sendbf[8];
                    sendbf[9]= (byte)(0 - crc);
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
                CommonRes.serialPort1.Write(sendbf, 0, sendbf[4] + 5);
            }
            catch
            {
                return;
            }
            
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            int crc = 0;
            int i = 0;
            int sdata;
            Array.Clear(sendbf, 0, 19);
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
                        sendbf[4] = 5; crc = sendbf[4];
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
                        sendbf[7] = (byte)(i); crc = crc + sendbf[7];
                        sendbf[8] = (byte)(i >> 8); crc = crc + sendbf[8];
                        sendbf[9] = (byte)(0 - crc);
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
                        sn = (sn + 1) % runnum;

                    }
                }

                if (sn == 0)
                {
                    CommonRes.serialPort1.Write(sendbf, 0, sendbf[4] + 5);

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
                
                //if (sn <=runnum || sn > 43 || sn == 29 || sn == 30)
                {
                    CommonRes.serialPort1.Write(sendbf, 0, sendbf[4] + 5);
                }
               
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            timer2.Enabled = false;
        }



        private void dataGridView3_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            old_value = Convert.ToInt32(dataGridView3.Rows[e.RowIndex].Cells[2].Value);//旧值一定是对的，不对的话也不是这里处理
        }

        private void dataGridView3_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                new_value = Convert.ToInt32(dataGridView3.Rows[e.RowIndex].Cells[2].Value);
                mrow = e.RowIndex;
            }

            catch (OverflowException)
            {
                MessageBox.Show("err:转化的不是一个int型数据");
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

            if (new_value != old_value)
            {
                //Column10.DefaultCellStyle.NullValue = "修改";
                bmodify = true;
                dataGridView3.Rows[e.RowIndex].Cells[4].Value = "修改";
            }
            else
            {
                bmodify = false;
                //Column10.DefaultCellStyle.NullValue = "";
                dataGridView3.Rows[e.RowIndex].Cells[4].Value = "";
            }
        }

        private void dataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            byte tempsn;
            int i;
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
                        i = Convert.ToInt16(dataGridView3.Rows[e.RowIndex].Cells[2].Value);
                        tempsn = Convert.ToByte(dataGridView3.Rows[e.RowIndex].Cells[0].Value);
                        sendbf[4] = 5; crc = sendbf[4];
                        sendbf[5] = tempsn; crc = crc + sendbf[5];
                        sendbf[6] = (byte)(data[tempsn].COMMAND); crc = crc + sendbf[6];
                        sendbf[7] = (byte)(i); crc = crc + sendbf[7]; i = i - sendbf[7]; i = i >> 8;
                        sendbf[8] = (byte)(i); crc = crc + sendbf[8];
                        sendbf[9] = (byte)(0 - crc);

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
            old_value = Convert.ToInt32(dataGridView2.Rows[e.RowIndex].Cells[2].Value);//旧值一定是对的，不对的话也不是这里处理
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            byte tempsn;
            int i;
            int new_v;
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
                        new_v=i = Convert.ToInt16(dataGridView2.Rows[e.RowIndex].Cells[2].Value);
                        tempsn = Convert.ToByte(dataGridView2.Rows[e.RowIndex].Cells[0].Value);
                        sendbf[4] = 5; crc = sendbf[4];
                        sendbf[5] = tempsn; crc = crc + sendbf[5];
                        sendbf[6] = (byte)(data[tempsn].COMMAND); crc = crc + sendbf[6];
                        sendbf[7] = (byte)(i); crc = crc + sendbf[7]; i = i - sendbf[7]; i = i >> 8;
                        sendbf[8] = (byte)(i); crc = crc + sendbf[8];
                        sendbf[9] = (byte)(0 - crc);

                        CommonRes.serialPort1.Write(sendbf, 0, sendbf[4] + 5);

                        OleDbConnection conn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=NYSCOM.mdb"); //Jet OLEDB:Database Password
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

        private void dataGridView2_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                new_value = Convert.ToInt32(dataGridView2.Rows[e.RowIndex].Cells[2].Value);
                mrow = e.RowIndex;
            }

            catch (OverflowException)
            {
                MessageBox.Show("err:转化的不是一个int型数据");
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
