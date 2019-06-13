using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;

namespace ServiceCentreClientApp.Tools
{
    public class WorkingWithFiles
    {
        /// <summary>
        /// Асинхронное сохранение документа Microsoft Word на диск.
        /// </summary>
        /// <param name="streams">Поток данных, содержащий документ.</param>
        /// <param name="filename">Имя документа.</param>
        /// <returns></returns>
        public static async Task SaveDocumentAsync(MemoryStream streams, string filename)
        {
            try
            {
                streams.Position = 0;
                StorageFile stFile;
                if (!(Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons")))
                {
                    FileSavePicker savePicker = new FileSavePicker();
                    savePicker.DefaultFileExtension = ".docx";
                    savePicker.SuggestedFileName = filename;
                    savePicker.FileTypeChoices.Add("Word Documents", new List<string>() { ".docx" });
                    stFile = await savePicker.PickSaveFileAsync();
                }
                else
                {
                    StorageFolder local = ApplicationData.Current.LocalFolder;
                    stFile = await local.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
                }
                if (stFile != null)
                {
                    using (IRandomAccessStream zipStream = await stFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        using (Stream outstream = zipStream.AsStreamForWrite())
                        {
                            byte[] buffer = streams.ToArray();
                            outstream.Write(buffer, 0, buffer.Length);
                            outstream.Flush();
                        }
                    }
                }
                MessageDialog msgDialog = new MessageDialog("Хотите открыть документ сейчас?", "Файл успешно сохранён.");
                UICommand yesCmd = new UICommand("Да");
                msgDialog.Commands.Add(yesCmd);
                UICommand noCmd = new UICommand("Нет");
                msgDialog.Commands.Add(noCmd);
                IUICommand cmd = await msgDialog.ShowAsync();
                if (cmd == yesCmd)
                {
                    await Windows.System.Launcher.LaunchFileAsync(stFile);
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Произошла следующая ошибка: {ex.Message}", "Что-то пошло не так :(").ShowAsync();
            }
        }
    }
}
