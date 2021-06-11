using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace fruit
{
    struct Data_r
    {
        public int SN, COMMAND, LENG, NO, TYP, ACK;
        public float VALUE, FACTOR;
        public string NAME, UNITor;
    }
    class DataBase_Interface
    {
        public Data_r[] data = new Data_r[200];
        public int u = 0;
        public int j = 0;
        OleDbConnection conn;
        OleDbCommand cmd;
        OleDbDataReader dr;
        string[] error1 = new string[16];
        public int runnum;
        int knum = 0;//记录存在多少条PRUN记录
        int num_pso = 0;//记录存在多少条PSO记录
        int UG_Num = 0;
        public void DataBase_PARAMETER_RUN_Init()
        {

            //OleDbConnection conn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=MOON.mdb"); //Jet OLEDB:Database Password
            //OleDbCommand cmd = conn.CreateCommand();
            
            conn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=MOON.mdb"); //Jet OLEDB:Database Password
            cmd = conn.CreateCommand();

            cmd.CommandText = "select * from PARAMETER_RUN";
            conn.Open();
            u = j;
            //OleDbDataReader dr = cmd.ExecuteReader();
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
        }


        public void DataBase_PARAMETER_SET_Init()
        {
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
        }

        public void DataBase_PARAMETER_FACTOR_Init()
        {
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
        }


        public void DataBase_ERROR_Table_Init()
        {
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
        }

        public void DataBase_PRUN_create()
        {
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

        public void DataBase_RUN_Save()
        {
            OleDbConnection conn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=fruit.mdb"); //Jet OLEDB:Database Password
            conn.Open();

            //string sql = "insert into PRUN(gspeed,id,iq,rspeed,rid,riq,ud,uq,us) values(";
            string sql = "insert into PRUN values (";
            sql += knum + ",";
            int a;
            for (a = 0; a < runnum; a++)
            {
                sql += data[a].VALUE;
                if (a != runnum - 1)
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

        public void DataBase_SET_Save(string table,float set_num, byte tempsn)
        {
            conn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=MOON.mdb"); //Jet OLEDB:Database Password
            conn.Open();

            string sql = "update "+table+" set [VALUE]=" + set_num + " where SN=" + tempsn;


            OleDbCommand cmd = new OleDbCommand(sql, conn);
            cmd.ExecuteNonQuery();

            conn.Close();
        }



        public void DataBase_UG_create()
        {
            cmd.Dispose();
            conn.Close();
            string query;
            string table_name;
           
            try
            {                                   
                table_name = "UGONE";
                query = "create table ";
                query += table_name;
                query += " (id nvarchar(20) not null)";
                cmd = conn.CreateCommand();
                cmd.CommandText = query;
                conn.Open();
                cmd.ExecuteNonQuery();

                for (int i = 0; i < 4; i++)
                {
                    query = "alter table UGONE add column ";
                    query += "UG";
                    query += i.ToString();
                    query += " float";
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("最后一次提取数据机会！！！");
                UG_Num = 1;
            }
            catch
            {
                query = "drop table UG";
                conn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=FRUIT.mdb"); //Jet OLEDB:Database Password

                cmd = conn.CreateCommand();
                cmd.CommandText = query;
                conn.Open();
                cmd.ExecuteNonQuery();

                query = "drop table UGONE";
                conn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=FRUIT.mdb"); //Jet OLEDB:Database Password

                cmd = conn.CreateCommand();
                cmd.CommandText = query;
                conn.Open();
                cmd.ExecuteNonQuery();

                //query = "create table UG (id nvarchar(20) not null)";
                table_name = "UG";
                query = "create table ";
                query += table_name;
                query += " (id nvarchar(20) not null)";

                cmd.CommandText = query;
                cmd.ExecuteNonQuery();

                for (int i = 0; i < 4; i++)
                {
                    query = "alter table UG add column ";
                    query += "UG";
                    query += i.ToString();
                    query += " float";
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                }
                UG_Num = 0;
            }
            






            cmd.Dispose();
            conn.Close();
        }

        public void DataBase_UG_Save(float []psodata)
        {
            OleDbConnection conn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=fruit.mdb"); //Jet OLEDB:Database Password
            conn.Open();

            string sql;
            if (UG_Num==0)
                sql = "insert into UG values (";
            else
                sql = "insert into UGONE values (";

            sql += num_pso + ",";
            int a;
            for (a = 0; a < psodata.Length; a++)
            {
                sql += psodata[a];
                if (a != psodata.Length-1)
                {
                    sql += ",";
                }
            }
            sql += ")";

            OleDbCommand cmd = new OleDbCommand(sql, conn);
            cmd.ExecuteNonQuery();
            conn.Close();
            //num_pso++;
        }


    }
}
