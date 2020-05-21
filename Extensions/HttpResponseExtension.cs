using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace Ophelia
{
    public static class HttpResponseExtension
    {
        public static void Write(this HttpResponse response, TextWriter writer)
        {
            response.Write(writer.ToString());
        }
        public static void Write(this HttpResponse response, string text)
        {
            response.Body.Write(System.Text.Encoding.UTF8.GetBytes(text));
        }
        public static ValueTask WriteAsync(this HttpResponse response, string text)
        {
            return response.Body.WriteAsync(System.Text.Encoding.UTF8.GetBytes(text));
        }
        public static void Write(this HttpResponse response, byte[] data)
        {
            response.Body.Write(data);
        }
        public static void Flush(this HttpResponse response)
        {
            response.Body.Flush();
        }
        public static void ClearContent(this HttpResponse response)
        {
            response.Clear();
        }
        public static void ClearHeaders(this HttpResponse response)
        {
            response.Headers.Clear();
        }
        public static void End(this HttpResponse response)
        {
            try
            {
                response.StatusCode = StatusCodes.Status200OK;
            }
            catch (System.Exception)
            {
                //Do nothing
            }
        }
        public static void AddHeader(this HttpResponse response, string key, string value)
        {
            response.Headers.Add(key, value);
        }
    }
}
