using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using TestUserApi.Models;

namespace TestUserApi.Controllers
{
    public class UserController : ApiController
    {
        public User GetUser()
        {
            string username = HttpContext.Current.User.Identity.Name;
            string query = "SELECT * FROM Users WHERE Username = '" + username + "'";
            SqlConnection sqlConnection = new SqlConnection("(local)");
            sqlConnection.Open();
            SqlCommand sqlCommand = sqlConnection.CreateCommand();
            sqlCommand.CommandText = query;
            SqlDataReader reader = sqlCommand.ExecuteReader();
            reader.Read();
            return new User
            {
                FirstName = (string)reader["FirstName"],
                LastName = (string)reader["LastName"]
            };
        }
    }
}
