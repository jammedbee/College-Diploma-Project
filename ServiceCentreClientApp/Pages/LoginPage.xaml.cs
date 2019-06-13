using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Data.SqlClient;
using ServiceCentreClientApp.Entities;
using ServiceCentreClientApp.Tools;
using Windows.UI.Popups;
using Windows.UI.Xaml.Navigation;

namespace ServiceCentreClientApp.Pages
{
    public sealed partial class LoginPage : Page
    {
        User currentUser;
        SqlConnection connection;

        public LoginPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            connection = e.Parameter as SqlConnection;

            base.OnNavigatedTo(e);
        }

        private async void LogInButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            ControlsInteraction.DisableControls(this);
            var user = new User();
            Progress.IsActive = true;
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();

                if (connection.State == System.Data.ConnectionState.Open)
                {
                    using (var command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "usp_Autorization";
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(
                            new SqlParameter("@login", loginTextBox.Text));
                        command.Parameters.Add(
                            new SqlParameter("@password", passwordPasswordBox.Password));
                        command.Parameters.Add(
                            new SqlParameter("@accountId", -1))
                            .Direction = System.Data.ParameterDirection.Output;
                        command.Parameters.Add(
                            new SqlParameter("@success", 0))
                            .Direction = System.Data.ParameterDirection.Output;

                        await command.ExecuteNonQueryAsync();
                        // если авторизация прошла успешно, то загружаем данные текущего пользователя
                        // с помощью процедуры usp_GetUser
                        if (Convert.ToInt32(command.Parameters["@success"].Value) == 1)
                        {
                            user.AccountId = Convert.ToInt32(command.Parameters["@accountId"].Value);

                            command.Parameters.Clear();
                            command.CommandText = "usp_GetUser";
                            command.Parameters.Add(
                                new SqlParameter("@accountId", user.AccountId));
                            command.Parameters.Add(
                                new SqlParameter("@id", System.Data.SqlDbType.Int))
                                .Direction = System.Data.ParameterDirection.Output;
                            command.Parameters.Add(
                                new SqlParameter("@firstName", System.Data.SqlDbType.NVarChar, 60))
                                .Direction = System.Data.ParameterDirection.Output;
                            command.Parameters.Add(
                                new SqlParameter("@lastName", System.Data.SqlDbType.NVarChar, 60))
                                .Direction = System.Data.ParameterDirection.Output;
                            command.Parameters.Add(
                                new SqlParameter("@patronymic", System.Data.SqlDbType.NVarChar, 60))
                                .Direction = System.Data.ParameterDirection.Output;
                            command.Parameters.Add(
                                new SqlParameter("@birthDate", System.Data.SqlDbType.DateTime2))
                                .Direction = System.Data.ParameterDirection.Output;
                            command.Parameters.Add(
                                new SqlParameter("@passportNumber", System.Data.SqlDbType.NVarChar, 60))
                                .Direction = System.Data.ParameterDirection.Output;
                            command.Parameters.Add(
                                new SqlParameter("@email", System.Data.SqlDbType.NVarChar, 512))
                                .Direction = System.Data.ParameterDirection.Output;
                            command.Parameters.Add(
                                new SqlParameter("@phoneNumber", System.Data.SqlDbType.NVarChar, 30))
                                .Direction = System.Data.ParameterDirection.Output;
                            command.Parameters.Add(
                                new SqlParameter("@typeId", System.Data.SqlDbType.Int))
                                .Direction = System.Data.ParameterDirection.Output;
                            command.Parameters.Add(
                                new SqlParameter("@photo", System.Data.SqlDbType.VarBinary, -1))
                                .Direction = System.Data.ParameterDirection.Output;

                            await command.ExecuteNonQueryAsync();

                            user.FirstName = (string)command.Parameters["@firstName"].Value;
                            user.LastName = (string)command.Parameters["@lastName"].Value;
                            user.Patronymic = (string)command.Parameters["@patronymic"].Value;
                            user.PassportNumber = (string)command.Parameters["@passportNumber"].Value;
                            user.Email = (string)command.Parameters["@email"].Value;
                            user.BirthDate = Convert.ToDateTime(command.Parameters["@birthDate"].Value);
                            user.PhoneNumber = (string)command.Parameters["@phoneNumber"].Value;
                            user.TypeId = Convert.ToInt32(command.Parameters["@typeId"].Value);
                            user.Photo = new Windows.UI.Xaml.Media.Imaging.BitmapImage();
                            user.Id = Convert.ToInt32(command.Parameters["@id"].Value);

                            currentUser = user;
                        }
                        else
                        {
                            ControlsInteraction.EnableControls(this);
                            throw new Exception("Ошибка авторизации. Проверьте правильность введённых данных и попробуйте снова.");
                        }
                    }

                }
                connection.Close();
                (Parent as Frame).Navigate(typeof(MainPage), currentUser);
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Произошла следующая ошибка: \"{ex.Message}\"", "Что-то пошло не так :(")
                    .ShowAsync();
            }
            Progress.IsActive = false;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Exit();
        }
    }
}
