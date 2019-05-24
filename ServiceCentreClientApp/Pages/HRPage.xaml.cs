﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using ServiceCentreClientApp.Entities;
using ServiceCentreClientApp.Parameters;
using ServiceCentreClientApp.Tools;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace ServiceCentreClientApp.Pages
{
    public sealed partial class HRPage : Page
    {
        SqlConnection connection;
        ObservableCollection<User> users;
        User user;

        public HRPage()
        {
            this.InitializeComponent();
            users = new ObservableCollection<User>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            user = (e.Parameter as User);
            connection = new SqlConnection((App.Current as App).ConnectionString);

            await GetUsersFromServerAsync();

            base.OnNavigatedTo(e);
        }

        private async Task GetUsersFromServerAsync()
        {
            try
            {
                if (users.Count > 0)
                    users = new ObservableCollection<User>();

                Progress.IsActive = true;
                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM [User]";

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                BitmapImage bitmapImage;
                                if (await reader.IsDBNullAsync(8))
                                {
                                    bitmapImage = null;
                                }
                                else
                                {
                                    bitmapImage = await new ImageConverter().ByteArrayToBitmapImageAsync((Byte[])reader["photo"]);
                                }
                                users.Add(
                                    new User
                                    {
                                        Id = reader.GetInt32(0),
                                        FirstName = reader.GetString(1),
                                        LastName = reader.GetString(2),
                                        Patronymic = reader.GetString(3),
                                        BirthDate = reader.GetDateTime(4),
                                        PassportNumber = reader.GetString(5),
                                        Email = reader.GetString(6),
                                        PhoneNumer = reader.GetString(7),
                                        Photo = bitmapImage,
                                        TypeId = reader.GetInt32(9),
                                        AccountId = reader.GetInt32(10)
                                    });
                            }
                        }
                    }
                    Progress.IsActive = false;
                }

            }
            catch (Exception ex)
            {
                await new MessageDialog($"Произошла следующая ошибка: \"{ex.Message}\"", "Что-то пошло не так :(").ShowAsync();
            }
        }

        private void RequestsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            (Parent as Frame).Navigate(typeof(UserActionsPage), new UserParameter(e.ClickedItem as User, connection));
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            (Parent as Frame).Navigate(typeof(UserActionsPage), new UserParameter(null, connection));
        }
    }
}