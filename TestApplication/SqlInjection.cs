using System;
using System.Data.SqlClient;

namespace TestApplication
{
    public class SqlInjection
    {
        public void TestMethod()
        {
            string command = Console.ReadLine();
            SqlCommand sqlCommand = new SqlCommand("locally built command");
            sqlCommand.ExecuteNonQuery();
        }
    }
}
