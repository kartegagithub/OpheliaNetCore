﻿using NJsonSchema.Annotations;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ophelia.Service
{
    public class ServiceObjectRequest<T> : IDisposable
    {
        public long ID { get; set; }
        public long LanguageID { get; set; }
        public string Name { get; set; }
        public virtual T Data { get; set; }
        public string TypeName { get; set; }

        [OpenApiIgnore]
        [JsonSchemaIgnore]
        public Dictionary<string, object> Parameters { get; set; }

        [OpenApiIgnore]
        [JsonSchemaIgnore]
        public List<FileData> Files { get; set; }

        public ServiceObjectRequest<T> AddParam(string key, object value)
        {
            this.Parameters[key] = value;
            return this;
        }
        public FileData? GetFile(string Key)
        {
            return this.Files?.Where(op => op.KeyName == Key).FirstOrDefault();
        }
        public ServiceObjectRequest()
        {
            this.Parameters = new Dictionary<string, object>();
            this.Files = new List<FileData>();
            this.TypeName = typeof(T).FullName;
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.Parameters = null;
            this.Files = null;
        }
    }

    public class FileData
    {
        public long ID { get; set; }
        public long StatusID { get; set; }
        public long LanguageID { get; set; }
        private byte[] oByteData;
        public string KeyName { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public byte[] ByteData
        {
            get
            {
                if (this.oByteData == null && !string.IsNullOrEmpty(this.Base64Data))
                {
                    try
                    {
                        if (this.Base64Data.IndexOf(',') > -1)
                            this.Base64Data = this.Base64Data.Substring(this.Base64Data.IndexOf(',') + 1);
                        this.oByteData = Convert.FromBase64String(this.Base64Data);
                        this.Base64Data = "";
                        if (this.oByteData != null)
                            this.FileSize = this.oByteData.Length;
                    }
                    catch
                    {
                        return this.oByteData;
                    }
                }
                return this.oByteData;
            }
            set
            {
                this.oByteData = value;
                if (value != null)
                    this.FileSize = value.Length;
            }
        }
        public string Base64Data { get; set; }
    }
}
