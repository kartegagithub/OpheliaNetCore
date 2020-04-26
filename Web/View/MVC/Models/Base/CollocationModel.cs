namespace Ophelia.Web.View.Mvc.Models
{
    public class CollocationModel
    {
        public Sortdirection Direction { get; set; }

        public enum Sortdirection : int
        {
            Ascending = 0,
            Descending = 1
        }
    }
}
