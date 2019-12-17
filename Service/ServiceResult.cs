using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Ophelia.Reflection;

namespace Ophelia.Service
{
    [DataContract(IsReference = true)]
    public class ServiceResult: IDisposable
    {
        [DataMember]
        public ServicePerformance Performance { get; set; }

        public ServiceExceptionHandler Handler { get; set; }

        [DataMember]
        public Dictionary<string, object> ExtraData { get; set; }

        [DataMember]
        public List<ServiceResultMessage> Messages { get; set; }

        [DataMember]
        public string Token { get; set; }

        [DataMember]
        public bool HasFailed { get; set; }

        [DataMember]
        public bool IsRetrievedFromCache { get; set; }

        public void Fail()
        {
            this.HasFailed = true;
        }

        public void Fail(ServiceResult result)
        {
            this.Messages.AddRange(result.Messages);
            this.Fail();
        }
        public void Fail(List<ServiceResultMessage> Messages)
        {
            this.Messages.AddRange(Messages);
            this.Fail();
        }

        public void Fail(string code, string message)
        {
            this.Messages.Add(new ServiceResultMessage() { Code = code, Description = message, IsError = true });
            this.Fail();
        }
        public void Fail(string message)
        {
            this.Messages.Add(new ServiceResultMessage() { Code = "E1", Description = message, IsError = true });
            this.Fail();
        }
        public void Fail(Exception ex)
        {
            this.Messages.Add(new ServiceResultMessage() { Code = "E1", Description = ex.Message + " " + ex.StackTrace, IsError = true });
            this.Fail();
            try
            {
                if (this.Handler == null)
                {
                    this.Handler = (ServiceExceptionHandler)typeof(ServiceExceptionHandler).GetRealTypeInstance();
                }
                if (this.Handler != null)
                    this.Handler.HandleException(ex);
            }
            catch (Exception)
            {

            }
        }

        public void Fail(Exception ex, bool reportError, string entry, string fileName)
        {
            this.Fail(ex);
        }

        public virtual void Dispose()
        {
            this.ExtraData = null;
            this.Handler = null;
            this.Performance = null;
            this.Messages = null;
        }
        public ServiceResult()
        {
            this.Messages = new List<ServiceResultMessage>();
            this.Performance = new ServicePerformance();
            this.ExtraData = new Dictionary<string, object>();
        }
    }
}
