using Microsoft.VisualBasic;
using System.Collections;
using System.Web;
using System;
using Ophelia;
using Microsoft.AspNetCore.Http;

namespace Ophelia.Web.Application.Client
{
    public class QueryString
    {
        private SortedList oInnerList;
        public int ItemCount = 0;
        private bool ValueCreated = false;
        private string sValue = "";
        private IList oKeyList;
        private IList oValueList;
        private string sScriptName = "";
        private string sRawUrl = "";
        public HttpRequest Request { get; set; }
        public SortedList InnerList
        {
            get { return this.oInnerList; }
        }
        public string ScriptName
        {
            get { return this.sScriptName; }
        }
        public string RawUrl
        {
            get { return this.sRawUrl; }
        }
        public object Item(string Key)
        {
            return this.InnerList[Key];
        }
        public IList KeyList
        {
            get
            {
                if (this.oKeyList == null)
                    this.oKeyList = this.InnerList.GetKeyList();
                return this.oKeyList;
            }
        }
        public IList ValueList
        {
            get
            {
                if (this.oValueList == null)
                    this.oValueList = this.InnerList.GetValueList();
                return this.oValueList;
            }
        }
        public string Value
        {
            get
            {
                this.Create();
                return this.sValue;
            }
        }
        public void Update(string Identifier, string Value)
        {
            if (string.IsNullOrEmpty(Value))
                Value = "";
            if (this.InnerList[Identifier] == null)
            {
                this.Add(Identifier, Value);
            }
            else
            {
                this.ValueCreated = false;
                this.InnerList[Identifier] = Value;
            }
        }
        public void Remove(string Identifier)
        {
            if ((this.InnerList[Identifier] != null))
            {
                this.ValueCreated = false;
                this.InnerList.Remove(Identifier);
                ItemCount -= 1;
            }
        }
        public void Add(string Identifier, string Value)
        {
            if (this.InnerList[Identifier] == null)
            {
                this.ValueCreated = false;
                this.InnerList[Identifier] = Value;
                ItemCount += 1;
            }
            else
            {
                this.Update(Identifier, Value);
            }
        }
        public void Create()
        {
            if (!this.ValueCreated)
            {
                int n = 0;
                this.sValue = this.ScriptName;
                if (this.sValue.IndexOf('?') < 0)
                    this.sValue += "?";
                for (n = 0; n <= this.ItemCount - 1; n++)
                {
                    if (Convert.ToString(this.ValueList[n]) == "true,false" || Convert.ToString(this.ValueList[n]) == "true,true")
                    {
                        this.sValue += this.KeyList[n] + "=true&";
                    }
                    else if (Convert.ToString(this.ValueList[n]) == "false,true" || Convert.ToString(this.ValueList[n]) == "false,false")
                    {
                        this.sValue += this.KeyList[n] + "=false&";
                    }
                    else
                        this.sValue += this.KeyList[n] + "=" + this.ValueList[n] + "&";
                }
                if (this.sValue.Length - 1 == this.sValue.LastIndexOf('&'))
                    this.sValue = this.sValue.Left(this.sValue.Length - 1);
                this.ValueCreated = true;
            }
        }
        private void AddExistingIdentifiers()
        {
            if ((this.Request != null))
            {
                int n = 0;
                foreach (var key in this.Request.Query.Keys)
                {
                    if (!string.IsNullOrEmpty(this.Request.GetValue(key)))
                    {
                        if (this.InnerList[key] == null)
                            ItemCount += 1;
                        this.InnerList[key] = this.Request.GetValue(key);
                    }
                    n++;
                }
                if(this.Request.Method == "POST")
                {
                    foreach (var key in this.Request.Form.Keys)
                    {
                        if (!string.IsNullOrEmpty(this.Request.GetValue(key)))
                        {
                            if (this.InnerList[key] == null)
                                ItemCount += 1;
                            this.InnerList[key] = this.Request.GetValue(key);
                        }
                    }
                }
                this.sRawUrl = this.Request.RawUrl();
            }
            else if (!string.IsNullOrEmpty(this.RawUrl))
            {
                if (this.RawUrl.IndexOf('?') > -1)
                {
                    string[] sIdentifiers = this.RawUrl.Right(this.RawUrl.Length - this.RawUrl.IndexOf('?') - 1).Split('&');
                    int n = 0;
                    string Key = "";
                    string Value = "";
                    while (!(n > sIdentifiers.Length - 1))
                    {
                        Value = "";
                        Key = "";
                        if (sIdentifiers[n].IndexOf('=') > -1)
                        {
                            Key = sIdentifiers[n].Left(sIdentifiers[n].IndexOf('='));
                        }
                        if (sIdentifiers[n].Length - sIdentifiers[n].IndexOf('=') - 1 > -1)
                        {
                            Value = sIdentifiers[n].Right(sIdentifiers[n].Length - sIdentifiers[n].IndexOf('=') - 1);
                        }
                        if (!string.IsNullOrEmpty(Key))
                            this.Add(Key, Value);
                        n += 1;
                    }
                }
            }
            if (this.RawUrl.IndexOf('?') > -1)
            {
                this.sScriptName = this.RawUrl.Left(this.RawUrl.IndexOf('?'));
            }
            else
            {
                this.sScriptName = this.RawUrl;
            }
        }
        public QueryString()
        {
            this.oInnerList = new SortedList();
        }
        public QueryString(string RawUrl) : this()
        {
            this.sRawUrl = RawUrl;
            this.AddExistingIdentifiers();
        }

        public QueryString(HttpRequest Request) : this()
        {
            this.Request = Request;
            this.AddExistingIdentifiers();
        }

        public QueryString(string RawUrl, HttpRequest Request) : this(Request)
        {
            this.sRawUrl = RawUrl;
            this.AddExistingIdentifiers();
        }
    }
}
