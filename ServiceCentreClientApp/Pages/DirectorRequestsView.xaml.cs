using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class DirectorRequestsView : Page
    {
        SqlConnection connection;
        ObservableCollection<RepairRequest> requests;
        User user;

        public DirectorRequestsView()
        {
            this.InitializeComponent();
            requests = new ObservableCollection<RepairRequest>();

        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            user = (e.Parameter as UserParameter).CurrentUser;

            connection = (e.Parameter as UserParameter).Connection;

            await GetRequestsAsync(user);

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
                this.Progress.IsActive = true;
                if (requests.Count > 0)
                {
                    requests = new ObservableCollection<RepairRequest>();
                }

                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                var command = connection.CreateCommand();
                command.CommandText = $"SELECT * FROM RepairRequest";

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
                    }
                    else
                    {
                        await new MessageDialog("Не было найдено ни одной заявки.").ShowAsync();
                    }
                }

            }

            catch (Exception ex)
            {
                await new MessageDialog($"Произошла следующая ошибка: \"{ex.Message}\"", "Что-то пошло не так :(").ShowAsync();
            }
            Progress.IsActive = false;
        }

        private void RequestsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            (Parent as Frame).Navigate(typeof(RepairRequestPage), new RepairRequestParameter(
                e.ClickedItem as RepairRequest, user, connection));
        }

        private async void RefreshButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await GetRequestsAsync(user);
        }

    }
}
