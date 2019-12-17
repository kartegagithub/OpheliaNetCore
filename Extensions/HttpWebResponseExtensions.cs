using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia
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
