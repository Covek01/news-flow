namespace NewsFlowAPI.DTOs
{
    public class NewsFilterDTO
    {
        public long AuthorId {  get; set; }
        public List<long> TagIds { get; set; }
        public long LocationId { get; set; }
    }
}
