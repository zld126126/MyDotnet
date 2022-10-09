using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace MySql.MySQLData
{
    [SugarTable("user")]
    public class User
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime Logindate { get; set; }
        public string Logintype { get; set; }
        public string Token { get; set; }
    }
}
