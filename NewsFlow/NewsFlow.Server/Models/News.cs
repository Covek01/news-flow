namespace NewsFlow.Server.Models
{
    public class News
    {
        public long Id { get; set; }
        public string Title { get; set; } = String.Empty;
        public string Summary { get; set; } = String.Empty;

        public string Text { get; set; } = String.Empty;
        public string ImageUrl { get; set; } = String.Empty;

        public User? Author { get; set; }

        public List<Tag> Tags { get; set; } = new List<Tag>();

        public Location? Location { get; set; }
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public DateTime PostTime { get; set; }
        public int ViewsCount { get; set; }

        public int ViewsLastPeriod { get; set; }
        public int CommentsLastPeriod { get; set; }


    }
}
