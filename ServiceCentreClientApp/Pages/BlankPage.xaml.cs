using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ServiceCentreClientApp.Pages
{
    public sealed partial class BlankPage : Page
    {
        public BlankPage()
        {
            this.InitializeComponent();
        }

        private async Task CreateAndSaveDocxAsync()
        {
            using (var document = new WordDocument())
            {
                StorageFile file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Assets\template.docx");

                await document.OpenAsync(file, FormatType.Docx);

                BookmarksNavigator bookmarkNavigator = new BookmarksNavigator(document);
                bookmarkNavigator.MoveToBookmark("RequestId");
                bookmarkNavigator.InsertText("0000000");
                bookmarkNavigator.MoveToBookmark("");
                bookmarkNavigator.InsertText("");
                bookmarkNavigator.MoveToBookmark("");
                bookmarkNavigator.InsertText("");
                bookmarkNavigator.MoveToBookmark("");
                bookmarkNavigator.InsertText("");
                bookmarkNavigator.MoveToBookmark("");
                bookmarkNavigator.InsertText("");
                bookmarkNavigator.MoveToBookmark("");
                bookmarkNavigator.InsertText("");
                bookmarkNavigator.MoveToBookmark("");
                bookmarkNavigator.InsertText("");

                MemoryStream stream = new MemoryStream();

                //Saves the Word document to MemoryStream
                await document.SaveAsync(stream, FormatType.Docx);

                //Saves the stream as Word document file in local machine
                await Tools.WorkingWithFiles.SaveDocumentAsync(stream, "Sample.docx");
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await CreateAndSaveDocxAsync();
        }
    }
}
