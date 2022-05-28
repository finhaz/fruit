using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Data;

namespace ocean
{
    public class CommonRes
    {
        public static SerialPort mySerialPort = new SerialPort();

        public static DataTable dt1 = new DataTable();
        public static DataTable dt2 = new DataTable();
        public static DataTable dt3 = new DataTable();
    }

}
