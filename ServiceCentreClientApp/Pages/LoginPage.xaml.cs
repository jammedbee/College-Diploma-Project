using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Data.SqlClient;
using ServiceCentreClientApp.Entities;
using ServiceCentreClientApp.Parameters;
using Windows.UI.Popups;
using Windows.UI.Xaml.Navigation;

namespace ServiceCentreClientApp.Pages
{
    public sealed partial class LoginPage : Page
    {
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

        //private async void NavigateByUserType(UserParameter userParameter)
        //{
        //    try
        //    {

        //        switch (userParameter.User.TypeId)
        //        {
        //            case 1:
        //                (Parent as Frame).Navigate(typeof(MainPage), userParameter);
        //                break;

        //            //case 2:
        //            //    (Parent as Frame).Navigate(typeof(), userParameter);
        //            //    break;

        //            case 3:
        //                (Parent as Frame).Navigate(typeof(ManagerPage), userParameter);
        //                break;

        //            case 4:
        //                (Parent as Frame).Navigate(typeof(EngineerPage), userParameter);
        //                break;

        //            case 5:
        //                (Parent as Frame).Navigate(typeof(HRPage), userParameter);
        //                break;

        //            case 6:
        //                (Parent as Frame).Navigate(typeof(DirectorPage), userParameter);
        //                break;

        //            case 7:
        //                (Parent as Frame).Navigate(typeof(ClientPage), userParameter);
        //                break;

        //            default:
        //                throw new Exception("Произошла ошибка во время определения пользователя. Обратитесь к системному администратору.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await new MessageDialog($"Произошла следующая ошибка: \"{ex.Message}\"", "Что-то пошло не так :(").ShowAsync();
        //    }
        //}

        private async void LogInButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            var user = new User();
            Progress.IsActive = true;
            try
            {
                await connection.OpenAsync();

                if (connection.State == System.Data.ConnectionState.Open)
                {
                    using (var command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "usp_Autorization";
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@login", loginTextBox.Text));
                        command.Parameters.Add(new SqlParameter("@password", passwordPasswordBox.Password));
                        command.Parameters.Add(new SqlParameter("@accountId", -1)).Direction = System.Data.ParameterDirection.Output;
                        command.Parameters.Add(new SqlParameter("@success", 0)).Direction = System.Data.ParameterDirection.Output;

                        await command.ExecuteNonQueryAsync();

                        if (Convert.ToInt32(command.Parameters["@success"].Value) == 1)
                        {
                            user.AccountId = Convert.ToInt32(command.Parameters["@accountId"].Value);

                            command.Parameters.Clear();
                            command.CommandText = "usp_GetUser";
                            command.Parameters.Add(new SqlParameter("@accountId", user.AccountId));
                            command.Parameters.Add(new SqlParameter("@id", System.Data.SqlDbType.Int)).Direction = System.Data.ParameterDirection.Output;
                            command.Parameters.Add(new SqlParameter("@firstName", System.Data.SqlDbType.NVarChar, 60)).Direction = System.Data.ParameterDirection.Output;
                            command.Parameters.Add(new SqlParameter("@lastName", System.Data.SqlDbType.NVarChar, 60)).Direction = System.Data.ParameterDirection.Output;
                            command.Parameters.Add(new SqlParameter("@patronymic", System.Data.SqlDbType.NVarChar, 60)).Direction = System.Data.ParameterDirection.Output;
                            command.Parameters.Add(new SqlParameter("@email", System.Data.SqlDbType.NVarChar, 512)).Direction = System.Data.ParameterDirection.Output;
                            command.Parameters.Add(new SqlParameter("@phoneNumber", System.Data.SqlDbType.NVarChar, 30)).Direction = System.Data.ParameterDirection.Output;
                            command.Parameters.Add(new SqlParameter("@typeId", System.Data.SqlDbType.Int)).Direction = System.Data.ParameterDirection.Output;

                            await command.ExecuteNonQueryAsync();

                            user.FirstName = (string)command.Parameters["@firstName"].Value;
                            user.LastName = (string)command.Parameters["@lastName"].Value;
                            user.Patronymic = (string)command.Parameters["@patronymic"].Value;
                            user.Email = (string)command.Parameters["@email"].Value;
                            user.PhoneNumer = (string)command.Parameters["@phoneNumber"].Value;
                            user.TypeId = Convert.ToInt32(command.Parameters["@typeId"].Value);
                            user.Id = Convert.ToInt32(command.Parameters["@id"].Value);
                        }
                    }

                }
                connection.Close();

                //var userParameter = new UserParameter(user, connection);
                //userParameter.Connection = connection;
                //userParameter.CurrentUser = user;
                (Parent as Frame).Navigate(typeof(MainPage), user);
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Произошла следующая ошибка: \"{ex.Message}\"", "Что-то пошло не так :(").ShowAsync();
            }
            Progress.IsActive = false;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Exit();
        }
    }
}
