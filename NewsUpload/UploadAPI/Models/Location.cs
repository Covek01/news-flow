namespace NewsFlowAPI.Models
{
    public class Location
    {
        public long Id { get; set; }
        public string Name { get; set; } = String.Empty;
        public List<News> NewsAtLocation = new List<News>();
    }
}
