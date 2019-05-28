namespace ServiceCentreClientApp.Entities
{
    public class DeviceType
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string GetName()
        {
            return this.Name;
        }
    }
}
