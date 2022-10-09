using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySql
{
    public class MySqlMgr : Singleton<MySqlMgr>
    {
#if DEBUG
        private const string connectingStr = "server=localhost;port=3306;uid=root;pwd=123456;database=dong";
#else
        //对应服务器配置
        private const string connectingStr = "server=localhost;uid=root;pwd=;database=";
#endif

        public SqlSugarClient SqlSugarDB = null;
        public void Init() 
        {
            SqlSugarDB = new SqlSugarClient(
                new ConnectionConfig() 
                {
                    ConnectionString = connectingStr,
                    DbType = DbType.MySql,
                    IsAutoCloseConnection = true,
                    InitKeyType = InitKeyType.Attribute
                });

#if DEBUG
            //用来打印Sql方便你调式    
            SqlSugarDB.Aop.OnLogExecuting = (sql, pars) =>
            {
                Console.WriteLine(sql + "\r\n" +
                SqlSugarDB.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
                Console.WriteLine();
            };
#endif
        }
    }
}
