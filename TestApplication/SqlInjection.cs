using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApplication
{
    public class SqlInjection
    {
        public void TestMethod()
        {
            string command = Console.ReadLine();
            SqlCommand sqlCommand = new SqlCommand(command);
            sqlCommand.ExecuteNonQuery();
        }
    }
}
