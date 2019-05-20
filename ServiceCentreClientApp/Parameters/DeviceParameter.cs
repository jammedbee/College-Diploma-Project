using System.Data.SqlClient;
using ServiceCentreClientApp.Entities;

namespace ServiceCentreClientApp.Parameters
{
    class DeviceParameter : ConnectionParameter
    {
        public Device Device { get; set; }

        public DeviceParameter(Device device, SqlConnection connection) : base(connection)
        {
            this.Device = device;
        }
    }
}
