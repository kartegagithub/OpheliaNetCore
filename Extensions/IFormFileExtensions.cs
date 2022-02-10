using Microsoft.AspNetCore.Http;
using Ophelia.Service;
using System.IO;

namespace Ophelia
{
    /// <summary>
    /// 
    /// </summary>
    public static class IFormFileExtensions
    {
        public static byte[] ToByteArray(this IFormFile formFile)
        {
            using (var inputStream = new MemoryStream())
            {
                formFile.CopyTo(inputStream);
                byte[] array = new byte[inputStream.Length];
                inputStream.Seek(0, SeekOrigin.Begin);
                inputStream.Read(array, 0, array.Length);
                return array;
            }
        }

        public static FileData ToFileData(this IFormFile formFile, string key = "")
        {
            if (formFile == null || formFile.Length <= 0)
                return null;

            return new FileData()
            {
                KeyName = string.IsNullOrEmpty(key) ? formFile.FileName : key,
                FileName = formFile.FileName,
                ByteData = formFile.ToByteArray(),
                FileSize = formFile.Length
            };
        }
    }
}
