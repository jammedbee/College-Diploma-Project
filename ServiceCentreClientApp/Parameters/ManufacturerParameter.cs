using System.Data.SqlClient;
using ServiceCentreClientApp.Entities;

namespace ServiceCentreClientApp.Parameters
{
    public class ManufacturerParameter : ConnectionParameter
    {
        public Manufacturer Manufacturer { get; set; }

        public ManufacturerParameter(Manufacturer manufacturer, SqlConnection connection) : base(connection)
        {
            Manufacturer = manufacturer;
        }
    }
}
