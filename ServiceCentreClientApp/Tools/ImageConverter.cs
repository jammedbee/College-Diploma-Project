using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace ServiceCentreClientApp.Tools
{
    public static class ImageConverter
    {
        /// <summary>
        /// Асинхронная конвертация потока IRandomAccessStream в массив байтов.
        /// </summary>
        /// <param name="stream">Объект IRandomAccessStream.</param>
        /// <returns>Массив байтов, представляющий данные из потока.</returns>
        public static async Task<byte[]> ConvertRandomAccessStreamToByteArrayAsync(IRandomAccessStream stream)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));

            DataReader dataReader = new DataReader(stream.GetInputStreamAt(0));
            byte[] bytes = new byte[stream.Size];
            await dataReader.LoadAsync((uint)stream.Size);
            dataReader.ReadBytes(bytes);
            return bytes;
        }
        /// <summary>
        /// Асинхронная конвертация объекта BitmapImage в массив байтов.
        /// </summary>
        /// <param name="image">Объект BitmapImage.</param>
        /// <returns>Массив байтов, представляющий изображение.</returns>
        public static async Task<byte[]> BitmapImageToByteArrayAsync(BitmapImage image)
        {
            if (image is null)
                throw new ArgumentNullException(nameof(image));

            RandomAccessStreamReference streamReference = RandomAccessStreamReference.CreateFromUri(image.UriSource);
            IRandomAccessStreamWithContentType streamWithContent = await streamReference.OpenReadAsync();
            byte[] buffer = new byte[streamWithContent.Size];
            await streamWithContent.ReadAsync(buffer.AsBuffer(), (uint)streamWithContent.Size, InputStreamOptions.None);
            return buffer;
        }
        /// <summary>
        /// Асинхронная конвертация массива байтов в объект BitmapImage.
        /// </summary>
        /// <param name="bytes">Массив байтов, представляющий изображение.</param>
        /// <returns>Объект BitmapImage.</returns>
        public static async Task<BitmapImage> ByteArrayToBitmapImageAsync(byte[] bytes)
        {
            if (bytes is null)
                throw new ArgumentNullException(nameof(bytes));

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
