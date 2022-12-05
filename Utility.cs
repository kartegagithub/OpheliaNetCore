using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Collections.Generic;
namespace Ophelia
{
    public static class Utility
    {
        public static string Generate(PasswordOptions opts = null)
        {
            if (opts == null) opts = new PasswordOptions()
            {
                RequiredLength = 8,
                RequiredUniqueChars = 4,
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true
            };

            string[] randomChars = new[] {
                "ABCDEFGHJKLMNOPQRSTUVWXYZ",
                "abcdefghijkmnopqrstuvwxyz",
                "0123456789",
                "!@$?_-"
            };
            CryptoRandom rand = new CryptoRandom();
            List<char> chars = new List<char>();

            if (opts.RequireUppercase)
                chars.Insert(rand.Next(0, chars.Count), randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (opts.RequireLowercase)
                chars.Insert(rand.Next(0, chars.Count), randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (opts.RequireDigit)
                chars.Insert(rand.Next(0, chars.Count), randomChars[2][rand.Next(0, randomChars[2].Length)]);

            if (opts.RequireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count), randomChars[3][rand.Next(0, randomChars[3].Length)]);

            var randomCharList = new List<string>();
            if (opts.RequireUppercase)
                randomCharList.Add("ABCDEFGHJKLMNOPQRSTUVWXYZ");
            if (opts.RequireLowercase)
                randomCharList.Add("abcdefghijkmnopqrstuvwxyz");
            if (opts.RequireDigit)
                randomCharList.Add("0123456789");
            if (opts.RequireNonAlphanumeric)
                randomCharList.Add("!@$?_-");

            randomChars = randomCharList.ToArray();

            for (int i = chars.Count; i < opts.RequiredLength || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count), rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }
        public static string GenerateRandomPassword(int maxSize, bool isNumeric = false)
        {
            char[] chars = new char[62];
            if (!isNumeric)
                chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            else
                chars = "1234567890".ToCharArray();

            byte[] data = new byte[1];
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
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
            Random r = new Random();
            string strRsayi = r.Next(1, 10000000).ToString() + String.Format("{0:T}", DateTime.Now).Replace(":", string.Empty);
            return strRsayi;
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
            byte[] bytes = null;
            object clonedObject = null;
            var formatter = new BinaryFormatter();
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    formatter.Serialize(stream, Original);
                    bytes = stream.ToArray();
                    stream.Close();
                }
                using (MemoryStream stream = new MemoryStream())
                {
                    clonedObject = formatter.Deserialize(stream);
                    stream.Close();
                }
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
            while (newResult.IndexOf("--") > -1)
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
