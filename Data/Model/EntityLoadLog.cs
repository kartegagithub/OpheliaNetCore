namespace Ophelia.Data.Model
{
    public class EntityLoadLog
    {
        public string Type { get; set; }
        public double ListLoadDuration { get; set; }
        public double Count { get; set; }
        public double EntityLoadDuration { get; set; }
        public EntityLoadLog(string type)
        {
            this.Type = type;
        }
    }
}
