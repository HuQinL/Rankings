using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;
using System.Threading;

namespace Rankings
{
    class Program
    {
        //SELECT * FROM `rankings`ORDER BY liveCount DESC;
        //static string DB = "Server=123.232.102.241;Port=28112;Database=ent;Uid=yunrunmysql; Password=Yunrunmysql2016!@#;";
        static void Main(string[] args)
        {
            Thread thread = new Thread(new ThreadStart(delegate { GetWork(Config.DB); }));
            Thread thread1 = new Thread(new ThreadStart(delegate { GetWork(Config.CS); }));
            thread.Start();
            thread1.Start();
            Console.WriteLine("程序运行开始！！");
        }



        public static void GetWork(string DB)
        {
            List<Names> Stars = new List<Names>();
            Stars = TakeData(DB);
            foreach (Names star in Stars)
            {
                Count ct = new Count();
                ct = save(star.name, star.word, DB);
                UpdateRankings(star.name, ct.count, ct.num, DB);
            }
        }
        /// <summary>
        /// 存到数据库
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        public static void InsertRankings(string Name, int liveCount, int num, string cs)
        {
            try
            {
                //INSERT INTO work (Name,Type,Time,Director,Writers,Star) SELECT @Name,@Type,@Time,@Director,@Writers,@Star FROM DUAL WHERE NOT EXISTS (SELECT Name FROM work WHERE Name=@Name and Time=@Time)
                string sql = "insert into rankings(Name,liveCount,Count) Values(@Name,@liveCount,@Count)";
                MySqlParameter[] Parameter = new MySqlParameter[3];
                Parameter[0] = new MySqlParameter("@Name", Name);
                Parameter[1] = new MySqlParameter("@liveCount", liveCount);
                Parameter[2] = new MySqlParameter("@Count", num);
                bool res = Insert(sql, Parameter, cs);
                if (res)
                {
                    Console.WriteLine("成功添加：" + Name);
                }
            }
            catch { }
        }

        /// <summary>
        /// 存到数据库
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        public static void UpdateRankings(string Name, int liveCount, int num, string DB)
        {

            //INSERT INTO work (Name,Type,Time,Director,Writers,Star) SELECT @Name,@Type,@Time,@Director,@Writers,@Star FROM DUAL WHERE NOT EXISTS (SELECT Name FROM work WHERE Name=@Name and Time=@Time)
            string sql = "update rankings  SET liveCount=@liveCount,Count=@Count where Name =@Name";
            MySqlParameter[] Parameter = new MySqlParameter[3];
            Parameter[0] = new MySqlParameter("@Name", Name);
            Parameter[1] = new MySqlParameter("@liveCount", liveCount);
            Parameter[2] = new MySqlParameter("@Count", num);
            MySqlConnection sqlcon = new MySqlConnection(DB);
            try
            {
                sqlcon.Open();
                using (MySqlCommand comm = new MySqlCommand())
                {
                    comm.Connection = sqlcon;
                    comm.CommandText = sql;
                    comm.Parameters.Clear();
                    comm.Parameters.AddRange(Parameter);
                    int r = comm.ExecuteNonQuery();
                    if (r > 0)
                    {
                        Console.WriteLine("成功更新：" + Name);
                        return;
                    }
                    else
                    {
                        InsertRankings(Name, liveCount, num, DB);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                if (sqlcon != null)
                {
                    if (sqlcon.State != ConnectionState.Closed)
                    {
                        sqlcon.Close();
                    }
                }
            }

        }

        /// <summary>
        /// 判断是否存到数据
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static bool Insert(string sql, MySqlParameter[] parameter, string DB)
        {
            bool res = false;
            MySqlConnection sqlcon = new MySqlConnection(DB);
            // MySqlConnection sqlcon = new MySqlConnection(DB);
            try
            {
                sqlcon.Open();
                MySqlCommand sqlcom = new MySqlCommand(sql, sqlcon);
                sqlcom.Parameters.AddRange(parameter);
                int a = sqlcom.ExecuteNonQuery();
                if (a > 0)
                {
                    res = true;
                }
                sqlcon.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                sqlcon.Close();
            }
            return res;
        }


        /// <summary>
        /// 从数据库拿内容
        /// </summary>
        /// <returns></returns>
        public static Count save(string name, string word, string DB)
        {
            int count = 0;
            int i = 0;
            Count num = new Count();
            using (MySqlConnection conn = new MySqlConnection(DB))
            {
                conn.Open();//打开数据库  
                MySqlCommand cmd = conn.CreateCommand();
                string sql = "";
                string[] words;
                int aa;
                if (string.IsNullOrEmpty(word))
                {
                    sql = "SELECT * FROM ent.livevideoinf WHERE title LIKE '%{0}%' ";
                    cmd.CommandText = string.Format(sql, name);
                }
                else
                {
                    words = word.Split('|');
                    aa = words.Count();
                    if (aa == 3)
                    {
                        sql = "SELECT * FROM ent.livevideoinf WHERE title LIKE '%{0}%' OR title LIKE '%{1}%'OR title LIKE '%{2}%'OR title LIKE '%{3}%'";
                        cmd.CommandText = string.Format(sql, name, words[0], words[1], words[2]);
                    }
                    else if (aa == 2)
                    {
                        sql = "SELECT * FROM ent.livevideoinf WHERE title LIKE '%{0}%' OR title LIKE '%{1}%'OR title LIKE '%{2}%'";
                        cmd.CommandText = string.Format(sql, name, words[0], words[1]);
                    }
                    else if (aa == 1)
                    {
                        sql = "SELECT * FROM ent.livevideoinf WHERE title LIKE '%{0}%' OR title LIKE '%{1}%'";
                        cmd.CommandText = string.Format(sql, name, words[0]);
                    }
                    else
                    {
                        sql = "SELECT * FROM ent.livevideoinf WHERE title LIKE '%{0}%' ";
                        cmd.CommandText = string.Format(sql, name);
                    }
                }

                //创建数据库命令  

                //创建查询语句  SELECT * FROM ent.livevideoinf WHERE title LIKE'%范冰冰%'
                // string sql = "SELECT * FROM ent.livevideoinf WHERE title LIKE '%{0}%' OR title LIKE '%{1}%'";

                //cmd.CommandText = "SELECT id, NAME,Content,IsComplete  FROM news.check  WHERE id =137785";
                //从数据库中读取数据流存入reader中  
                MySqlDataReader reader = cmd.ExecuteReader();

                //从reader中读取下一行数据,如果没有数据,reader.Read()返回flase  
                while (reader.Read())
                {

                    //reader.GetOrdinal("id")是得到ID所在列的index,  
                    //reader.GetInt32(int n)这是将第n列的数据以Int32的格式返回  
                    //reader.GetString(int n)这是将第n列的数据以string 格式返回  
                    //user.id = reader.GetInt32(reader.GetOrdinal("ID"));
                    // user.name = reader["Name"] is DBNull ? "" : (string)reader["Name"];
                    num.count = reader.GetInt32(reader.GetOrdinal("liveCount"));
                    i++;
                    //统计总人数
                    count += num.count;
                    //格式输出数据  
                    //Console.Write("ID:{0},name:{1},liveCount:{2}\n", user.id, user.name, user.liveCount);
                }
                num.count = count;
                num.num = i;
            }

            return num;
        }

        /// <summary>
        /// 从数据库拿内容
        /// </summary>
        /// <returns></returns>
        public static List<Names> TakeData(string DB)
        {
            using (MySqlConnection conn = new MySqlConnection(DB))
            {
                conn.Open();//打开数据库  
                List<Names> Stars = new List<Names>();


                //创建数据库命令  
                MySqlCommand cmd = conn.CreateCommand();
                //创建查询语句  SELECT * FROM ent.livevideoinf WHERE title LIKE'%范冰冰%'
                string sql = "SELECT NAME,Word FROM ent.star";
                cmd.CommandText = sql;
                //cmd.CommandText = "SELECT id, NAME,Content,IsComplete  FROM news.check  WHERE id =137785";
                //从数据库中读取数据流存入reader中  
                MySqlDataReader reader = cmd.ExecuteReader();

                //从reader中读取下一行数据,如果没有数据,reader.Read()返回flase  
                while (reader.Read())
                {
                    Names star = new Names();
                    //reader.GetOrdinal("id")是得到ID所在列的index,  
                    //reader.GetInt32(int n)这是将第n列的数据以Int32的格式返回  
                    //reader.GetString(int n)这是将第n列的数据以string 格式返回  
                    // user.id = reader.GetInt32(reader.GetOrdinal("ID"));
                    star.name = reader["Name"] is DBNull ? "" : (string)reader["Name"];
                    string word = reader["Word"] is DBNull ? "" : (string)reader["Word"];
                    star.word = word.Replace("(", "").Replace(")", "");
                    //统计总人数

                    Stars.Add(star);
                    //格式输出数据  
                    Console.Write("name:{0},word:{1}\n", star.name, star.word);
                }
                return Stars;
            }

        }
    }
}
