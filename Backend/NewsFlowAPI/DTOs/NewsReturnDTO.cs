
namespace NewsFlowAPI.DTOs
{
    public class NewsReturnDTO
    {
        public long Id { get; set; }
        public string Title { get; set; } = String.Empty;
        public string Summary { get; set; } = String.Empty;
        public string Text { get; set; } = String.Empty;
        public string ImageUrl { get; set; } = String.Empty;
        public List<long> tagsIds { get; set; } = new List<long>();
        public long authorId { get; set; }
        public string authorName { get; set; }
        public long? locationId { get; set; }
        public long ViewsCount { get; set; }
        public long LikeCount { get; set; }
        public DateTime PostTime { get; set; }
        
    }
}
