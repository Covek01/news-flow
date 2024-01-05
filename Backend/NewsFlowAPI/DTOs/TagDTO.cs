namespace NewsFlowAPI.DTOs
{
    public class TagDTO
    {
        public string Name { get; set; } = String.Empty;

        public List<NewsDTO> News { get; set; } = new List<NewsDTO>();
    }
}
