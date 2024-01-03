namespace NewsFlow.Server.Models
{
    public class Comment
    {
        public long Id { get; set; }
        public string Text { get; set; }=String.Empty;
        public User? Author { get; set; }
        public News? News { get; set; }
        public int LikeCount { get; set; }

    }
}
