using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Neo4jClient;
using NewsFlow.Server.DTOs;
using NewsFlow.Server.Models;
using NewsFlow.Server.Services;
using StackExchange.Redis;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using System.Text;

namespace NewsFlow.Server.Controllers
{
    [ApiController]
    [Route("user")]
    public class UserController : Controller
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IBoltGraphClient _neo4j;
        private readonly IIdentifierService _ids;
        private readonly IConfiguration _config;

        public UserController(
            IConnectionMultiplexer redis,
            IBoltGraphClient neo4j,
            IIdentifierService ids,
            IConfiguration config)
        {
            _redis = redis;
            _neo4j = neo4j;
            _ids = ids;
            _config = config;
        }

        [HttpPost("signup")]
        public async Task<ActionResult> SignUp([FromBody] UserRegisterDTO u)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(u.Password);
            var nameStrings = u.Name.Split(" ");
            string generatedImage = $"https://ui-avatars.com/api/?background=311b92&color=fff&name={nameStrings.First()}+{nameStrings.Last()}&rounded=true";
            var newUser = new User
            {
                Id = await _ids.UserNext(),
                Name = u.Name,
                Email = u.Email,
                Phone = u.Phone,
                ImageUrl = generatedImage,
                PasswordHash = passwordHash,
                EmailConfirmed = false,
                Country = u.Country,
                City = u.City,
                Role = u.Role
            };

            var testEmail = await _neo4j.Cypher
                .Match("(user:User)")
                .Where((User user)=>user.Email==u.Email)
                .Return(user => user.As<User>()).ResultsAsync;

            if(testEmail.Any())
            {
                return BadRequest("This email address is already in use, please enter a new one!");
            }


            byte[] tokenBytes = Guid.NewGuid().ToByteArray();
            var codeEncoded = WebEncoders.Base64UrlEncode(tokenBytes);
            var confirmationLink = Url.Action("VerifyUserEmail", "user", new { codeEncoded, email = u.Email }, Request.Scheme);

            String poruka;
            poruka = $"Welcome {u.Name},\n\nPlease confirm your account registered on Bye World with this email adress on link down below.\n" +
                confirmationLink + "\n\nWelcome to Bye World!";

            var emailAdress = _config.GetSection("MailCredentials:email").Value!.ToString();
            var password = _config.GetSection("MailCredentials:password").Value!.ToString();

            SmtpClient Client = new SmtpClient()
            {
                Host = "smtp.outlook.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential()
                {
                    UserName = emailAdress,
                    Password = password
                }
            };

            MailAddress fromMail = new MailAddress(emailAdress, "ByeWorld");
            MailAddress toMail = new MailAddress(u.Email, u.Name);
            MailMessage message = new MailMessage()
            {
                From = fromMail,
                Subject = "Confirm your Bye World account",
                Body = poruka
            };

            message.To.Add(toMail);
            await Client.SendMailAsync(message);

            var db = _redis.GetDatabase();
            await db.StringSetAsync(u.Email, codeEncoded, expiry: TimeSpan.FromMinutes(30));
            await _neo4j.Cypher.Create("(u:User $user)")
                   .WithParam("user", newUser)
                   .ExecuteWithoutResultsAsync();

            return Ok("User added succesful!");
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("VerifyUserEmail")]
        public async Task<ActionResult> VerifyUserEmail([FromQuery] String codeEncoded, [FromQuery] String email)
        {
            if (String.IsNullOrEmpty(codeEncoded))
                return BadRequest("Invalid verification code");

            var result = await _neo4j.Cypher
                .Match("(u:User)")
                .Where((User u) => u.Email == email)
                .Return(u => u.As<User>())
                .ResultsAsync;

            var user = result.FirstOrDefault();
            if (user == null)
                return BadRequest("No user account with given adress");

            if (user.EmailConfirmed)
                return BadRequest("Account already confirmed");

            var codeDecodedBytes = WebEncoders.Base64UrlDecode(codeEncoded);
            var codeDecoded = Encoding.UTF8.GetString(codeDecodedBytes);

            var db = _redis.GetDatabase();
            String? confirmationCode = db.StringGet(email);
            if (confirmationCode == null)
                return BadRequest("Error verifying user account, try again later!");

            if (confirmationCode == codeEncoded)
            {
                await _neo4j.Cypher
                    .Match("(u:User)")
                    .Where((User u) => u.Email == email)
                    .Set("u.EmailConfirmed = true")
                    .Return(u => u.As<User>())
                    .ExecuteWithoutResultsAsync();

                await db.KeyDeleteAsync(email);

                return Ok("Account confirmed");
            }

            return BadRequest("Error verifying user account, try again later!");
        }
    }
}
