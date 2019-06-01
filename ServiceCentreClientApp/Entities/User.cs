using System;
using Windows.UI.Xaml.Media.Imaging;

namespace ServiceCentreClientApp.Entities
{
    public class User : Specs
    {
        public int AccountId { get; set; }
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string PassportNumber { get; set; }
        public string LastName { get; set; }
        public string Patronymic { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public BitmapImage Photo { get; set; }
        public DateTime BirthDate { get; set; }
        public int TypeId { get; set; }
        public bool IsDeleted { get; set; }

        public string GetFullName { get
            {
                return LastName + " " + FirstName + " " + Patronymic;
            } }
        public string GetBirthDate { get
            {
                return BirthDate.Day.ToString() + "/" + BirthDate.Month.ToString() + "/" + BirthDate.Year.ToString();
            } }
    }
}
