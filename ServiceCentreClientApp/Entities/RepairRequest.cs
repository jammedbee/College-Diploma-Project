using System;

namespace ServiceCentreClientApp.Entities
{
    public class RepairRequest
    {
        public Int64 Id { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public Int64 DeviceId { get; set; }
        public int ClientId { get; set; }
        public int ManagerId { get; set; }
        public int EngineerId { get; set; }
        public int StatusId { get; set; }
        public decimal Price { get; set; }
        public bool IsUnderWarranty { get; set; }
    }
}
