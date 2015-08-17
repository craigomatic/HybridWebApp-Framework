using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using System.IO;

namespace HybridWebApp.Toolkit.Imaging
{
    public static class ImagingExtensions
    {
        public async static Task<string> ToDataUriAsync(this IRandomAccessStream randomAccessStream, string contentType)
        {
            var buffer = new byte[randomAccessStream.Size];
            await randomAccessStream.AsStreamForRead().ReadAsync(buffer, 0, buffer.Length);

            return string.Format("data:image/{0};base64,{1}", contentType, Convert.ToBase64String(buffer));
        }
    }
}
