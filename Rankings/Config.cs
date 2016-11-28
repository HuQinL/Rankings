using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Rankings
{
    class Config
    {
        /// <summary>
        /// 正式数据库的位置
        /// </summary>
        public static readonly string DB;
        /// <summary>
        /// 备选数据库的位置
        /// </summary>
        public static readonly string CS;

        static Config()
        {
            DB = ConfigurationManager.AppSettings["MyDatabase"];
            CS = ConfigurationManager.AppSettings["KeywordsDatabase"];
        }
    }
}
