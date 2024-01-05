using System.Text.Json.Serialization;

namespace NewsFlowAPI.DTOs
{
    public class LocationDTO 
    {
        public string Name { get; set; } = String.Empty;
        [JsonIgnore]
        public List<NewsDTO> NewsAtLocation = new List<NewsDTO>();
    }
}
