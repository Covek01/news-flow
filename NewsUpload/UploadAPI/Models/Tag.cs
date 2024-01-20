namespace NewsFlowAPI.Models
{
    public class Tag
    {
        public long Id { get; set; }
        public string Name { get; set; } = String.Empty;

        public List<News> News { get; set; }=new List<News>();
    }
}
