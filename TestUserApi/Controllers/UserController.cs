using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Http;
using TestUserApi.Models;

namespace TestUserApi.Controllers
{
    public class UserController : ApiController
    {
        public async Task<User> GetUser(string username)
        {
            SqlCommand sqlCommand = new SqlCommand($"SELECT * FROM Users WHERE Username = '{username}'");
            SqlDataReader reader = await sqlCommand.ExecuteReaderAsync();
            return new User
            {
                FirstName = (string)reader["FirstName"],
                LastName = (string)reader["LastName"]
            };
        }
    }
}
