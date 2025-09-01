using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ophelia.Application
{
    public class ApplicationClient : IDisposable
    {
        [ThreadStatic]
        protected static ApplicationClient? _Current;
        public static ApplicationClient Current => _Current;

        public decimal InstanceID { get; private set; }
        public string ApplicationName { get; set; }
        public Dictionary<string, object> SharedData { get; set; }
        public string ComputerName => System.Net.Dns.GetHostName();
        public virtual string UserHostAddress { get; set; }

        public virtual string UserAgent { get; set; }
        public virtual string SessionID { get; set; }
        public virtual int CurrentLanguageID { get; set; }

        public virtual string TranslateText(string Text)
        {
            return Text;
        }
        public virtual string GetImagePath(string path, bool forListing = true)
        {
            return path;
        }
        public virtual void Disconnect()
        {
            _Current = null;
            this.SharedData = null;
        }
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.Disconnect();
            Debug.WriteLine("Client.Dispose ManagedThreadId: " + Environment.CurrentManagedThreadId);
        }

        public ApplicationClient()
        {
            _Current = this;

            var rnd = new Random();
            this.InstanceID = rnd.Next(int.MaxValue);
            this.SharedData = new Dictionary<string, object>();
        }
    }
}