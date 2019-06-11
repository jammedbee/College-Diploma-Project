using ServiceCentreClientApp.Entities;
using ServiceCentreClientApp.Parameters;
using System;
using System.Data.SqlClient;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ServiceCentreClientApp.Pages
{
    public sealed partial class DeviceTypePage : Page
    {
        SqlConnection connection;
        Device device;

        public DeviceTypePage()
        {
            this.InitializeComponent();

            TypeNameTextBox.Loaded += (object sender, RoutedEventArgs e) =>
            {
                SaveButton.IsEnabled = !string.IsNullOrWhiteSpace((sender as TextBox).Text);
            };
            TypeNameTextBox.TextChanging += (TextBox sender, TextBoxTextChangingEventArgs args) =>
            {
                SaveButton.IsEnabled = !string.IsNullOrWhiteSpace(sender.Text);
            };
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                connection = (e.Parameter as DeviceParameter).Connection;
                device = (e.Parameter as DeviceParameter).Device;

                if (device == null)
                {
                    EditButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    TypeNameTextBox.IsEnabled = false;
                    SaveButton.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Произошла следующая ошибка: \"{ex.Message}\"", "Что-то пошло не так :(").ShowAsync();
            }

            base.OnNavigatedTo(e);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EditButton.Visibility = Visibility.Collapsed;
            SaveButton.Visibility = Visibility.Visible;
            TypeNameTextBox.IsEnabled = true;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    if (device != null)
                    {
                        command.CommandText = $"UPDATE DeviceType SET [Name]=N'@newName' WHERE [Id]=@id";
                        command.Parameters.AddWithValue("@id", device.Id);
                        command.Parameters.Add(new SqlParameter("@newName", TypeNameTextBox.Text));
                    }
                    else
                    {
                        command.CommandText = "INSERT INTO DeviceType VALUES (@name)";
                        command.Parameters.AddWithValue("@name", TypeNameTextBox.Text);
                    }
                    await command.ExecuteNonQueryAsync();
                }

                CancelButton_Click(sender, e);
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Произошла следующая ошибка: \"{ex.Message}\"", "Что-то пошло не так :(").ShowAsync();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            (Parent as Frame).GoBack();
        }
    }
}
