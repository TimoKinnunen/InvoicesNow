using System;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace InvoicesNow.Helpers
{
    /// <summary>
    /// Generates a logotype from an image
    /// use temporary folder
    /// </summary>
    public sealed class HelpLogotypeMaker
    {
        static string FileName { get; set; }

        public HelpLogotypeMaker(Guid SellerId)
        {
            FileName = SellerId.ToString();
        }

        /// <summary>
        /// Creates a logotype (as thumbnail) image in temporary storage based on the source file, then returns the URI to the thumbnail
        /// </summary>
        /// <param name="file">The source image file to generate the logotype (as thumbnail) from</param>
        /// <returns>The URI to the generated logotype (as thumbnail) in temporary storage</returns>
        public async Task<StorageFile> GenerateLogotypeAsThumbnailAsync(IStorageFile file, uint logotypeWidth)
        {
            using (IRandomAccessStreamWithContentType fileStream = await file.OpenReadAsync())
            {
                // decode the file using the built-in image decoder
                BitmapDecoder bitmapDecoder = await BitmapDecoder.CreateAsync(fileStream);

                uint height = bitmapDecoder.PixelHeight;
                uint width = bitmapDecoder.PixelWidth;
                // use only width
                height = (uint)(logotypeWidth * height / (float)width);
                width = (uint)(logotypeWidth * width / (float)width);

                // create the output file for the logotype (as thumbnail)
                StorageFile logotypeFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(
                    $"{FileName}{file.FileType}",
                    CreationCollisionOption.ReplaceExisting);

                // create a stream for the output file
                using (IRandomAccessStream outputStream = await logotypeFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    // create an encoder from the existing decoder and set the scaled height
                    // and width 
                    BitmapEncoder bitmapEncoder = await BitmapEncoder.CreateForTranscodingAsync(
                        outputStream,
                        bitmapDecoder);
                    bitmapEncoder.BitmapTransform.ScaledHeight = height;
                    bitmapEncoder.BitmapTransform.ScaledWidth = width;
                    //encoder.BitmapTransform.ScaledHeight = 200;
                    //encoder.BitmapTransform.ScaledWidth = 200;
                    await bitmapEncoder.FlushAsync();
                }

                return logotypeFile;
            }
        }
    }
}
