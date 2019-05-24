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
using Windows.UI.Xaml.Navigation;

namespace ServiceCentreClientApp.Pages
{
    public sealed partial class UserActionsPage : Page
    {
        SqlConnection connection;
        User user;
        Account account;
        Byte[] ByteImage;

        public UserActionsPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            connection = (e.Parameter as UserParameter).Connection;
            user = (e.Parameter as UserParameter).CurrentUser;
            account = (e.Parameter as UserParameter).CurrentAccount;

            if (user != null)
            {
                LoadUser(user);
                ByteImage = await GetUserPhoto(user);
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
                ByteImage = await new ImageConverter().ConvertRandomAccessStreamToByteArray(await selectedFile.OpenAsync(Windows.Storage.FileAccessMode.Read));
                Photo.Source = writeableBitmap;
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
            Patronymic.Text = user.Patronymic;
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
