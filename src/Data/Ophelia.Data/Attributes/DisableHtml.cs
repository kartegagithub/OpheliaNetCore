namespace Ophelia.Data.Attributes
{
    public class DisableHtml : AllowHtml
    {
        public DisableHtml()
        {
            this.Sanitize = false;
            this.Forbidden = true;
        }
    }
}