using System;
using System.ComponentModel.DataAnnotations;

namespace Ophelia.Web.View.Mvc.Models
{
    public abstract class BaseModel : IDisposable
    {
        public BaseModel()
        {
            this.StatusID = "";
            this.ID = 0;
            this.AlertMessage = new AlertMessageModel();
        }

        [Display(Name = "ID")]
        public long ID { get; set; }

        public string StatusID { get; set; }

        public bool IsNew { get { return this.ID == 0; } }
        public AlertMessageModel AlertMessage { get; set; }

        public virtual void Dispose()
        {

        }
    }
}
