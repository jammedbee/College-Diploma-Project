namespace ServiceCentreClientApp.Entities
{
    public class RequestStatus
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public enum RequestStatusId : int
        {
            Accepted = 1, Waiting = 2, Processing = 3, Ready = 4, Archieved = 5
        }
    }
}
