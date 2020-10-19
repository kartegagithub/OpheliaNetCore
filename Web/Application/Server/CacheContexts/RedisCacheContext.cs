using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Newtonsoft.Json;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Newtonsoft;

namespace Ophelia.Web.Application.Server.CacheContexts
{
    public class RedisCacheContext : ICacheContext
    {
        internal RedisCacheClient CacheClient { get; set; }
        public string HostName { get; set; }
        public RedisConfiguration Configuration { get; set; }
        public int Port { get; set; }
        public int ConnectRetry { get; set; }
        private bool IsConnected { get; set; }
        public int CurrentDBIndex { get; set; }

        public RedisCacheContext()
        {
            this.ConnectRetry = 5;
        }
        public void Connect()
        {
            try
            {
                if (this.IsConnected)
                    return;

                var serializer = new NewtonsoftSerializer();
                if (this.Configuration == null)
                    this.Configuration = new RedisConfiguration()
                    {
                        Database = 0
                    };

                this.CacheClient = new RedisCacheClient(new RedisCacheConnectionPoolManager($"{this.HostName}:{this.Port},defaultDatabase=0,abortConnect=false,connectRetry={this.ConnectRetry}"), serializer, this.Configuration);

                this.IsConnected = true;
            }
            catch (RedisConnectionException err)
            {
                this.IsConnected = false;
                throw err;
            }
        }

        public long CacheCount { get { return this.GetDb().SearchKeysAsync("*").Result.Count(); } }

        public bool Add(string key, object value, DateTime absoluteExpiration)
        {
            this.SetItem(key, value);
            return true;
        }

        public bool Add(string keyGroup, string keyItem, object value, DateTime absoluteExpiration)
        {
            return false;
        }

        public bool ClearAll()
        {
            this.GetDb().FlushDbAsync();
            return true;
        }

        public object Get(string key)
        {
            return this.GetItem(key);
        }

        public T Get<T>(string key)
        {
            return this.GetItem<T>(key);
        }

        public object Get(string keyGroup, string keyItem)
        {
            return null;
        }

        public List<string> GetAllKeys()
        {
            return this.GetDb().SearchKeysAsync("*").Result.ToList();
        }

        public bool Remove(string key)
        {
            var result = this.GetDb().RemoveAsync(key).Result;
            return true;
        }

        internal IRedisDatabase GetDb()
        {
            return this.CacheClient.GetDb(this.CurrentDBIndex);
        }
        public void SetItem(string key, object value)
        {
            var result = this.GetDb().AddAsync(key, ToByteArray(value)).Result;
        }

        public object GetItem(string key)
        {
            var objResult = FromByteArray(this.GetDb().GetAsync<byte[]>(key).Result);
            return objResult;
        }

        public T GetItem<T>(string key)
        {
            var objResult = FromByteArray<T>(this.GetDb().GetAsync<byte[]>(key).Result);
            return objResult;
        }

        public byte[] ToByteArray(object obj)
        {
            if (obj == null)
                return null;

            return System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore }));
            //var bf = new BinaryFormatter();
            //using (MemoryStream ms = new MemoryStream())
            //{
            //    bf.Serialize(ms, obj);
            //    return ms.ToArray();
            //}
        }
        public T FromByteArray<T>(byte[] data)
        {
            if (data == null)
                return default(T);

            return JsonConvert.DeserializeObject<T>(System.Text.Encoding.UTF8.GetString(data));
            //var bf = new BinaryFormatter();
            //using (MemoryStream ms = new MemoryStream(data))
            //{
            //    object obj = bf.Deserialize(ms);
            //    return obj;
            //}
        }
        public object FromByteArray(byte[] data)
        {
            if (data == null)
                return default(object);

            return JsonConvert.DeserializeObject(System.Text.Encoding.UTF8.GetString(data));
            //var bf = new BinaryFormatter();
            //using (MemoryStream ms = new MemoryStream(data))
            //{
            //    object obj = bf.Deserialize(ms);
            //    return obj;
            //}
        }
    }

    public class RedisCacheConnectionPoolManager : IRedisCacheConnectionPoolManager
    {
        private StackExchange.Redis.ConnectionMultiplexer _redis;
        public RedisCacheConnectionPoolManager(string connStr)
        {
            this._redis = StackExchange.Redis.ConnectionMultiplexer.Connect(connStr);
        }
        public void Dispose()
        {

        }

        public StackExchange.Redis.IConnectionMultiplexer GetConnection()
        {
            return this._redis;
        }
    }
}
