namespace ServiceCentreClientApp.Entities
{
    public class UserType
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public enum UserTypeId : int
        {
            Developer = 1, SystemAdmin = 2,
            Manager = 3, Engineer = 4,
            HR = 5, Director = 6, Client = 7
        }
    }
}
