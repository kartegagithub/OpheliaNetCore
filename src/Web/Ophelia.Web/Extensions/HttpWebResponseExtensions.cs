using System.IO;
using System.Net;

namespace Ophelia.Web
{
    public static class HttpWebResponseExtensions
    {
        public static string Read(this HttpWebResponse response)
        {
            string responseFromServer = "";

            var dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            responseFromServer = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();

            return responseFromServer;
        }
    }
}
