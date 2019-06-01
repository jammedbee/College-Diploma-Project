using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ServiceCentreClientApp.Entities;
using ServiceCentreClientApp.Parameters;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ServiceCentreClientApp.Pages
{
    public sealed partial class ManagerPage : Page
    {
        SqlConnection connection;
        ObservableCollection<RepairRequest> requests;
        User currentUser;
        ObservableCollection<RequestStatus> statuses;

        public ManagerPage()
        {
            this.InitializeComponent();
            requests = new ObservableCollection<RepairRequest>();
        }

        private async Task<ObservableCollection<RequestStatus>> GetStatusesAsync()
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();

                var retrievedStatuses = new ObservableCollection<RequestStatus>();
                retrievedStatuses.Add(new RequestStatus { Id = 0, Name = "Не выбрано" });

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM RequestStatus";

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                retrievedStatuses.Add(new RequestStatus { Id = reader.GetInt32(0), Name = reader.GetString(1) });
                            }
                        }
                    }
                }

                connection.Close();
                return retrievedStatuses;
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Произошла следующая ошибка: \"{ex.Message}\"", "Что-то пошло не так :(").ShowAsync();
                return null;
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                currentUser = e.Parameter as User;

                connection = new SqlConnection((App.Current as App).ConnectionString);

                await GetRequestsAsync(currentUser);
                statuses = await GetStatusesAsync();
                FilterComboBox.ItemsSource = statuses;
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
                command.CommandText = $"SELECT * FROM RepairRequest WHERE ManagerId = {user.Id}";

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
                        connection.Close();
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

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            (Parent as Frame).Navigate(typeof(RepairRequestPage), new RepairRequestParameter(
                null, currentUser, connection));
        }

        private void ResetFilterButton_Click(object sender, RoutedEventArgs e)
        {
            RequestsGridView.ItemsSource = requests;
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((FilterComboBox.SelectedItem as RequestStatus).Id != 0)
            {
                var filteredRequest = requests.Where(r => r.StatusId == (FilterComboBox.SelectedItem as RequestStatus).Id);
                RequestsGridView.ItemsSource = filteredRequest;
            }
        }

        private void SearchSearchBox_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            var regex = new Regex(args.QueryText);

            var searched = requests.Where(r => regex.IsMatch(r.Id.ToString()) || regex.IsMatch(r.RegistrationDate.ToString()));

            RequestsGridView.ItemsSource = searched;
        }
    }
}
