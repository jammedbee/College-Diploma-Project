using System;
using System.Data.SqlClient;

namespace ServiceCentreClientApp.Entities
{
    public class Device
    {
        public Int64 Id { get; set; }
        public int TypeId { get; set; }
        public string Model { get; set; }
        public string SerialNumber { get; set; }
        public int ManufacturerId { get; set; }
        public string ProblemDescription { get; set; }
        public bool IsUnderWarranty { get; set; }

        public string GetDetailedInfo()
        {
            using (var connection = new SqlConnection((App.Current as App).ConnectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"SELECT [Name] FROM Manufacturer LEFT JOIN Device ON Manufacturer.Id = Device.ManufacturerId WHERE Device.Id = {Id}";
                    connection.Open();

                    return $"{Convert.ToString(command.ExecuteScalar())} {Model} ({SerialNumber})";
                }
            }
        }
    }
}
