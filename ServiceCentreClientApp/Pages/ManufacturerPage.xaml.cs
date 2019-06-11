using ServiceCentreClientApp.Entities;
using ServiceCentreClientApp.Parameters;
using System;
using System.Data.SqlClient;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ServiceCentreClientApp.Pages
{
    public sealed partial class ManufacturerPage : Page
    {
        SqlConnection connection;
        Manufacturer manufacturer;

        public ManufacturerPage()
        {
            this.InitializeComponent();

            NameTextBox.Loaded += (object sender, RoutedEventArgs e) =>
            {
                SaveButton.IsEnabled = !string.IsNullOrWhiteSpace((sender as TextBox).Text);
            };
            NameTextBox.TextChanging += (TextBox sender, TextBoxTextChangingEventArgs args) =>
            {
                SaveButton.IsEnabled = !string.IsNullOrWhiteSpace(sender.Text);
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            connection = (e.Parameter as ManufacturerParameter).Connection;
            manufacturer = (e.Parameter as ManufacturerParameter).Manufacturer;

            if (manufacturer == null)
            {
                EditButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                NameTextBox.IsEnabled = false;
                SaveButton.Visibility = Visibility.Collapsed;
            }

            base.OnNavigatedTo(e);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EditButton.Visibility = Visibility.Collapsed;
            SaveButton.Visibility = Visibility.Visible;
            NameTextBox.IsEnabled = true;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                if (manufacturer != null)
                {
                    command.CommandText = $"UPDATE Manufacturer SET [Name]='@newName' WHERE [Id]=@id";
                    command.Parameters.AddWithValue("@id", manufacturer.Id);
                    command.Parameters.Add(new SqlParameter("@newName", NameTextBox.Text));
                }
                else
                {
                    command.CommandText = "INSERT INTO Manufacturer VALUES (@name)";
                    command.Parameters.AddWithValue("@name", NameTextBox.Text);
                }
                await command.ExecuteNonQueryAsync();
            }

            CancelButton_Click(sender, e);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            (Parent as Frame).GoBack();
        }
    }
}
