namespace NewsFlow.Server.Models
{
    public class FollowsTag
    {
        public User? User { get; set; }
        public Tag? Tag { get; set; }

        public int ViewCount { get; set; }
        public int LikeCount { get; set; }

        public int CommentCount { get; set; }

        public double InterestCoefficient { get; set; }

    }
}
