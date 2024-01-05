

using NewsFlowAPI.Models;

namespace NewsFlowAPI.DTOs
{
    public class UserDTO 
    {
        public string Name { get; set; } = String.Empty;
        public string Email { get; set; } = String.Empty;
        public string Phone { get; set; } = String.Empty;

        public string ImageUrl { get; set; } = String.Empty;

        public string Country { get; set; } = String.Empty;

        public string City { get; set; } = String.Empty;

        public string Role { get; set; } = "User";

        public List<FollowsTag> FollowedTags { get; set; } = new List<FollowsTag>();
        public List<Location> Locations { get; set; } = new List<Location>();
        public List<UserDTO> SubscribedTo { get; set; } = new List<UserDTO>();

        public List<NewsDTO> NewsWritten { get; set; } = new List<NewsDTO>();
    }
}
