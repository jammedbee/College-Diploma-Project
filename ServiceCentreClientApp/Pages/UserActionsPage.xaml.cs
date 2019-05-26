using System;
using System.Collections.Generic;
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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using System.Collections.ObjectModel;

namespace ServiceCentreClientApp.Pages
{
    public sealed partial class UserActionsPage : Page
    {
        SqlConnection connection;
        ObservableCollection<UserType> types;
        User user;
        Account account;
        byte[] ByteImage;

        public UserActionsPage()
        {
            this.InitializeComponent();
            types = new ObservableCollection<UserType>();
        }
        
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            connection = (e.Parameter as UserParameter).Connection;
            user = (e.Parameter as UserParameter).CurrentUser;
            account = (e.Parameter as UserParameter).CurrentAccount;
            await GetUserTypesAsync();

            if (user != null)
            {
                LoadUser(user);
                TypeComboBox.ItemsSource = types;
                TypeComboBox.SelectedItem = types.FirstOrDefault(t => t.Id == user.TypeId);
                ControlsInteraction.DisableControls(this);
                EditButton.IsEnabled = true;
                CancelButton.IsEnabled = true;
                if ((account != null) && (account.Id == -1))
                {
                    OptionsButton.Visibility = Visibility.Collapsed;
                }
                ByteImage = await GetUserPhoto(user);
                BitmapImage bitmapImage = await ImageConverter.ByteArrayToBitmapImageAsync(ByteImage);
                PhotoImage.Source = bitmapImage;
            }
            else
            {
                TypeComboBox.ItemsSource = types;
            }

            base.OnNavigatedTo(e);
        }

        private async Task<byte[]> GetUserPhoto(User user)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"SELECT photo FROM [dbo].[User] WHERE [User].[Id] = {user.Id}";

                using (var reader = await command.ExecuteReaderAsync())
                {
                    reader.Read();
                    if (reader["photo"] != DBNull.Value)
                    {
                        return (byte[])reader["photo"];
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        private async void UploadPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            Windows.Storage.Pickers.FileOpenPicker fileOpenPicker = new Windows.Storage.Pickers.FileOpenPicker();
            fileOpenPicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            fileOpenPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.FileTypeFilter.Add(".jpeg");
            fileOpenPicker.FileTypeFilter.Add(".png");

            Windows.Storage.StorageFile selectedFile = await fileOpenPicker.PickSingleFileAsync();
            if (selectedFile != null)
            {
                BitmapImage image = new BitmapImage();
                await image.SetSourceAsync(await selectedFile.OpenAsync(Windows.Storage.FileAccessMode.Read));
                WriteableBitmap writeableBitmap = new WriteableBitmap(image.PixelWidth, image.PixelHeight);
                await writeableBitmap.SetSourceAsync(await selectedFile.OpenAsync(Windows.Storage.FileAccessMode.Read));
                ByteImage = await ImageConverter.ConvertRandomAccessStreamToByteArray(await selectedFile.OpenAsync(Windows.Storage.FileAccessMode.Read));
                PhotoImage.Source = writeableBitmap;
            }
        }

        private void LoadUser(User user)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            FirstNameTextBox.Text = user.FirstName;
            LastNameTextBox.Text = user.LastName;
            PatronymicTextBox.Text = user.Patronymic;
            PassportNumberTextBox.Text = user.PassportNumber;
            EmailTextBox.Text = user.Email;
            PhoneNumberTextBox.Text = user.PhoneNumber;
            BirthDatePicker.SelectedDate = user.BirthDate;
        }

        private async Task GetUserTypesAsync()
        {
            if (types.Count > 0)
                types = new ObservableCollection<UserType>();

            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM UserType";

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                        while (await reader.ReadAsync())
                            types.Add(new UserType
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1)
                            });
                }
            }
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            (Parent as Frame).GoBack();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();
            if (user != null)
            {
                User editedUser = new User
                {
                    AccountId = user.AccountId,
                    Id = user.Id,
                    BirthDate = BirthDatePicker.Date.Date,
                    FirstName = FirstNameTextBox.Text,
                    LastName = LastNameTextBox.Text,
                    Patronymic = PatronymicTextBox.Text,
                    PassportNumber = PassportNumberTextBox.Text,
                    PhoneNumber = PhoneNumberTextBox.Text,
                    Email = EmailTextBox.Text,
                    TypeId = (TypeComboBox.SelectedItem as UserType).Id
                };

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"UPDATE [dbo].[User] SET " +
                        $"BirthDate = '{editedUser.BirthDate.Date.Year.ToString() + "-" + editedUser.BirthDate.Date.Month.ToString() + "-" + editedUser.BirthDate.Date.Day.ToString()}', " +
                        $"FirstName = '{editedUser.FirstName}', LastName = '{editedUser.LastName}', Patronymic = '{editedUser.Patronymic}', " +
                        $"PassportNumber = '{editedUser.PassportNumber}', PhoneNumber = '{editedUser.PhoneNumber}', Email = '{editedUser.Email}', " +
                        $"TypeId = {editedUser.TypeId}, Photo = @photo " +
                        $"WHERE [User].[Id] = {editedUser.Id}";

                    var imageParameter = new SqlParameter("@photo", System.Data.SqlDbType.VarBinary)
                    {
                        Direction = System.Data.ParameterDirection.Input,
                        Size = ByteImage.Length,
                        Value = ByteImage
                    };

                    command.Parameters.Add(imageParameter);

                    await command.ExecuteNonQueryAsync();
                }

                (Parent as Frame).GoBack();
            }
            else
            {
                User newUser = new User
                {
                    AccountId = account.Id,
                    BirthDate = BirthDatePicker.Date.Date,
                    FirstName = FirstNameTextBox.Text,
                    LastName = LastNameTextBox.Text,
                    Patronymic = PatronymicTextBox.Text,
                    PassportNumber = PassportNumberTextBox.Text,
                    PhoneNumber = PhoneNumberTextBox.Text,
                    Email = EmailTextBox.Text,
                    TypeId = (TypeComboBox.SelectedItem as UserType).Id
                };

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"usp_NewAccount";
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@accountId", System.Data.SqlDbType.Int)).Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add(new SqlParameter("@login", account.Login));
                    command.Parameters.Add(new SqlParameter("@password", account.Password));
                    await command.ExecuteNonQueryAsync();
                    account.Id = (int)command.Parameters[0].Value;
                    command.Parameters.Clear();
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = $"INSERT INTO [dbo].[User] VALUES (" +
                        $"'{newUser.FirstName}', '{newUser.LastName}', '{newUser.Patronymic}', " +
                        $"'{newUser.BirthDate.Date.Year.ToString() + "-" + newUser.BirthDate.Date.Month.ToString() + "-" + newUser.BirthDate.Date.Day.ToString()}', " +
                        $"'{newUser.PassportNumber}', '{newUser.PhoneNumber}', '{newUser.Email}', " +
                        $"@photo, {newUser.TypeId}, {account.Id})";

                    var imageParameter = new SqlParameter("@photo", System.Data.SqlDbType.VarBinary)
                    {
                        Direction = System.Data.ParameterDirection.Input,
                        Size = ByteImage.Length,
                        Value = ByteImage
                    };

                    command.Parameters.Add(imageParameter);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            ControlsInteraction.EnableControls(this);
        }

        private void EmailTextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            if (TextBoxRegex.GetIsValid(EmailTextBox))
            {
                // To-Do
            }
                
        }

        private async void RefreshComboBoxButton_Click(object sender, RoutedEventArgs e)
        {
            await GetUserTypesAsync();
            TypeComboBox.ItemsSource = types;
        }
    }
}
