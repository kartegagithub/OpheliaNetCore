using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

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
    }
}
