using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using ServiceCentreClientApp.Entities;
using ServiceCentreClientApp.Parameters;
using ServiceCentreClientApp.Tools;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

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
            BirthDatePicker.SelectedDate = null;
            FirstNameTextBox.TextChanging += TextChanging;
            LastNameTextBox.TextChanging += TextChanging;
            PassportNumberTextBox.TextChanging += TextChanging;
            PhoneNumberTextBox.TextChanging += TextChanging;
            EmailTextBox.TextChanging += TextChanging;
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
                SaveButton.Visibility = Visibility.Collapsed;
                EditButton.IsEnabled = true;
                CancelButton.IsEnabled = true;
                if ((account != null) && (account.Id == -1))
                {
                    OptionsButton.Visibility = Visibility.Collapsed;
                }
                ByteImage = await GetUserPhoto(user);
                if (user.Photo != null)
                {
                    BitmapImage bitmapImage = await ImageConverter.ByteArrayToBitmapImageAsync(ByteImage);
                    if (bitmapImage != null)
                        PhotoImage.ImageSource = bitmapImage;
                }
            }
            else
            {
                EditButton.Visibility = Visibility.Collapsed;
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
                PhotoImage.ImageSource = writeableBitmap;
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

        private async void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            await ConfirmationDialog.ShowAsync();
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
                    TypeId = (TypeComboBox.SelectedItem as UserType).Id,
                    IsDeleted = false
                };

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"UPDATE [dbo].[User] SET " +
                        $"BirthDate = '{editedUser.BirthDate.Date.Year.ToString() + "-" + editedUser.BirthDate.Date.Month.ToString() + "-" + editedUser.BirthDate.Date.Day.ToString()}', " +
                        $"FirstName = N'{editedUser.FirstName}', LastName = N'{editedUser.LastName}', Patronymic = N'{editedUser.Patronymic}', " +
                        $"PassportNumber = '{editedUser.PassportNumber}', PhoneNumber = '{editedUser.PhoneNumber}', Email = '{editedUser.Email}', " +
                        $"TypeId = {editedUser.TypeId}, IsDeleted = {Convert.ToInt32(editedUser.IsDeleted)} Photo = ";

                    var imageParameter = new SqlParameter("@photo", System.Data.SqlDbType.VarBinary);

                    imageParameter.Direction = System.Data.ParameterDirection.Input;
                    if (ByteImage != null)
                    {
                        imageParameter.Size = ByteImage.Length;
                        imageParameter.Value = ByteImage;
                        command.CommandText += $"@photo WHERE [User].[Id] = {editedUser.Id}";
                        command.Parameters.Add(imageParameter);
                    }
                    else
                    {
                        command.CommandText += $"null WHERE [User].[Id] = {editedUser.Id}";
                    }

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
                        $"N'{newUser.FirstName}', N'{newUser.LastName}', N'{newUser.Patronymic}', " +
                        $"'{newUser.BirthDate.Date.Year.ToString() + "-" + newUser.BirthDate.Date.Month.ToString() + "-" + newUser.BirthDate.Date.Day.ToString()}', " +
                        $"'{newUser.PassportNumber}', '{newUser.Email}', '{newUser.PhoneNumber}', ";

                    var imageParameter = new SqlParameter("@photo", System.Data.SqlDbType.VarBinary);
                    imageParameter.Direction = System.Data.ParameterDirection.Input;
                    if (ByteImage != null)
                    {
                        imageParameter.Size = ByteImage.Length;
                        imageParameter.Value = ByteImage;
                        command.CommandText += $"@photo, {newUser.TypeId}, {account.Id}, 0)";
                        command.Parameters.Add(imageParameter);
                    }
                    else
                    {
                        command.CommandText += $"null, {newUser.TypeId}, {account.Id}, 0)";
                    }

                    await command.ExecuteNonQueryAsync();
                }
            }
            connection.Close();
            (Parent as Frame).GoBack();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            ControlsInteraction.EnableControls(this);

            SaveButton.Visibility = Visibility.Visible;
            EditButton.Visibility = Visibility.Collapsed;
        }

        private void EmailTextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            if (TextBoxRegex.GetIsValid(EmailTextBox) || string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                EmailTextBox.BorderBrush = LastNameTextBox.BorderBrush;
            }
            else
            {
                EmailTextBox.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
            }
        }

        private async void RefreshComboBoxButton_Click(object sender, RoutedEventArgs e)
        {
            await GetUserTypesAsync();
            TypeComboBox.ItemsSource = types;
        }

        private void FirstLetterToUpperOnLostFocus(object sender, RoutedEventArgs e)
        {
            string text = (sender as TextBox).Text.ToLower();
            if (!string.IsNullOrWhiteSpace(text))
                (sender as TextBox).Text = char.ToUpper(text[0]) + text.Substring(1);
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            user.IsDeleted = true;
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"UPDATE [User] SET IsDeleted=1 WHERE [User].[Id]={user.Id}";
                await command.ExecuteNonQueryAsync();
            }
            connection.Close();
            await new MessageDialog("Пользователь был успешно удалён.").ShowAsync();
            (Parent as Frame).GoBack();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            sender.Hide();
        }

        private void CheckInputs()
        {
            if (!string.IsNullOrWhiteSpace(FirstNameTextBox.Text) 
                && !string.IsNullOrWhiteSpace(LastNameTextBox.Text) 
                && !string.IsNullOrWhiteSpace(PassportNumberTextBox.Text) && TextBoxRegex.GetIsValid(PassportNumberTextBox)
                && !string.IsNullOrWhiteSpace(EmailTextBox.Text) && TextBoxRegex.GetIsValid(EmailTextBox)
                && !string.IsNullOrWhiteSpace(PhoneNumberTextBox.Text) && TextBoxRegex.GetIsValid(PhoneNumberTextBox)
                && BirthDatePicker.SelectedDate != null)
            {
                SaveButton.IsEnabled = true;
            }
            else
            {
                SaveButton.IsEnabled = false;
            }
        }

        private void TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            CheckInputs();
        }
    }
}
