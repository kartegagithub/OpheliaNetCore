using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ophelia.Web.View.Mvc.Models
{
    public class AlertMessageModel
    {
        public AlertMessageModel()
        {
        }

        public AlertMessageModel(string message , string type)
        {
            Type = type;
            Message = message;
            Show = true;
            Closeable = false;
        }

        [DefaultValue(true)]
        public bool Show { get; set; }

        [DefaultValue("block")]
        public string Type { get; set; }

        [DefaultValue(true)]
        public bool Closeable { get; set; }

        [DefaultValue("Mesaj içeriği yok")]
        public string Message { get; set; }

    }

    public class MessageType{

        public static string Block = "block";

        public static string Success = "success";

        public static string Info = "info";

        public static string Error = "error";

        public static string ErrorLoginPopup = "error_loginpopup";

        public static string InfoLoginPopup = "info_loginpopup";

        public static string SuccessLoginPopup = "success_loginpopup";
    }
}
