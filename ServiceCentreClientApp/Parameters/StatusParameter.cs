using System.Data.SqlClient;
using ServiceCentreClientApp.Entities;

namespace ServiceCentreClientApp.Parameters
{
    public class StatusParameter : ConnectionParameter
    {
        public RequestStatus Status { get; set; }

        public StatusParameter(RequestStatus status, SqlConnection connection) : base(connection)
        {
            Status = status;
        }
    }
}
