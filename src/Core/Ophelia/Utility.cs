using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Ophelia
{
    /// <summary>
    /// Provides various utility methods for common tasks such as password generation, string manipulation, and object cloning.
    /// </summary>
    public static class Utility
    {
        private static DateTimeKind _NowType = DateTimeKind.Local;
        /// <summary>
        /// Gets the current DateTimeKind setting for the application.
        /// </summary>
        public static DateTimeKind NowType { get { return _NowType; } }

        /// <summary>
        /// Gets the current time based on the <see cref="NowType"/>.
        /// </summary>
        public static DateTime Now => _NowType == DateTimeKind.Utc ? DateTime.UtcNow : DateTime.Now;

        public static void SetNowType(DateTimeKind kind)
        {
            _NowType = kind;
        }
        /// <summary>
        /// Generates a random password string of the specified size.
        /// </summary>
        /// <param name="maxSize">The length of the password to generate.</param>
        /// <param name="isNumeric">If true, the password will only contain numeric characters.</param>
        /// <returns>A randomly generated string.</returns>
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
        /// <summary>
        /// Generates a semi-random unique string based on the current time.
        /// </summary>
        /// <returns>A string representation of a random number and time.</returns>
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
        /// <summary>
        /// Creates a deep clone of the specified object using JSON serialization.
        /// </summary>
        /// <param name="Original">The object to clone.</param>
        /// <returns>A new instance of the object with the same values.</returns>
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

        /// <summary>
        /// Encodes a URL string and optionally replaces special characters with dashes.
        /// </summary>
        /// <param name="strIn">The input string to encode.</param>
        /// <param name="ReplaceSpecialChars">If true, replaces non-alphanumeric characters with dashes.</param>
        /// <returns>The encoded and formatted URL string.</returns>
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
