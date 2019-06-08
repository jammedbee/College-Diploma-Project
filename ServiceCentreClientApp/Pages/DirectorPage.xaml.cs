using ServiceCentreClientApp.Entities;
using ServiceCentreClientApp.Parameters;
using System.Data.SqlClient;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ServiceCentreClientApp.Pages
{
    public sealed partial class DirectorPage : Page
    {
        User currentUser;
        SqlConnection connection;

        public DirectorPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            currentUser = (e.Parameter as User);
            connection = new SqlConnection((App.Current as App).ConnectionString);

            base.OnNavigatedTo(e);
        }

        private void PersonnelViewButton_Click(object sender, RoutedEventArgs e)
        {
            (Parent as Frame).Navigate(typeof(HRPage), currentUser);
        }

        private void RequestsViewButton_Click(object sender, RoutedEventArgs e)
        {
            (Parent as Frame).Navigate(
                typeof(DirectorRequestsView), 
                new UserParameter(currentUser, connection));
        }
    }
}
