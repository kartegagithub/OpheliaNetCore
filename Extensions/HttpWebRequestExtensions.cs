﻿using System.Net;

namespace Ophelia
{
    public static class HttpWebRequestExtensions
    {
        public static HttpWebResponse GetResponseWithoutException(this HttpWebRequest req)
        {
            try
            {
                return (HttpWebResponse)req.GetResponse();
            }
            catch (WebException we)
            {
                var resp = we.Response as HttpWebResponse;
                if (resp == null)
                    throw;
                return resp;
            }
        }
    }
}
