using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ServiceCentreClientApp.Pages;
using ServiceCentreClientApp.Parameters;
using ServiceCentreClientApp.Entities;
using System.Data.SqlClient;

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
            (Parent as Frame).Navigate(typeof(DirectorRequestsView), new UserParameter(currentUser, connection));
        }
    }
}
