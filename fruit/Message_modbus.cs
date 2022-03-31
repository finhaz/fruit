using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

//RTU格式
//地址 功能码	数据	CRC校验
//1 byte	1 byte	N bytes	2 bytes

namespace fruit
{
    public class Message_modbus//计划设计modbus协议版本
    {
        public byte[] sendbf = new byte[128];
        byte[] revbuffer = new byte[256];


        public void Monitor_Get_03(int sn,int num)
        {
            int crc;
            Array.Clear(sendbf, 0, sendbf.Length);
            sendbf[0] = 0x08;
            sendbf[1] = 0x01;
            sendbf[2] = 0x03;
            //寄存器地址
            byte[] temp_i = BitConverter.GetBytes(sn);            
            sendbf[3] = temp_i[1];
            sendbf[4] = temp_i[0];
            //读取数据个数
            temp_i = BitConverter.GetBytes(num);
            sendbf[5] = temp_i[1]; 
            sendbf[6] = temp_i[0];

            crc = crc16_ccitt(sendbf, 6,1);

            temp_i = BitConverter.GetBytes(crc);
            sendbf[7] = temp_i[0];
            sendbf[8] = temp_i[1];
        }

        public void Monitor_Set_06(int sn,float send_value)
        {
            int crc = 0;
            Int16 svalue = (short)send_value;
            Array.Clear(sendbf, 0, sendbf.Length);
            sendbf[0] = 0x08;
            sendbf[1] = 0x01;
            sendbf[2] = 0x06;
            //寄存器地址
            byte[] temp_i = BitConverter.GetBytes(sn);
            sendbf[3] = temp_i[1];
            sendbf[4] = temp_i[0];

            temp_i = BitConverter.GetBytes(svalue);
            sendbf[5] = temp_i[1];
            sendbf[6] = temp_i[0];
            crc = crc16_ccitt(sendbf, 6,1);

            temp_i = BitConverter.GetBytes(crc);
            sendbf[7] = temp_i[0];
            sendbf[8] = temp_i[1];
        }

        public void Monitor_Run(byte machine,int adr, bool brun)
        {
            int crc;
            sendbf[0] = 0x08;
            sendbf[1] = machine;
            sendbf[2] = 0x06;
            byte[] temp_i = BitConverter.GetBytes(adr);

            sendbf[3] = temp_i[1];
            sendbf[4] = temp_i[0];

            sendbf[5] = 0x00;
            if (brun)
                sendbf[6] = 0xaa;
            else
                sendbf[6] = 0x55;

            crc = crc16_ccitt(sendbf,6,1);

            temp_i=BitConverter.GetBytes(crc);
            sendbf[7] = temp_i[0];
            sendbf[8] = temp_i[1];
        }


        public int monitor_check(byte[] buffer,int len)
        {
            int crc;
            int crc_g;
            int index = len - 2;
            crc = crc16_ccitt(buffer, (len-3), 0);
            if (index > 2)
            {
                crc_g = BitConverter.ToInt16(buffer, index);
                if (crc_g == crc)
                    return 0X01;               
            }
            return 0X03;     
        }

        public UInt16 crc16_ccitt(byte[] data, int len,UInt16 StartIndex)
        {
            UInt16 ccitt16 = 0xA001;
            UInt16 crc = 0xFFFF;

            for (int j= StartIndex; j<=len ; j++)
            {
                crc ^= data[j];
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x0001)==1)
                    {
                        crc >>= 1;
                        crc ^= ccitt16;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }
            return crc;
        }


    }
}
