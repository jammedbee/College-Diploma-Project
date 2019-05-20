using System.Data.SqlClient;
using ServiceCentreClientApp.Entities;

namespace ServiceCentreClientApp.Parameters
{
    public class UserParameter : ConnectionParameter
    {
        public User CurrentUser{ get; set;}

        public UserParameter(User user, SqlConnection connection) : base(connection)
        {
            CurrentUser = user;
        }
    }
}
