using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace kasirrrrrrrrrrrrr
{
    public class DatabaseHelper
    {
        private string connectionstring = "Server=LocalHost;Database=grocerseeker;Uid = root;pwd=;";


        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionstring);
        }
    }
}