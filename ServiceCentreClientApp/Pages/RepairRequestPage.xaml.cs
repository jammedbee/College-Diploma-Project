using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ServiceCentreClientApp.Entities;
using ServiceCentreClientApp.Parameters;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ServiceCentreClientApp.Tools;

namespace ServiceCentreClientApp.Pages
{
    public sealed partial class RepairRequestPage : Page
    {
        SqlConnection connection;
        RepairRequest request;
        User currentUser;
        ObservableCollection<User> managers, clients, engineers;
        ObservableCollection<Device> devices;
        ObservableCollection<RequestStatus> statuses;

        public RepairRequestPage()
        {
            this.InitializeComponent();

            devices = new ObservableCollection<Device>();
            managers = new ObservableCollection<User>();
            clients = new ObservableCollection<User>();
            engineers = new ObservableCollection<User>();
            statuses = new ObservableCollection<RequestStatus>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if ((e.Parameter as RepairRequestParameter).Request != null)
            {
                request = (e.Parameter as RepairRequestParameter).Request;
            }

            currentUser = (e.Parameter as RepairRequestParameter).CurrentUser;

            if ((currentUser.TypeId == 3) || (currentUser.TypeId == 6))
            {
                EditButton.Visibility = Visibility.Visible;
            }
            else
            {
                EditButton.Visibility = Visibility.Collapsed;
            }
            connection = (e.Parameter as RepairRequestParameter).Connection;

            await LoadDataAsync();

            if (request == null)
            {
                PriceTextBox.Text = "0";
                SaveButton.Visibility = Visibility.Collapsed;
                EditButton.Visibility = Visibility.Collapsed;
                ManagerComboBox.SelectedItem = managers.FirstOrDefault(m => m.Id == currentUser.Id);
                ManagerComboBox.IsEnabled = false;
            }
            else
            {
                ControlsInteraction.DisableControls(this);
                EditButton.IsEnabled = true;
                CancelButton.IsEnabled = true;
                SetCurrentValues(request);
                SaveButton.Visibility = Visibility.Collapsed;
                AddButton.Visibility = Visibility.Collapsed;
            }

            base.OnNavigatedTo(e);
        }

        private void SetCurrentValues(RepairRequest request)
        {
            DeviceComboBox.SelectedItem = devices.FirstOrDefault(d => d.Id == request.DeviceId);
            ManagerComboBox.SelectedItem = managers.FirstOrDefault(m => m.Id == request.ManagerId);
            EngineerComboBox.SelectedItem = engineers.FirstOrDefault(e => e.Id == request.EngineerId);
            ClientComboBox.SelectedItem = clients.FirstOrDefault(c => c.Id == request.ClientId);
            StatusComboBox.SelectedItem = statuses.FirstOrDefault(s => s.Id == request.StatusId);
            IdTextBlock.Text = request.Id.ToString();
            RegistrationDate.Text = request.RegistrationDate.ToString();
            UpdateDate.Text = request.UpdateDate.ToString();
            PriceTextBox.Text = request.Price.ToString();
        }

        private async Task LoadDataAsync()
        {
            devices = await GetDevicesAsync();
            statuses = await GetRequestStatusesAsync();
            managers = await GetUsersByTypeAsync(3);
            engineers = await GetUsersByTypeAsync(4);
            clients = await GetUsersByTypeAsync(7);

            StatusComboBox.ItemsSource = statuses;
            ManagerComboBox.ItemsSource = managers;
            EngineerComboBox.ItemsSource = engineers;
            ClientComboBox.ItemsSource = clients;
            return;
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
                command.CommandText = $"SELECT * FROM [User] WHERE [User].TypeId = {typeId}";

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
                command.CommandText = "SELECT * FROM Device";

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

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            Progress.IsActive = true;
            ControlsInteraction.DisableControls(this);

            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"INSERT INTO RepairRequest VALUES " +
                    $"('{DateTime.Now.Year.ToString() + '-' + DateTime.Now.Month.ToString() + '-' + DateTime.Now.Day.ToString() + ' ' + DateTime.Now.TimeOfDay.ToString()}'," +
                    $" null, {(DeviceComboBox.SelectedItem as Device).Id}, {(ClientComboBox.SelectedItem as User).Id}," +
                    $"{(ManagerComboBox.SelectedItem as User).Id}, {(EngineerComboBox.SelectedItem as User).Id}, " +
                    $"{(StatusComboBox.SelectedItem as RequestStatus).Id}, {PriceTextBox.Text})";

                await command.ExecuteNonQueryAsync();
            }

            connection.Close();
            ControlsInteraction.EnableControls(this);
            Progress.IsActive = false;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Progress.IsActive = true;
            ControlsInteraction.DisableControls(this);

            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"UPDATE RepairRequest SET " +
                    $"ManagerId = {(ManagerComboBox.SelectedItem as User).Id}," +
                    $"EngineerId = {(EngineerComboBox.SelectedItem as User).Id}," +
                    $"StatusId = {(StatusComboBox.SelectedItem as RequestStatus).Id}," +
                    $"Price = {PriceTextBox.Text}" +
                    $"WHERE Id = {request.Id}";

                await command.ExecuteNonQueryAsync();
            }

            connection.Close();
            ControlsInteraction.EnableControls(this);
            Progress.IsActive = false;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            request = null;
            (Parent as Frame).GoBack();
        }

        private void NewClient_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NewDevice_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            ControlsInteraction.EnableControls(this);
            EditButton.Visibility = Visibility.Collapsed;
            SaveButton.Visibility = Visibility.Visible;
        }

        private enum RequestAction : int
        {
            CreateNew = 0, ModifyExisting = 1
        }
    }
}
