using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Ophelia
{
    public static class Utility
    {
        private static DateTimeKind _NowType = DateTimeKind.Local;
        public static DateTimeKind NowType { get { return _NowType; } }

        public static DateTime Now => _NowType == DateTimeKind.Utc ? DateTime.UtcNow : DateTime.Now;

        public static void SetNowType(DateTimeKind kind)
        {
            _NowType = kind;
        }
        public static string GenerateRandomPassword(int maxSize, bool isNumeric = false)
        {
            char[] chars = new char[62];
            if (!isNumeric)
                chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            else
                chars = "1234567890".ToCharArray();

            byte[] data = new byte[1];
            var crypto = RandomNumberGenerator.Create();
            crypto.GetNonZeroBytes(data);
            data = new byte[maxSize];
            crypto.GetNonZeroBytes(data);
            StringBuilder result = new StringBuilder(maxSize);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }
        public static string Randomize()
        {
            var r = new Random();
            return $"{r.Next(1, 10000000)}{$"{Utility.Now:T}".Replace(":", string.Empty)}";
        }
        public static string GetQueryStringValue(string url, string queryString)
        {
            return System.Web.HttpUtility.ParseQueryString(url).Get(queryString);
        }
        public static string GetQueryStringValue(string url, string key, string seperator)
        {
            if (url.IndexOf(key, StringComparison.InvariantCultureIgnoreCase) > -1 && url.Contains(seperator))
            {
                url = (new Regex(seperator)).Replace(url, "?", 1);
                return GetQueryStringValue(url, key);
            }
            return string.Empty;
        }
        public static object Clone(object Original)
        {
            object clonedObject = null;
            try
            {

#if NETCOREAPP3_1
                clonedObject = JsonSerializer.Deserialize(JsonSerializer.Serialize(Original), Original.GetType(), new JsonSerializerOptions() { MaxDepth = 10 });
#else
                byte[] bytes = null;
                using (var stream = new MemoryStream())
                {
                    JsonSerializer.Serialize(stream, Original);
                    bytes = stream.ToArray();
                    stream.Close();
                }
                using (var stream = new MemoryStream())
                {
                    clonedObject = JsonSerializer.Deserialize(stream, Original.GetType(), new JsonSerializerOptions() { MaxDepth = 10 });
                    stream.Close();
                }
#endif
            }
            catch { return clonedObject; }
            return clonedObject;
        }

        public static string EncodeUrl(string strIn, bool ReplaceSpecialChars = true)
        {
            if (string.IsNullOrEmpty(strIn)) return strIn;
            StringBuilder sbOut = new StringBuilder(strIn.Trim().Replace("-", "").Replace("  ", "-").Replace(" ", "-").ReplaceSpecialVowelsAndConsonant());
            if (!ReplaceSpecialChars)
            {
                sbOut = sbOut.Replace("$", "-24");
                sbOut = sbOut.Replace("&", "-26");
                sbOut = sbOut.Replace("+", "-2B");
                sbOut = sbOut.Replace(",", "-2C");
                sbOut = sbOut.Replace("/", "-2F");
                sbOut = sbOut.Replace(":", "-3A");
                sbOut = sbOut.Replace(";", "-3B");
                sbOut = sbOut.Replace("=", "-3D");
                sbOut = sbOut.Replace("?", "-3F");
                sbOut = sbOut.Replace("@", "-40");
                sbOut = sbOut.Replace("'", "-22");
                sbOut = sbOut.Replace("<", "-3C");
                sbOut = sbOut.Replace(">", "-3E");
                sbOut = sbOut.Replace("#", "-23");
                sbOut = sbOut.Replace("%", "-25");
                sbOut = sbOut.Replace("{", "-7B");
                sbOut = sbOut.Replace("}", "-7D");
                sbOut = sbOut.Replace("|", "-7C");
                sbOut = sbOut.Replace("\\", "-5C");
                sbOut = sbOut.Replace("^", "-5E");
                sbOut = sbOut.Replace("~", "-7E");
                sbOut = sbOut.Replace("[", "-5B");
                sbOut = sbOut.Replace("]", "-5D");
                sbOut = sbOut.Replace("`", "-60");
                sbOut = sbOut.Replace("\"", "-22");
                sbOut = sbOut.Replace("‘", "-91");
                sbOut = sbOut.Replace("’", "-92");
                sbOut = sbOut.Replace("ˆ", "-88");
                sbOut = sbOut.Replace("‚", "-83");
                sbOut = sbOut.Replace("*", "-42");
                sbOut = sbOut.Replace("!", "-33");
            }
            else
            {
                sbOut = sbOut.Replace("$", "-");
                sbOut = sbOut.Replace("&", "-");
                sbOut = sbOut.Replace("+", "-");
                sbOut = sbOut.Replace(",", "-");
                sbOut = sbOut.Replace("/", "-");
                sbOut = sbOut.Replace(":", "-");
                sbOut = sbOut.Replace(";", "-");
                sbOut = sbOut.Replace("=", "-");
                sbOut = sbOut.Replace("?", "-");
                sbOut = sbOut.Replace("@", "-");
                sbOut = sbOut.Replace("'", "-");
                sbOut = sbOut.Replace("<", "-");
                sbOut = sbOut.Replace(">", "-");
                sbOut = sbOut.Replace("#", "-");
                sbOut = sbOut.Replace("%", "-");
                sbOut = sbOut.Replace("{", "-");
                sbOut = sbOut.Replace("}", "-");
                sbOut = sbOut.Replace("|", "-");
                sbOut = sbOut.Replace("\\", "-");
                sbOut = sbOut.Replace("^", "-");
                sbOut = sbOut.Replace("~", "-");
                sbOut = sbOut.Replace("[", "-");
                sbOut = sbOut.Replace("]", "-");
                sbOut = sbOut.Replace("`", "-");
                sbOut = sbOut.Replace("\"", "-");
                sbOut = sbOut.Replace("‘", "-");
                sbOut = sbOut.Replace("’", "-");
                sbOut = sbOut.Replace("ˆ", "-");
                sbOut = sbOut.Replace("‚", "-");
                sbOut = sbOut.Replace("*", "-");
                sbOut = sbOut.Replace("!", "-");
            }
            var result = sbOut.ToString().Trim('-');
            result = result.Trim('.');
            sbOut = null;

            var newResult = "";

            for (int i = 0; i < result.Length; i++)
            {
                if (Char.IsLetterOrDigit(result[i]) || Char.IsPunctuation(result[i]))
                {
                    newResult += result[i];
                }
            }
            result = null;

            var controlIndex = 0;
            while (newResult.IndexOf("--", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                newResult = newResult.Replace("--", "-");
                controlIndex++;
                if (controlIndex > 100)
                    break;
            }
            return newResult;
        }
    }
}
