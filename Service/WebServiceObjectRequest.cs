﻿using System.Collections.Generic;

namespace Ophelia.Service
{
    public class WebServiceObjectRequest
    {
        public long ID { get; set; }
        public string TypeName { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public WebServiceObjectRequest AddParam(string key, object value)
        {
            this.Parameters[key] = value;
            return this;
        }
        public WebServiceObjectRequest()
        {
            this.Parameters = new Dictionary<string, object>();
        }
    }
}
