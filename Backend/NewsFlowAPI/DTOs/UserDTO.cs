

using NewsFlowAPI.Models;
using System.Text.Json.Serialization;

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

        //public List<FollowsTagDTO> FollowedTags { get; set; } = new List<FollowsTagDTO>();
        public List<LocationDTO> Locations { get; set; } = new List<LocationDTO>();
        public List<UserDTO> SubscribedTo { get; set; } = new List<UserDTO>();
        [JsonIgnore]
        public List<NewsDTO> NewsWritten { get; set; } = new List<NewsDTO>();
    }
}
