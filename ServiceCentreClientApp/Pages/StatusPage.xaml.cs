using System;
using System.Data.SqlClient;
using ServiceCentreClientApp.Entities;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ServiceCentreClientApp.Parameters
{
    public sealed partial class StatusPage : Page
    {
        SqlConnection connection;
        RequestStatus status;

        public StatusPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            connection = (e.Parameter as StatusParameter).Connection;
            status = (e.Parameter as StatusParameter).Status;

            if (status == null)
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
                if (status != null)
                {
                    command.CommandText = $"UPDATE RequestStatus SET [Name]=N'@newName' WHERE [Id]=@id";
                    command.Parameters.AddWithValue("@id", status.Id);
                    command.Parameters.Add(new SqlParameter("@newName", NameTextBox.Text));
                }
                else
                {
                    command.CommandText = "INSERT INTO RequestStatus VALUES (@name)";
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
