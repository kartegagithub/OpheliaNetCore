using System.Text.Json.Serialization;

namespace Ophelia.Data.Model.Proxy
{
    public interface ITrackedEntity
    {
        [System.Xml.Serialization.XmlIgnore]
        [JsonIgnore]
        PocoEntityTracker Tracker { get; set; }
    }
}
