using System.ComponentModel;

namespace Ophelia.Web.View.Mvc.Models
{
    public class AlertMessageModel
    {
        public AlertMessageModel()
        {
        }

        public AlertMessageModel(string message, string type)
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

    public class MessageType
    {

        public static readonly string Block = "block";

        public static readonly string Success = "success";

        public static readonly string Info = "info";

        public static readonly string Error = "error";

        public static readonly string ErrorLoginPopup = "error_loginpopup";

        public static readonly string InfoLoginPopup = "info_loginpopup";

        public static readonly string SuccessLoginPopup = "success_loginpopup";
    }
}
