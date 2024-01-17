namespace NewsFlowAPI.DTOs
{
    public class NewsRedisStorageDTO
    {
        public long Id { get; set; }
        public string Title { get; set; } = String.Empty;
        public string Summary { get; set; } = String.Empty;
        public string Text { get; set; } = String.Empty;
        public string ImageUrl { get; set; } = String.Empty;
        public long authorId { get; set; }
        public long? locationId { get; set; }
        public int? likeCount { get; set; }
        public int? viewsCount { get; set; }
        public DateTime PostTime { get; set; }
    }
}
