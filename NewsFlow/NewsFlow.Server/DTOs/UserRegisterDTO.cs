namespace NewsFlow.Server.DTOs
{
    public class UserRegisterDTO
    {
        public String Name { get; set; }=String.Empty;

        public String Email { get; set; } =String.Empty;

        public String Phone { get; set; } =String.Empty;

        public String Password { get; set; } =String.Empty;

        public String Country { get; set; } =String.Empty;

        public String City { get; set; } = String.Empty;

        public String Role { get; set; } = "User";
    }
}
