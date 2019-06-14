using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ServiceCentreClientApp.Entities;
using ServiceCentreClientApp.Parameters;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ServiceCentreClientApp.Pages
{
    public sealed partial class EngineerPage : Page
    {
        SqlConnection connection;
        ObservableCollection<RepairRequest> requests;
        User currentUser;


        public EngineerPage()
        {
            InitializeComponent();
            requests = new ObservableCollection<RepairRequest>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                currentUser = e.Parameter as User;
                connection = new SqlConnection((App.Current as App).ConnectionString);

                await GetRequestsAsync(currentUser);

                if (RequestsGridView.ItemsSource == null)
                {
                    WellDoneTextBlock.Visibility = Visibility.Visible;
                }
                else
                {
                    WellDoneTextBlock.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Произошла следующая ошибка: \"{ex.Message}\"", "Что-то пошло не так :(").ShowAsync();
            }

            base.OnNavigatedTo(e);
        }

        private async Task GetRequestsAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            try
            {
                ProgressProgressRing.IsActive = true;
                if (requests.Count > 0)
                {
                    requests = new ObservableCollection<RepairRequest>();
                }

                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                var command = connection.CreateCommand();
                command.CommandText = $"SELECT * FROM RepairRequest WHERE EngineerId = {user.Id} WHERE StatusId NOT IN (4,5)";

                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            DateTime? updateDate;
                            if (await reader.IsDBNullAsync(2))
                            {
                                updateDate = null;
                            }
                            else
                            {
                                updateDate = reader.GetDateTime(2);
                            }

                            requests.Add(new RepairRequest
                            {
                                Id = reader.GetInt64(0),
                                RegistrationDate = reader.GetDateTime(1),
                                UpdateDate = updateDate,
                                DeviceId = reader.GetInt64(3),
                                ClientId = reader.GetInt32(4),
                                ManagerId = reader.GetInt32(5),
                                EngineerId = reader.GetInt32(6),
                                StatusId = reader.GetInt32(7),
                                IsUnderWarranty = reader.GetBoolean(8),
                                Price = reader.GetDecimal(9)
                            });
                        }

                        var reqs = requests.Where(r => r.StatusId != 4 && r.StatusId != 5);
                        requests = new ObservableCollection<RepairRequest>(reqs);
                        reqs = null;

                        if (requests.Count == 0)
                        {
                            RequestsGridView.ItemsSource = null;

                            await new MessageDialog("Не было найдено ни одной активной заявки на Ваше имя.").ShowAsync();
                        }
                        else
                        {
                            RequestsGridView.ItemsSource = requests;
                        }
                    }
                    else
                    {
                        await new MessageDialog("Не было найдено ни одной заявки на Ваше имя.").ShowAsync();
                    }
                }

            }

            catch (Exception ex)
            {
                await new MessageDialog($"Произошла следующая ошибка: \"{ex.Message}\"", "Что-то пошло не так :(").ShowAsync();
            }
            ProgressProgressRing.IsActive = false;
        }

        private void RequestsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            (Parent as Frame).Navigate(typeof(RepairRequestPage), new RepairRequestParameter(
                e.ClickedItem as RepairRequest, currentUser, connection));
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await GetRequestsAsync(currentUser);
        }
    }
}
