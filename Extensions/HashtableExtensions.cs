using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading.Tasks;

namespace Ophelia
{
    public static class HashtableExtensions
    {
        public static string ToQueryString(this Hashtable Hashtable)
        {
            var s = new StringBuilder();
            foreach (DictionaryEntry item in Hashtable)
            {
                s.Append(Convert.ToString(item.Key));
                s.Append("=");
                s.Append(HttpUtility.UrlEncode(Convert.ToString(item.Value)));
                s.Append("&");
            }
            return s.ToString();
        }

        public static string ToQueryString(this List<KeyValuePair<string, object>> list)
        {
            var s = new StringBuilder();
            foreach (var item in list)
            {
                s.Append(Convert.ToString(item.Key));
                s.Append("=");
                s.Append(HttpUtility.UrlEncode(Convert.ToString(item.Value)));
                s.Append("&");
            }
            return s.ToString();
        }

        public static object GetItem(this List<KeyValuePair<string, object>> list, string key)
        {
            return list.Where(op => op.Key == key).FirstOrDefault().Value;
        }

        public static void AddItem(this List<KeyValuePair<string, object>> list, string key, object value)
        {
            list.Add(new KeyValuePair<string, object>(key, value));
        }
        public static void UpdateItem(this List<KeyValuePair<string, object>> list, string key, object value)
        {
            var item = list.Where(op => op.Key == key).FirstOrDefault();

            if (default(KeyValuePair<string, object>).Equals(item))
                list.AddItem(key, value);
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if(list[i].Key == key)
                    {
                        list.RemoveAt(i);
                        list.Insert(i, new KeyValuePair<string, object>(key, value));
                    }
                }
            }
        }
    }
}
