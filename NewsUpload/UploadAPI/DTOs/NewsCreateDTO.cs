
namespace NewsFlowAPI.DTOs
{
    public class NewsCreateDTO
    {
        public string Title { get; set; } = String.Empty;
        public string Summary { get; set; } = String.Empty;
        public string Text { get; set; } = String.Empty;
        public string ImageUrl { get; set; } = String.Empty;
        public List<long> tagsIds { get; set; } = new List<long>();
        public int authorId { get; set; }
        public int locationId { get; set; }
    }
}
