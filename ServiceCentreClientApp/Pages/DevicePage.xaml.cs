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
    public sealed partial class DevicePage : Page
    {
        SqlConnection connection;
        ObservableCollection<Manufacturer> manufacturers;
        ObservableCollection<DeviceType> deviceTypes;
        Device device;

        public DevicePage()
        {
            this.InitializeComponent();
            manufacturers = new ObservableCollection<Manufacturer>();
            deviceTypes = new ObservableCollection<DeviceType>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                connection = (e.Parameter as DeviceParameter).Connection;
                device = (e.Parameter as DeviceParameter).Device;
                await GetDeviceTypesAsync();
                await GetManufacturersAsync();

                if (device == null)
                {
                    EditButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    Tools.ControlsInteraction.DisableControls(this);
                    EditButton.IsEnabled = true;
                    SaveButton.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Произошла следующая ошибка: \"{ex.Message}\"", "Что-то пошло не так :(").ShowAsync();
            }
            base.OnNavigatedTo(e);
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    if (device == null)
                    {
                        command.CommandText = $"INSERT INTO [Device] VALUES ({(TypeComboBox.SelectedItem as DeviceType).Id}," +
                            $"N'{ModelTextBox.Text}',N'{SerialNumberTextBox.Text}', {(ManufacturerComboBox.SelectedItem as Manufacturer).Id}," +
                            $"N'{DescriptionRichTextBox.PlaceholderText}',{Convert.ToByte(WarrantyCheckBox.IsChecked)})";
                    }
                    else
                    {
                        command.CommandText = $"UPDATE [Device] SET TypeId={(TypeComboBox.SelectedItem as DeviceType).Id}," +
                            $"Model=N'{ModelTextBox.Text}',SerialNumber='{SerialNumberTextBox.Text}'," +
                            $"ManufacturerId={(ManufacturerComboBox.SelectedItem as DeviceType).Id}," +
                            $"ProblemDescription=N'{DescriptionRichTextBox.PlaceholderText}'," +
                            $"IsUnderWarranty={Convert.ToByte(WarrantyCheckBox.IsChecked)} " +
                            $"WHERE [Device].[Id] = {device.Id}";
                    }

                    await command.ExecuteNonQueryAsync();
                }

                connection.Close();
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Произошла следующая ошибка: \"{ex.Message}\"", "Что-то пошло не так :(").ShowAsync();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Tools.ControlsInteraction.EnableControls(this);
            SaveButton.Visibility = Visibility.Visible;
            EditButton.Visibility = Visibility.Collapsed;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            (Parent as Frame).GoBack();
        }

        private async Task GetDeviceTypesAsync()
        {
            try
            {
                if (deviceTypes.Count > 0)
                    deviceTypes.Clear();

                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM DeviceType";

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                            while (await reader.ReadAsync())
                                deviceTypes.Add(new DeviceType { Id = reader.GetInt32(0), Name = reader.GetString(1) });
                    }
                }

                connection.Close();
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Произошла следующая ошибка: \"{ex.Message}\"", "Что-то пошло не так :(").ShowAsync();
            }
        }

        private async Task GetManufacturersAsync()
        {
            try
            {
                if (manufacturers.Count > 0)
                    manufacturers.Clear();

                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM Manufacturer";

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                            while (await reader.ReadAsync())
                                manufacturers.Add(new Manufacturer { Id = reader.GetInt32(0), Name = reader.GetString(1) });
                    }
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Произошла следующая ошибка: \"{ex.Message}\"", "Что-то пошло не так :(").ShowAsync();
            }
        }

        private async void LoadDevice()
        {
            try
            {
                ModelTextBox.Text = device.Model;
                SerialNumberTextBox.Text = device.SerialNumber;
                DescriptionRichTextBox.PlaceholderText = device.ProblemDescription;
                WarrantyCheckBox.IsChecked = device.IsUnderWarranty;
                ManufacturerComboBox.SelectedItem = manufacturers.Where(m => m.Id == device.ManufacturerId).FirstOrDefault();
                TypeComboBox.SelectedItem = deviceTypes.Where(t => t.Id == device.TypeId);
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Произошла следующая ошибка: \"{ex.Message}\"", "Что-то пошло не так :(").ShowAsync();
            }
        }

        private void NewTypeButton_Click(object sender, RoutedEventArgs e)
        {
            (Parent as Frame).Navigate(typeof(DeviceTypePage), new DeviceParameter(null, connection));
        }

        private void NewManufacturerButton_Click(object sender, RoutedEventArgs e)
        {
            (Parent as Frame).Navigate(typeof(ManufacturerPage), new ManufacturerParameter(null, connection));
        }
    }
}
