using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using ServiceCentreClientApp.Entities;
using ServiceCentreClientApp.Pages;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ServiceCentreClientApp
{
    public sealed partial class MainPage : Page
    {
        private SqlConnection connection;
        private User user;

        public MainPage()
        {
            this.InitializeComponent();
            connection = new SqlConnection((App.Current as App).ConnectionString);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            user = e.Parameter as User;
            await HideNavigationItems(user);
            base.OnNavigatedTo(e);
        }

        public async Task HideNavigationItems(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            try
            {

                switch (user.TypeId)
                {
                    case 1:
                        foreach (var item in menu.MenuItems)
                        {
                            (item as NavigationViewItem).Visibility = Windows.UI.Xaml.Visibility.Visible;
                        }
                        mainFrame.Navigate(typeof(EngineerPage), user);
                        break;

                    //case 2:
                    //    (Parent as Frame).Navigate(typeof(), userParameter);
                    //    break;

                    case 3:
                        mainFrame.Navigate(typeof(ManagerPage), user);
                        break;

                    case 4:
                        mainFrame.Navigate(typeof(EngineerPage), user);
                        break;

                    case 5:
                        mainFrame.Navigate(typeof(HRPage), user);
                        break;

                    case 6:
                        mainFrame.Navigate(typeof(DirectorPage), user);
                        break;

                    case 7:
                        mainFrame.Navigate(typeof(ClientPage), user);
                        break;

                    default:
                        throw new Exception("Произошла ошибка во время определения пользователя. Обратитесь к системному администратору.");
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Произошла следующая ошибка: \"{ex.Message}\"", "Что-то пошло не так :(").ShowAsync();
            }
        }

        private void DirectorView_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            mainFrame.Navigate(typeof(DirectorPage), user);
        }

        private void HRView_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            mainFrame.Navigate(typeof(HRPage), user);
        }

        private void ManagerView_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            mainFrame.Navigate(typeof(ManagerPage), user);
        }

        private void EngineerView_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            mainFrame.Navigate(typeof(EngineerPage), user);
        }

        private void ClientView_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            mainFrame.Navigate(typeof(ClientPage), user);
        }

        private void Menu_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            menu.IsBackEnabled = mainFrame.CanGoBack;
        }

        private void Menu_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            mainFrame.GoBack();
            menu.IsBackEnabled = mainFrame.CanGoBack;
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            menu.IsBackEnabled = mainFrame.CanGoBack;
        }
    }
}
