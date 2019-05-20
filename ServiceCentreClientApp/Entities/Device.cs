using System;

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
    }
}
