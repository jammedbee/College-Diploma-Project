using System.Data.SqlClient;

namespace ServiceCentreClientApp.Parameters
{
    public class ConnectionParameter
    {
        public SqlConnection Connection { get; set; }

        public ConnectionParameter(SqlConnection connection) => Connection = connection;
    }
}
