using NewsFlowAPI.Models;

namespace NewsFlowAPI.DTOs
{
    public class NewsDTO
    {
        public string Title { get; set; } = String.Empty;
        public string Summary { get; set; } = String.Empty;
        public string Text { get; set; } = String.Empty;
        public string ImageUrl { get; set; } = String.Empty;
        public List<TagDTO> Tags { get; set; } = new List<TagDTO>();
        public UserDTO? Author { get; set; }
        public LocationDTO? Location { get; set; }
        public DateTime PostTime { get; set; }
    }
}
