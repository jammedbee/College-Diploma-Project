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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ServiceCentreClientApp.Pages
{
    public sealed partial class RepairRequestPage : Page
    {
        SqlConnection connection;
        RepairRequest request;
        User currentUser, manager, engineer, client;
        ObservableCollection<User> managers, clients, engineers;
        ObservableCollection<Device> devices;
        ObservableCollection<RequestStatus> statuses;
        List<Manufacturer> manufacturers;

        public RepairRequestPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if ((e.Parameter as RepairRequestParameter).Request != null)
            {
                request = (e.Parameter as RepairRequestParameter).Request;
            }

            currentUser = (e.Parameter as RepairRequestParameter).CurrentUser;
            connection = (e.Parameter as RepairRequestParameter).Connection;

            if (request == null)
                await GetDataByNavigationReasonAsync(RequestAction.CreateNew);
            else
                await GetDataByNavigationReasonAsync(RequestAction.ModifyExisting);
            
            base.OnNavigatedTo(e);
        }

        private async Task FillDataAsync()
        {
            IdTextBlock.Text = request.Id.ToString();
            RegistrationDatePicker.SelectedDate = request.RegistrationDate.Date;
            RegistrationTimePicker.SelectedTime = request.RegistrationDate.TimeOfDay;
            UpdateDate.Text = (request.UpdateDate == null) ? "-" : request.UpdateDate.ToString();
            return;
        }

        private async Task<List<Manufacturer>> GetManufacturersAsync()
        {
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();

            List<Manufacturer> retrievedManufacturers = new List<Manufacturer>();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Manufacturer";
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                        while (await reader.ReadAsync())
                            retrievedManufacturers.Add(new Manufacturer
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1)
                            });
                }
            }

            connection.Close();
            return retrievedManufacturers;
        }

        private async Task<ObservableCollection<RequestStatus>> GetRequestStatusesAsync()
        {
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();

            ObservableCollection<RequestStatus> retrievedStatuses = new ObservableCollection<RequestStatus>();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM RequestStatus";

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                        while (await reader.ReadAsync())
                            retrievedStatuses.Add(new RequestStatus { Id = reader.GetInt32(0), Name = reader.GetString(1) });
                }
            }
            connection.Close();

            return retrievedStatuses;
        }

        private async Task<ObservableCollection<User>> GetUsersByTypeAsync(int typeId)
        {
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();

            ObservableCollection<User> retrievedUsers = new ObservableCollection<User>();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"SELECT * FROM User WHERE User.TypeId = {typeId}";

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                        while (await reader.ReadAsync())
                            retrievedUsers.Add(new User
                            {
                                Id = reader.GetInt32(0),
                                FirstName = reader.GetString(1),
                                LastName = reader.GetString(2),
                                Patronymic = reader.GetString(3),
                                Email = reader.GetString(4),
                                PhoneNumer = reader.GetString(5),
                                TypeId = reader.GetInt32(6),
                                AccountId = reader.GetInt32(7)
                            });
                }
            }
            connection.Close();

            return retrievedUsers;
        }

        private async Task<ObservableCollection<Device>> GetDevicesAsync()
        {
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();

            ObservableCollection<Device> retrievedDevices = new ObservableCollection<Device>();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Device WHERE";

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                        while (await reader.ReadAsync())
                            retrievedDevices.Add(new Device
                            {
                                Id = reader.GetInt64(0),
                                TypeId = reader.GetInt32(1),
                                Model = reader.GetString(2),
                                SerialNumber = reader.GetString(3),
                                ManufacturerId = reader.GetInt32(4),
                                ProblemDescription = reader.GetString(5)
                            });
                }
            }

            connection.Close();

            return retrievedDevices;
        }

        private async Task GetDataByNavigationReasonAsync(RequestAction action)
        {
            switch (action)
            {
                // to create new request
                case RequestAction.CreateNew:
                    engineers = await GetUsersByTypeAsync(4);
                    clients = await GetUsersByTypeAsync(7);
                    managers = await GetUsersByTypeAsync(3);
                    devices = await GetDevicesAsync();
                    statuses = await GetRequestStatusesAsync();
                    break;
                // to edit existing request
                case RequestAction.ModifyExisting:
                    if (currentUser.TypeId == 3)
                    {
                        engineers = await GetUsersByTypeAsync(3);
                        clients = await GetUsersByTypeAsync(7);
                        statuses = await GetRequestStatusesAsync();
                    }
                    else
                        if (currentUser.TypeId == 4)
                        {
                            statuses = await GetRequestStatusesAsync();
                        }
                        else
                            if (currentUser.TypeId == 6)
                            {
                                engineers = await GetUsersByTypeAsync(4);
                                clients = await GetUsersByTypeAsync(7);
                                managers = await GetUsersByTypeAsync(3);
                                statuses = await GetRequestStatusesAsync();
                            }
                    break;
            }
            return;
        }

        private async Task<string> GetManufacturerNameAsync(Device device)
        {
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Manufacturer.Name FROM Manufacturer JOIN Device ON Manufacturer.Id = Device.ManufacturerId";

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        if (await reader.IsDBNullAsync(0))
                        {
                            return "N/A";
                        }

                        return reader.GetString(0);
                    }
                    else
                    {
                        return "N/A";
                    }
                }
            }
        }

        private enum RequestAction : int
        {
            CreateNew = 0, ModifyExisting = 1
        }
    }
}
