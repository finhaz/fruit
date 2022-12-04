using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SomeNameSpace
{
    class SB_Msqlite
    {
        public static string dbfile = "Data Source=test.db";
        

        static void create_table(string dbfile)
        {
            //string dbfile = @"URI=file:sql.db";
            SqliteConnection cnn = new SqliteConnection(dbfile);
            cnn.Open();

            string sql = "Create table Person (Id integer primary key, Name text);";
            SqliteCommand cmd = new SqliteCommand(sql, cnn);
            cmd.ExecuteNonQuery();
            cnn.Close();
        }

        void addtableline(string dbfile)
        {
            //string dbfile = @"URI=file:sql.db";
            SqliteConnection cnn = new SqliteConnection(dbfile);
            cnn.Open();

            string sql = "insert into  Person (Id , Name) values(1,'Mike');";
            SqliteCommand cmd = new SqliteCommand(sql, cnn);
            cmd.ExecuteNonQuery();
            cnn.Close();

            Console.WriteLine("Insert row OK");

        }

        void find_data(string dbfile)
        {
            //string dbfile = @"URI=file:sql.db";

            SqliteConnection cnn = new SqliteConnection(dbfile);

            cnn.Open();

            string sql = "Select * From  Person";

            SqliteCommand cmd = new SqliteCommand(sql, cnn);

            SqliteDataReader reader = cmd.ExecuteReader();

            while (reader.Read())

            {

                Console.WriteLine($"{reader.GetInt32(0)}  {reader.GetString(1)} ");

            }

            reader.Close();

            cnn.Close();


        }


        void update_data(string dbfile)
        {
            //string dbfile = @"URI=file:sql.db";
            SqliteConnection cnn = new SqliteConnection(dbfile);
            cnn.Open();

            string sql = "update  Person set Name='Jim jones' where id=1;";
            SqliteCommand cmd = new SqliteCommand(sql, cnn);
            cmd.ExecuteNonQuery();
            cnn.Close();

            Console.WriteLine("Update row OK");

        }


        void delete_data(string dbfile)
        {
            //string dbfile = @"URI=file:sql.db";
            SqliteConnection cnn = new SqliteConnection(dbfile);
            cnn.Open();

            string sql = "delete from  Person where id=3;";
            SqliteCommand cmd = new SqliteCommand(sql, cnn);
            cmd.ExecuteNonQuery();
            cnn.Close();
            Console.WriteLine("Delete row OK");

        }

    }
}