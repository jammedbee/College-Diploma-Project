using System.Data.SqlClient;
using ServiceCentreClientApp.Entities;

namespace ServiceCentreClientApp.Parameters
{
    public class RepairRequestParameter : ConnectionParameter 
    {
        public RepairRequest Request { get; set; }
        public User CurrentUser { get; set; }

        public RepairRequestParameter(RepairRequest request, User user, SqlConnection connection) : base(connection)
        {
            Request = request;
            CurrentUser = user;
        }
    }
}
