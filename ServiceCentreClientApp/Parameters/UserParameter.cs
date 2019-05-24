using System.Data.SqlClient;
using ServiceCentreClientApp.Entities;

namespace ServiceCentreClientApp.Parameters
{
    public class UserParameter : ConnectionParameter
    {
        public User CurrentUser{ get; set;}
        public Account CurrentAccount { get; set; }

        public UserParameter(User user, SqlConnection connection) : base(connection)
        {
            CurrentUser = user;
            CurrentAccount = null;
        }

        public UserParameter(User user, SqlConnection connection, Account account) : this(user, connection)
        {
            CurrentAccount = account;
        }
    }
}
