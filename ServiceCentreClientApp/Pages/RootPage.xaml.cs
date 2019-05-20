using System.Data.SqlClient;
using Windows.UI.Xaml.Controls;

namespace ServiceCentreClientApp.Pages
{
    public sealed partial class RootPage : Page
    {
        SqlConnection connection;
        public RootPage()
        {
            connection = new SqlConnection((App.Current as App).ConnectionString);
            this.InitializeComponent();
            mainFrame.Navigate(typeof(LoginPage), connection);
        }
    }
}
