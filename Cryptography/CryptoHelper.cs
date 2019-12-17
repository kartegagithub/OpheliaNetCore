using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Cryptography
{
    public static class CryptoHelper
    {
        public static byte[] ConcatToByteArray(string password, string salt)
        {
            return Encoding.Unicode.GetBytes(String.Concat(salt, password));
        }

        public static string GetHexaDecimal(byte[] bytes)
        {
            int length = bytes.Length;
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i <= length - 1; i++)
                builder.Append(bytes[i].ToString("x2"));
            
            //for (int n = 0; n <= length - 1; n++)
            //    builder.Append(String.Format("{0,2:x}", bytes[n]).Replace(" ", "0"));
            return builder.ToString();
        }
    }
}
