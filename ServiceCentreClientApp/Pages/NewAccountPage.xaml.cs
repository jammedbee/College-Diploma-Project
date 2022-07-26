﻿using ServiceCentreClientApp.Parameters;
using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ServiceCentreClientApp.Pages
{
    public sealed partial class NewAccountPage : Page
    {
        SqlConnection connection;

        public NewAccountPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            connection = e.Parameter as SqlConnection;
        }

        private async void Login_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckingLoginProgress.IsActive = true;
            if (String.IsNullOrWhiteSpace(Login.Text))
            {

            }
            else
            {
                using (SqlCommand command = 
                new SqlCommand($"SELECT COUNT(*) FROM Account WHERE (Account.Login LIKE '{Login.Text}')", connection))
                {
                    if (connection.State == System.Data.ConnectionState.Closed)
                        await connection.OpenAsync();
                    // проверка на наличие введённого логина в базе данных 
                    int result = (int)await command.ExecuteScalarAsync();
                    if (result != 0) // если такой логин уже существует, вывести сообщение об этом
                    {
                        Continue.IsEnabled = false;
                        LoginExistsFlyout.ShowAt(Login);
                    }
                    else
                    {
                        CheckInputs();
                        LoginExistsFlyout.Hide();
                    }
                }
            }
            CheckingLoginProgress.IsActive = false;
        }

        private void CheckInputs()
        {
            if ((Password.Password == ConfirmedPassword.Password) &&
                (!String.IsNullOrWhiteSpace(Password.Password)) &&
                (!String.IsNullOrWhiteSpace(Login.Text)))
            {
                Continue.IsEnabled = true;
            }
            else
            {
                Continue.IsEnabled = false;
            }
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            (Parent as Frame).Navigate(typeof(UserActionsPage), new UserParameter(null, connection, new Entities.Account
            {
                Id = 0,
                Login = Login.Text,
                // шифровка пароля с помощью алгоритма SHA-512 
                Password = Encoding.Unicode.GetString(
                    new SHA512Managed()
                    .ComputeHash(Encoding.Unicode.GetBytes(Password.Password)))
            }));
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Login.Text = string.Empty;
            Password.Password = string.Empty;
            ConfirmedPassword.Password = string.Empty;
            (Parent as Frame).GoBack();
        }

        private void ConfirmedPassword_PasswordChanging(PasswordBox sender, PasswordBoxPasswordChangingEventArgs args)
        {
            CheckInputs();
        }

        private async void Login_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            CheckingLoginProgress.IsActive = true;
            if (String.IsNullOrWhiteSpace(Login.Text))
            {

            }
            else
            {
                using (SqlCommand command = new SqlCommand($"SELECT COUNT(*) FROM [Account] WHERE ([Login] LIKE '{Login.Text}')", connection))
                {
                    if (connection.State == System.Data.ConnectionState.Closed)
                        await connection.OpenAsync();
                    int result = (int)await command.ExecuteScalarAsync();
                    if (result != 0)
                        Continue.IsEnabled = false;

                }
            }
            CheckingLoginProgress.IsActive = false;
        }
    }
}