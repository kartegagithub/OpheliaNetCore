using Ophelia.Cryptography;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Ophelia
{
    public static class StringExtensions
    {
        private static readonly Encoding Encoding = Encoding.GetEncoding("Cyrillic");
        public static string ReplaceSpecialVowelsAndConsonant(this string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return value.ToLowerInvariant()
                        .Replace(" ", "")
                        .Replace("ı", "i")
                        .Replace("ğ", "g")
                        .Replace("ü", "u")
                        .Replace("ö", "o")
                        .Replace("ş", "s")
                        .Replace("ç", "c");
            }
            return "";
        }
        public static string RemoveExtraWhitespaces(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            while (value.IndexOf("  ") > -1)
            {
                value = value.Replace("  ", " ");
            }
            return value;
        }
        public static string Sanitize(this string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                var sanitizer = new Ganss.XSS.HtmlSanitizer();
                return sanitizer.Sanitize(value);
            }
            else
                return "";
        }
        public static string EncodeURL(this string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return System.Text.Encodings.Web.UrlEncoder.Default.Encode(value);
            }
            else
                return "";
        }
        public static string EncodeJavascript(this string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return System.Text.Encodings.Web.JavaScriptEncoder.Default.Encode(value);
            }
            else
                return "";
        }
        public static string EncodeHTML(this string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return System.Text.Encodings.Web.HtmlEncoder.Default.Encode(value);
            }
            else
                return "";
        }
        public static string CheckForInjection(string value)
        {
            if (!string.IsNullOrEmpty(value))
                value = value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("/*", "").Replace("*/", "").Replace("\"", "&quot;");
            return value;
        }
        public static object ArrangeStringAgainstRiskyChar(this string value)
        {
            if (!string.IsNullOrEmpty(value))
                value = value.Replace("<", "").Replace(">", "").Replace("(", "").Replace(")", "").Replace(";", "").Replace("&", "").Replace("+", "").Replace("%", "").Replace("#", "").Replace("$", "").Replace("\\", "").Replace("*", "").Replace("|", "").Replace("'", "").Replace("script", "");
            return value;
        }
        public static string RemoveHTML(this string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                value = value.Replace("<br>", "\n");
                value = value.Replace("<br/>", "\n");
                value = value.Replace("<br />", "\n");
                value = Regex.Replace(value, "<.*?>", string.Empty);
                value = Regex.Replace(value, "&nbsp;", " ");
                return value;
            }
            else
                return "";
        }
        public static string Right(this string value, int length)
        {
            return value.Substring(Math.Max(0, value.Length - length));
        }

        public static string Left(this string value, int length)
        {
            return value.Substring(0, Math.Min(length, value.Length));
        }

        public static bool IsNumeric(this string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                value = value.Trim();
                if (value.Equals("0"))
                    return true;

                decimal decValue = 0;
                decimal.TryParse(value, out decValue);
                return decValue > 0;
            }
            return false;
        }
        public static bool IsValidEmail(this string value)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(value);
                return addr.Address == value;
            }
            catch
            {
                return false;
            }
        }
        public static bool IsEmailAddress(this string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                string format = @"^[a-zA-Z0-9_\-\.]+@[a-zA-Z0-9_\-\.]+\.[a-zA-Z]{2,}$";
                Regex regex = new Regex(format);
                if (!regex.IsMatch(value))
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public static bool In(this string value, params string[] stringValues)
        {
            foreach (string comparedValue in stringValues)
                if (string.Compare(value, comparedValue) == 0)
                    return true;

            return false;
        }

        public static string Format(this string value, params object[] args)
        {
            return string.Format(value, args);
        }

        public static bool IsMatch(this string value, string pattern)
        {
            Regex regex = new Regex(pattern);
            return value.IsMatch(regex);
        }

        public static bool IsMatch(this string value, Regex regex)
        {
            return regex.IsMatch(value);
        }

        public static string ChangeInvalidSpaces(this string value)
        {
            return value.Replace((char)160, (char)32);
        }
        /// <summary>
        /// 64 digit tabanlı string veriyi 8 bitlik unsigned integer diziye dönüştürür.
        /// </summary>
        public static byte[] ToByteArray(this string text)
        {
            return System.Convert.FromBase64String(text);
        }

        /// <summary>
        /// 8 bitlik unsigned integer diziyi 64 digit tabanlı string'e dönüştürür.
        /// </summary>
        public static string ToText(this byte[] buffer)
        {
            return System.Convert.ToBase64String(buffer);
        }

        public static Nullable<T> ToNullable<T>(this string source) where T : struct
        {

            Nullable<T> result = new Nullable<T>();
            try
            {
                if (!string.IsNullOrEmpty(source) && source.Trim().Length > 0)
                {
                    TypeConverter conv = TypeDescriptor.GetConverter(typeof(T));
                    result = (T)conv.ConvertFromInvariantString(source);
                }
            }
            catch { return result; }
            return result;
        }

        public static string ToCamelCase(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return char.ToLowerInvariant(value[0]) + value.Substring(1);
        }

        /// <summary>
        /// Decimal'i belirtilen formata ve kültürel bilgiye göre string'e çevirir.
        /// </summary>
        public static string ToPriceString(this decimal source)
        {
            return source.ToPriceString("tr-TR");
        }

        public static string ToPriceString(this decimal source, string cultureInfo)
        {
            return source.ToString("F", CultureInfo.GetCultureInfo(cultureInfo));
        }

        /// <summary>
        /// Decimal'i belirtilen kültürel bilgiye göre string'e çevirir.
        /// </summary>
        public static string ToPriceString(this string source)
        {
            return source.ToPriceString("tr-TR");
        }

        public static string ToPriceString(this string source, string cultureInfo)
        {
            return source.ToString(System.Globalization.CultureInfo.GetCultureInfo(cultureInfo));
        }

        /// <summary>
        /// Ondalıklı sayı biçimindeki string ifadedyi bir üst sayıya yuvarlayıp, sayının tam sayı kısmını alır.
        /// </summary>
        public static string ToRoundedValue(this string value)
        {
            var left = System.Math.Round(value.ToDecimal());
            return string.Format("{0:0}", left);
        }

        public static Int16 ToInt16(this string value)
        {
            Int16 result = 0;

            if (!string.IsNullOrEmpty(value))
                Int16.TryParse(value, out result);

            return result;
        }

        public static Int32 ToInt32(this string value)
        {
            Int32 result = 0;

            if (!string.IsNullOrEmpty(value))
                Int32.TryParse(value, out result);

            return result;
        }

        public static long ToInt64(this string value)
        {
            Int64 result = 0;

            if (!string.IsNullOrEmpty(value))
                Int64.TryParse(value, out result);

            return result;
        }

        public static byte ToByte(this string value)
        {
            byte result = 0;

            if (!string.IsNullOrEmpty(value))
                byte.TryParse(value, out result);

            return result;
        }
        public static bool ToBoolean(this string value)
        {
            if (value.IsNumeric())
                return value.ToInt64() > 0;
            else if (value.ToLowerInvariant().Equals("true"))
                return true;
            else if (value.ToLowerInvariant().Equals("yes"))
                return true;
            return false;
        }
        public static List<long> ToLongList(this string value, params char[] seperator)
        {
            if (seperator == null || seperator.Length == 0)
                seperator = new char[] { ',' };

            if (string.IsNullOrEmpty(value))
                return new List<long>();
            try
            {
                return value.ToString().Split(seperator, StringSplitOptions.RemoveEmptyEntries).Select(i => long.Parse(i)).ToList();
            }
            catch (Exception)
            {

                var list = new List<long>();
                var splitted = value.Split(seperator);
                foreach (var item in splitted)
                {
                    long tmpLong = 0;
                    if (long.TryParse(item, out tmpLong))
                    {
                        list.Add(tmpLong);
                    }
                }
                return list;
            }
        }

        public static List<int> ToIntList(this string value, char seperator = ',')
        {
            if (string.IsNullOrEmpty(value))
                return new List<int>();

            return value.ToString().Split(seperator).Select(i => int.Parse(i)).ToList();
        }

        public static List<byte> ToByteList(this string value, char seperator = ',')
        {
            if (string.IsNullOrEmpty(value))
                return new List<byte>();

            return value.ToString().Split(seperator).Select(i => byte.Parse(i)).ToList();
        }

        public static List<decimal> ToDecimalList(this string value, char seperator = ',')
        {
            if (string.IsNullOrEmpty(value))
                return new List<decimal>();

            return value.ToString().Split(seperator).Select(i => decimal.Parse(i)).ToList();
        }
        /// <summary>
        /// Ondalıklı sayı biçimindeki string ifadeyi kontrollü biçimde decimal değerine çevirir.
        /// </summary>
        public static decimal ToDecimal(this string value)
        {
            try
            {
                var numberFormat = new NumberFormatInfo();
                numberFormat.NumberDecimalSeparator = ",";
                return Convert.ToDecimal(value.Replace(".", ","), numberFormat);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static T ToEnum<T>(this string name) where T : struct
        {
            if (Enum.IsDefined(typeof(T), name))
                return (T)Enum.Parse(typeof(T), name, true);
            else return default(T);
        }

        public static string ToDashCase(this string input)
        {
            string pattern = "[A-Z]";
            string dash = "-";
            return Regex.Replace(input, pattern,
                m => ((m.Index == 0) ? string.Empty : dash) + m.Value.ToLowerInvariant());
        }

        public static string ToStringSlug(this string value)
        {

            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var str = String.Join("", value.Normalize(NormalizationForm.FormD)
            .Where(c => char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));

            str = value.RemoveAccent().ToLowerInvariant();
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = str.Substring(0, str.Length <= 200 ? str.Length : 200).Trim();
            str = Regex.Replace(str, @"\s", "-");
            str = Regex.Replace(str, @"-+", "-");
            return str;
        }

        /// <summary>
        /// Splits to lines. Every line has max 'length' size chars.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static List<string> SplitToLines(this string value, int length)
        {
            var splitted = new List<string>();
            while (value.Length > 0)
            {
                if (value.Length <= length)
                    splitted.Add(value);
                else
                    splitted.Add(value.Substring(0, Math.Min(value.Length, length)));

                value = value.Remove(0, splitted.LastOrDefault().Length);
            }
            return splitted;
        }

        /// <summary>
        /// Splits by pattern.
        /// </summary>
        public static string[] SplitString(this string value, string regexPattern, int maxLength)
        {
            string[] splitted = new string[3];

            if (string.IsNullOrEmpty(value))
                return splitted;

            value = value.Trim();

            if (value.Length > maxLength)
                value = value.Substring(0, maxLength);

            Match matchResults = null;
            Regex paragraphs = new Regex(regexPattern, RegexOptions.Singleline);
            matchResults = paragraphs.Match(value);
            if (matchResults.Success)
            {
                splitted[0] = matchResults.Groups[1].Value;
                splitted[1] = matchResults.Groups[2].Value;
                splitted[2] = matchResults.Groups[3].Value;
            }

            return splitted;

        }

        public static string AddLeadingZeros(this long value, int totalLength)
        {
            return value.AddLeadingZeros(totalLength, string.Empty);
        }

        public static string AddLeadingZeros(this long value, int totalLength, string prefix)
        {
            totalLength = totalLength - prefix.Length;
            return prefix + value.ToString().PadLeft(totalLength, '0');
        }

        public static string AddLeadingZeros(this string value, int totalLength, string prefix)
        {
            totalLength = totalLength - prefix.Length;
            return prefix + value.ToString().PadLeft(totalLength, '0');
        }

        public static string AddLeadingZeros(this int value, int totalLength, string prefix)
        {
            return ((long)value).AddLeadingZeros(totalLength, prefix);
        }

        public static string AddLeadingZeros(this byte value, int totalLength, string prefix)
        {
            return ((long)value).AddLeadingZeros(totalLength, prefix);
        }

        /// <summary>
        /// Clear metodunu çalıştırarak kaynak içindeki belirtilen karakterleri siler.
        /// </summary>
        public static string Clear(this string source)
        {
            if (string.IsNullOrEmpty(source)) return string.Empty;
            return source.Trim('_').Clear(new[] { ' ', '(', ')', '_', '-' });
        }

        /// <summary>
        /// Belirtilen karakterleri kaldırır string'den.
        /// </summary>
        public static string Clear(this string source, params char[] removeChars)
        {
            var destination = source;
            if (!string.IsNullOrEmpty(destination))
            {
                var splittedData = source.Split(removeChars, StringSplitOptions.RemoveEmptyEntries);
                destination = string.Concat(splittedData);
            }

            return destination;
        }

        public static string RemoveAccent(this string value)
        {
            byte[] bytes = Encoding.GetBytes(value);
            return Encoding.ASCII.GetString(bytes);
        }

        public static string Encrypt(this string chipperText, string encryptionKey = "")
        {
            return Cryptography.CryptoManager.Encrypt(chipperText, encryptionKey);
        }
        public static string Decrypt(this string richText, string decryptionKey = "")
        {
            return Cryptography.CryptoManager.Decrypt(richText, decryptionKey);
        }
        public static string ComputeHash(this string plainText, string saltText = "", HashAlgorithms algorithm = HashAlgorithms.SHA1, Encoding encoding = null)
        {
            return Cryptography.CryptoManager.ComputeHash(plainText, saltText, algorithm, encoding);
        }
        public static string GetMd5Hash(this string plainText, string saltText = "")
        {
            return Cryptography.CryptoManager.GetMd5Hash(plainText, saltText);
        }

        /// <summary>
        /// Özel türkçe harfleri latin harflere çevirir.
        /// </summary>
        public static string ClearTurkishChars(this string value)
        {
            StringBuilder sb = new StringBuilder(value);
            sb = sb.Replace("ı", "i");
            sb = sb.Replace("ğ", "g");
            sb = sb.Replace("ü", "u");
            sb = sb.Replace("ş", "s");
            sb = sb.Replace("ö", "o");
            sb = sb.Replace("ç", "c");
            sb = sb.Replace("İ", "I");
            sb = sb.Replace("Ğ", "G");
            sb = sb.Replace("Ü", "U");
            sb = sb.Replace("Ş", "S");
            sb = sb.Replace("Ö", "O");
            sb = sb.Replace("Ç", "C");
            sb = sb.Replace("'", string.Empty);

            return sb.ToString();
        }

        public static string EncodeTurkishChars(this string text)
        {
            text = text.Replace("İ", "\u0130");
            text = text.Replace("ı", "\u0131");
            text = text.Replace("Ş", "\u015e");
            text = text.Replace("ş", "\u015f");
            text = text.Replace("Ğ", "\u011e");
            text = text.Replace("ğ", "\u011f");
            text = text.Replace("Ö", "\u00d6");
            text = text.Replace("ö", "\u00f6");
            text = text.Replace("ç", "\u00e7");
            text = text.Replace("Ç", "\u00c7");
            text = text.Replace("ü", "\u00fc");
            text = text.Replace("Ü", "\u00dc");
            return text;
        }
        public static string ClearRequestParameter(this string param)
        {
            return param.Replace("\"", "");
        }
        public static string Pluralize(this string source)
        {
            var x = new Pluralize.NET.Pluralizer();
            return x.Pluralize(source);
        }
        public static string Singularize(this string source)
        {
            var x = new Pluralize.NET.Pluralizer();
            return x.Singularize(source);
        }
        public static bool HasValue(this string value, string[] array)
        {
            foreach (var item in array)
            {
                if (value.IndexOf(item, StringComparison.InvariantCultureIgnoreCase) > -1)
                    return true;
            }
            return false;
        }

        public static bool EndsWith(this string value, string[] array)
        {
            foreach (var item in array)
            {
                if (value.EndsWith(item, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }
        public static string CompressString(this string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            var memoryStream = new MemoryStream();
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gZipStream.Write(buffer, 0, buffer.Length);
            }

            memoryStream.Position = 0;

            var compressedData = new byte[memoryStream.Length];
            memoryStream.Read(compressedData, 0, compressedData.Length);

            var gZipBuffer = new byte[compressedData.Length + 4];
            Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
            return Convert.ToBase64String(gZipBuffer);
        }
        public static string DecompressString(this string compressedText)
        {
            byte[] gZipBuffer = Convert.FromBase64String(compressedText);
            using (var memoryStream = new MemoryStream())
            {
                int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
                memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

                var buffer = new byte[dataLength];

                memoryStream.Position = 0;
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }
        public static string RequestURL(this string strHostAddress, string data = "", string method = "POST", string contentType = "multipart/form-data")
        {
            HttpWebRequest _WebRequest = (HttpWebRequest)WebRequest.Create(strHostAddress);
            Stream dataStream = null;
            _WebRequest.Method = method;
            _WebRequest.KeepAlive = false;
            _WebRequest.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; ru; rv:1.9.2.3) Gecko/20100401 Firefox/4.0 (.NET CLR 3.5.30729) Ophelia";
            _WebRequest.Accept = "*/*;q=0.8";

            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            _WebRequest.ContentType = contentType;

            if (!string.IsNullOrEmpty(data))
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(data);
                _WebRequest.ContentLength = byteArray.Length;

                dataStream = _WebRequest.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
            }

            HttpWebResponse _WebResponse = (HttpWebResponse)_WebRequest.GetResponse();

            if (_WebResponse.StatusCode == HttpStatusCode.OK || _WebResponse.StatusCode == HttpStatusCode.BadGateway)
            {
                Encoding Encoding;
                if (string.IsNullOrEmpty(_WebResponse.CharacterSet))
                    Encoding = Encoding.Default;
                else
                {
                    Encoding = Encoding.GetEncoding(_WebResponse.CharacterSet);
                }
                return new StreamReader(_WebResponse.GetResponseStream(), Encoding).ReadToEnd();
            }

            return "";
        }

        public static string RequestURL(this string strHostAddress, Dictionary<string, object> dictionary)
        {
            var NVCdata = dictionary.Aggregate(new System.Collections.Specialized.NameValueCollection(), (seed, current) => { seed.Add(current.Key, Convert.ToString(current.Value)); return seed; });
            return strHostAddress.RequestURL(NVCdata);
        }

        public static string RequestURL(this string strHostAddress, List<KeyValuePair<string, object>> values)
        {
            var NVCdata = values.Aggregate(new System.Collections.Specialized.NameValueCollection(), (seed, current) => { seed.Add(current.Key, Convert.ToString(current.Value)); return seed; });
            return strHostAddress.RequestURL(NVCdata);
        }

        public static string RequestURL(this string strHostAddress, System.Collections.Specialized.NameValueCollection data)
        {
            using (var webClient = new WebClient())
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                return Encoding.UTF8.GetString(webClient.UploadValues(strHostAddress, data));
            }
        }
        public static string Remove(this string val, string[] itemsToRemove)
        {
            if (string.IsNullOrEmpty(val))
                return "";
            foreach (var item in itemsToRemove)
            {
                val = val.Replace(item, "");
            }
            return val;
        }
        public static string Remove(this string val, char[] itemsToRemove)
        {
            if (string.IsNullOrEmpty(val))
                return "";
            foreach (var item in itemsToRemove)
            {
                val = val.Replace(Convert.ToString(item), "");
            }
            return val;
        }
        public static List<Type> FindTypeInAssemblies(this string val, List<Assembly> assemblies)
        {
            var types = new List<Type>();
            foreach (var item in assemblies)
            {
                types.AddRange(item.GetTypes().Where(op => op.Name == val).ToList());
            }
            return types;
        }
        public static bool IsBase64String(this string base64)
        {
            //https://stackoverflow.com/questions/6309379/how-to-check-for-a-valid-base64-encoded-string
            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out int _);
        }
        public static bool IsGuid(this string inputString)
        {
            Guid guidOutput;
            return Guid.TryParse(inputString, out guidOutput);
        }
        public static string RemoveNonNumericChars(this string value)
        {
            if (!string.IsNullOrEmpty(value))
                value = value.Replace(" ", "").Replace("(", "").Replace(")", "").Replace("-", "");
            return value;
        }
        /// <summary>
        /// QueryableDataSet Full Text Search Support
        /// </summary>
        /// <param name="val"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static bool ContainsFTS(this string val, params string[] parameters)
        {
            return true;
        }
    }
}
