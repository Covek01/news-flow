using System.Text.Json.Serialization;

namespace NewsFlow.Server.Models
{

    public class User
    {
        public long Id { get; set; }
        public string Name { get; set; } = String.Empty;
        public string Email { get; set; }=String.Empty;
        public string Phone { get; set; } = String.Empty;

        public string ImageUrl { get; set; } = String.Empty;
        [JsonIgnore]
        public string PasswordHash {  get; set; } = String.Empty;

        public bool EmailConfirmed { get; set; } = false;

        public string Country { get; set; } = String.Empty;

        public string City { get; set; } = String.Empty;

        public string Role { get; set; } = "User";

        public List<FollowsTag> FollowedTags { get; set; } = new List<FollowsTag>();  
        public List<Location> Locations { get; set; } = new List<Location>();
        public List<User> SubscribedTo { get; set; } = new List<User>();

        public List<News> NewsWritten { get; set; }=new List<News>();

        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}
