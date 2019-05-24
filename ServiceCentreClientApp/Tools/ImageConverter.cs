using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace ServiceCentreClientApp.Tools
{
    class ImageConverter
    {
        public async Task<byte[]> ConvertRandomAccessStreamToByteArray(IRandomAccessStream stream)
        {
            DataReader dataReader = new DataReader(stream.GetInputStreamAt(0));
            byte[] bytes = new byte[stream.Size];
            await dataReader.LoadAsync((uint)stream.Size);
            dataReader.ReadBytes(bytes);
            return bytes;
        }

        public async Task<byte[]> BitmapImageToByteArrayAsync(BitmapImage image)
        {
            RandomAccessStreamReference streamReference = RandomAccessStreamReference.CreateFromUri(image.UriSource);
            IRandomAccessStreamWithContentType streamWithContent = await streamReference.OpenReadAsync();
            byte[] buffer = new byte[streamWithContent.Size];
            await streamWithContent.ReadAsync(buffer.AsBuffer(), (uint)streamWithContent.Size, InputStreamOptions.None);
            return buffer;
        }

        public async Task<BitmapImage> ByteArrayToBitmapImageAsync(byte[] bytes)
        {
            BitmapImage image = new BitmapImage();
            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                await stream.WriteAsync(bytes.AsBuffer());
                stream.Seek(0);
                await image.SetSourceAsync(stream);
            }
            return image;
        }
    }
}
