using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Neo4jClient;
using NewsFlowAPI.DTOs;
using NewsFlowAPI.Models;
using NewsFlowAPI.Services;
using StackExchange.Redis;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Runtime.CompilerServices;

namespace NewsFlowAPI.Controllers
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
            string generatedImage = $"https://ui-avatars.com/api/?background=f18d00&color=fff&name={nameStrings.First()}+{nameStrings.Last()}&rounded=true";
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
                .Where((User user) => user.Email == u.Email)
                .Return(user => user.As<User>()).ResultsAsync;

            if (testEmail.Any())
            {
                return BadRequest("This email address is already in use, please enter a new one!");
            }


            byte[] tokenBytes = Guid.NewGuid().ToByteArray();
            var codeEncoded = WebEncoders.Base64UrlEncode(tokenBytes);
            var confirmationLink = Url.Action("VerifyUserEmail", "user", new { codeEncoded, email = u.Email }, Request.Scheme);

            String poruka;
            poruka = $"Welcome {u.Name},\n\nPlease confirm your account registered on NewsFlow with this email adress on link down below.\n" +
                confirmationLink + "\n\nWelcome to NewsFlow!";

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

            MailAddress fromMail = new MailAddress(emailAdress, "NewsFlow");
            MailAddress toMail = new MailAddress(u.Email, u.Name);
            MailMessage message = new MailMessage()
            {
                From = fromMail,
                Subject = "Confirm your NewsFlow World account",
                Body = poruka
            };

            message.To.Add(toMail);
            await Client.SendMailAsync(message);

            var db = _redis.GetDatabase();
            await db.StringSetAsync(u.Email, codeEncoded, expiry: TimeSpan.FromMinutes(30));
            await _neo4j.Cypher.Create("(u:User $user)")
                   .WithParam("user", newUser)
                   .ExecuteWithoutResultsAsync();

            return Ok("User added succesfuly!");
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("VerifyUserEmail")]
        public async Task<ActionResult> VerifyUserEmail([FromQuery] String codeEncoded, [FromQuery] String email)
        {
            try
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

                    await _neo4j.Cypher
                        .Merge("(l:Location {Name:$locationName})")
                        .OnCreate()
                        .Set("l.Id=$locationId")
                        .With("l")
                        .Match("(u:User)")
                        .Where((User u) => u.Id == user.Id)
                        .Match("(l2:Location {Name:$World})")
                        .Merge("(u)-[:FOLLOWS_LOCATION]->(l)")
                        .Merge("(u)-[:FOLLOWS_LOCATION]->(l2)")
                        .WithParams(new
                        {
                            locationName = user.City,
                            locationId = await _ids.LocationNext(),
                            World = "World"
                        })
                        .ExecuteWithoutResultsAsync();



                    return Ok("Account confirmed");
                }

                return BadRequest("Error verifying user account, try again later!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        }

        [HttpPost("signin")]
        public async Task<ActionResult> SignIn([FromBody] UserLoginDTO creds)
        {
            try
            {
                var result = await _neo4j.Cypher
                    .Match("(u:User)")
                    .Where((User u) => u.Email == creds.Email)
                    .Return(u => u.As<User>())
                    .ResultsAsync;

                var user = result.FirstOrDefault();

                if (user == null ||
                    BCrypt.Net.BCrypt.Verify(creds.Password, user.PasswordHash) == false)
                {
                    return NotFound("User with given email or password does not exist");
                }

                var db = _redis.GetDatabase();

                string sessionId = new PasswordGenerator.Password(
                    includeLowercase: true,
                    includeUppercase: true,
                    passwordLength: 50,
                    includeSpecial: false,
                    includeNumeric: false).Next();

                db.StringSet($"sessions:{sessionId}", JsonSerializer.Serialize(user), expiry: TimeSpan.FromHours(2));
                db.SetAdd("users:authenticated", user.Id);
                db.StringSet($"users:last_active:{user.Id}", DateTime.Now.ToString("ddMMyyyyHHmmss"), expiry: TimeSpan.FromHours(2));

                return Ok(new
                {
                    Session = new
                    {
                        Id = sessionId,
                        Expires = DateTime.Now.ToLocalTime() + TimeSpan.FromHours(2)
                    },
                    User = user
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize]
        [HttpPut("signout")]
        public async Task<ActionResult> UserSignOut()
        {
            try
            {
                var claims = HttpContext.User.Claims;
                var sessionId = claims.Where(c => c.Type == "SessionId").FirstOrDefault()?.Value;
                var userId = claims.FirstOrDefault(c => c.Type.Equals("Id"))?.Value;

                var db = _redis.GetDatabase();

                await db.KeyDeleteAsync(sessionId);
                await db.KeyDeleteAsync($"users:last_active:{userId}");
                await db.SetRemoveAsync("users:authenticated", userId);

                return Ok("Signed out successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("authcount")]
        public async Task<ActionResult> AuthenticatedUsersCount()
        {
            try
            {
                var db = _redis.GetDatabase();

                var count = 0;

                var authenticatedUsers = (await db.SetMembersAsync("users:authenticated")).ToList();
                foreach (var userId in authenticatedUsers)
                {
                    var timeActive = (await db.StringGetAsync($"users:last_active:{userId}")).ToString();

                    if (string.IsNullOrEmpty(timeActive))
                    {
                        await db.SetRemoveAsync("users:authenticated", userId);
                        continue;
                    }

                    var timeActiveDt = DateTime.ParseExact(timeActive, "ddMMyyyyHHmmss", null);
                    if (DateTime.Now - timeActiveDt <= TimeSpan.FromMinutes(15))
                    {
                        count++;
                    }
                }

                return Ok(count);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize]
        [HttpGet("search")]
        public async Task<ActionResult> Search([FromQuery] string name)
        {
            try
            {
                var query = _neo4j.Cypher
                    .Match("(u:User)")
                    .Where("u.Name =~ $query")
                    .OrWhere("u.Email =~ $query")
                    .WithParam("query", $"(?i).*{name ?? ""}.*")
                    .Return(u => new
                    {
                        u.As<User>().Id,
                        u.As<User>().Name,
                        u.As<User>().Email,
                        u.As<User>().ImageUrl
                    })
                    .Limit(5);

                return Ok(await query.ResultsAsync);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(long id)
        {
            try
            {
                var userId = long.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals("Id"))?.Value ?? "-1");

                var baseQueryResult = await _neo4j.Cypher
                    .Match("(u:User)")
                    .Where((User u) => u.Id == id)
                    .Return(u => new
                    {
                        u.As<User>().Id,
                        u.As<User>().Name,
                        u.As<User>().Email,
                        u.As<User>().ImageUrl
                    }).ResultsAsync;

                return Ok(baseQueryResult);
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize]
        [HttpGet("GetUserById/{id}")]
        public async Task<ActionResult> GetFullUserById(long id)
        {
            try
            {
                var userId = long.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals("Id"))?.Value ?? "-1");

                var baseQueryResult = await _neo4j.Cypher
                    .Match("(u:User)")
                    .Where((User u) => u.Id == id)
                    .Return(u => new AuthorInfoDTO
                    {
                        Id = u.As<User>().Id,
                        Name = u.As<User>().Name
                    }).ResultsAsync;

                var result = baseQueryResult.ToList();
                if (result.Count == 0)
                {
                    throw new Exception("USER DOESN'T EXIST");
                }

                return Ok(result);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        //[Authorize(Roles="Admin")]
        [HttpDelete("DeleteExpiredConfirmation")]
        public async Task<ActionResult> DeleteUsersExpiredConfirmation()
        {
            try
            {
                var unconfirmedUsers = await _neo4j.Cypher
                    .Match("(u:User)")
                    .Where((User u) => u.EmailConfirmed == false)
                    .Return(u => new
                    {
                        u.As<User>().Id,
                        u.As<User>().Email
                    })
                    .ResultsAsync;

                var db = _redis.GetDatabase();
                List<Task> deletes = new List<Task>();
                List<long> idsToDelete = new List<long>();
                foreach (var user in unconfirmedUsers)
                {
                    String? temp = db.StringGet(user.Email);
                    if (temp == null)
                    {
                        idsToDelete.Add(user.Id);
                        deletes.Append(
                            _neo4j.Cypher
                            .Match("(u:User)")
                            .Where((User u) => u.Id == user.Id)
                            .Delete("u").ExecuteWithoutResultsAsync()
                            );
                    }
                }
                try
                {
                    Task.WaitAll(deletes.ToArray());
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
                return Ok(idsToDelete);
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize]
        [HttpPut("FollowTag")]
        public async Task<ActionResult> FollowTags([FromBody] List<long> tagIds)
        {
            try
            {

                var claims = HttpContext.User.Claims;

                var userId = Int32.Parse(claims.Where(c => c.Type == "Id").FirstOrDefault()?.Value ?? "-1");
                //userId = 1;

                if (userId == -1)
                    return Unauthorized("Error user not signed in");

                var existingTags = await _neo4j.Cypher
                    .Match("(t:Tag)<-[:FOLLOWS_TAG]-(u:User)")
                    .Where("t.Id IN $tagIds")
                    .AndWhere((User u) => u.Id == userId)
                    .WithParam("tagIds", tagIds)
                    .Return(t => t.As<Tag>().Id)
                    .ResultsAsync;

                //var existingTagIds = existingTags.Select(tag => tag.Id).ToList();
                var newTagIds = tagIds.Except(existingTags).ToList();

                foreach (var tagId in newTagIds)
                {

                    await _neo4j.Cypher
                        .Match("(u:User)", "(t:Tag)")
                        .Where((User u) => u.Id == userId)
                        .AndWhere((Tag t) => t.Id == tagId)
                        .Create("(u)-[ft:FOLLOWS_TAG]->(t)")
                        .Set("ft.LikeCount=$LikeCount, ft.InterestCoefficient=$Interest")
                        //.WithParam("LikeCount", 1)
                        .WithParams(new { LikeCount = 0, Interest = 1 })
                        .ExecuteWithoutResultsAsync();
                }

                return Ok(newTagIds.Count);
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }


        [Authorize]
        [HttpPut("UnfollowTag")]
        public async Task<ActionResult> UnfollowTags([FromBody] List<long> tagIds)
        {
            try
            {
                var claims = HttpContext.User.Claims;

                var userId = Int32.Parse(claims.Where(c => c.Type == "Id").FirstOrDefault()?.Value ?? "-1");

                if (userId == -1)
                    return Unauthorized("Error user not signed in");

                await _neo4j.Cypher
                   .Match("(t:Tag)<-[ft:FOLLOWS_TAG]-(u:User)")
                   .Where("t.Id IN $tagIds")
                   .AndWhere((User u) => u.Id == userId)
                   .WithParam("tagIds", tagIds)
                   .Delete("ft")
                   .ExecuteWithoutResultsAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize]
        [HttpPut("AddLocation")]
        public async Task<ActionResult> AddLocation([FromBody] List<long> locIds)
        {
            try
            {
                var claims = HttpContext.User.Claims;

                var userId = Int32.Parse(claims.Where(c => c.Type == "Id").FirstOrDefault()?.Value ?? "-1");


                if (userId == -1)
                    return Unauthorized("Error user not signed in");

                var existingLocations = await _neo4j.Cypher
                    .Match("(l:Location)<-[:FOLLOWS_LOCATION]-(u:User)")
                    .Where("l.Id IN $locIds")
                    .AndWhere((User u) => u.Id == userId)
                    .WithParam("locIds", locIds)
                    .Return(l => l.As<Location>().Id)
                    .ResultsAsync;

                //var existingLocIds = existingLocations.Select(tag => tag.Id).ToList();
                var newLocIds = locIds.Except(existingLocations).ToList();

                foreach (var locId in newLocIds)
                {

                    await _neo4j.Cypher
                        .Match("(u:User)", "(l:Location)")
                        .Where((User u) => u.Id == userId)
                        .AndWhere((Location l) => l.Id == locId)
                        .Create("(u)-[fl:FOLLOWS_LOCATION]->(l)")
                        .ExecuteWithoutResultsAsync();
                }

                return Ok(newLocIds.Count);
            }catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }


        [Authorize]
        [HttpPut("RemoveLocation")]
        public async Task<ActionResult> RemoveLocation([FromBody] List<long> locIds)
        {
            try
            {
                var claims = HttpContext.User.Claims;

                var userId = Int32.Parse(claims.Where(c => c.Type == "Id").FirstOrDefault()?.Value ?? "-1");

                if (userId == -1)
                    return Unauthorized("Error user not signed in");

                await _neo4j.Cypher
                    .Match("(l:Location)<-[fl:FOLLOWS_LOCATION]-(u:User)")
                    .Where("l.Id IN $locIds")
                    .AndWhere((User u) => u.Id == userId)
                    .WithParam("locIds", locIds)
                    .Delete("fl")
                    .ExecuteWithoutResultsAsync();

                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize]
        [HttpPut("SubscribeTo")]
        public async Task<ActionResult> SubscribeTo([FromBody] List<long> userIds)
        {
            try
            {
                var claims = HttpContext.User.Claims;

                var userId = Int32.Parse(claims.Where(c => c.Type == "Id").FirstOrDefault()?.Value ?? "-1");

                if (userId == -1)
                    return Unauthorized("Error user not signed in");

                var alreadySubscribed = await _neo4j.Cypher
                    .Match("(u1:User)-[:SUBSCRIBED_TO]->(u2:User)")
                    .Where("u2.Id IN $userIds")
                    .AndWhere((User u1) => u1.Id == userId)
                    .WithParam("userIds", userIds)
                    .Return(u2 => u2.As<User>().Id)
                    .ResultsAsync;

                var newUserIds = userIds.Except(alreadySubscribed).ToList();

                foreach (var uId in newUserIds)
                {

                    await _neo4j.Cypher
                        .Match("(u1:User)", "(u2:User)")
                        .Where((User u1) => u1.Id == userId)
                        .AndWhere((User u2) => u2.Role == "Author")
                        .AndWhere((User u2) => u2.Id == uId)
                        .Create("(u1)-[:SUBSCRIBED_TO]->(u2)")
                        .ExecuteWithoutResultsAsync();
                }

                return Ok(newUserIds.Count);
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }



        [Authorize]
        [HttpPut("UnsubscribeFrom")]
        public async Task<ActionResult> UnsubscribeFrom([FromBody] List<long> userIds)
        {
            try
            {
                var claims = HttpContext.User.Claims;

                var userId = Int32.Parse(claims.Where(c => c.Type == "Id").FirstOrDefault()?.Value ?? "-1");

                if (userId == -1)
                    return Unauthorized("Error user not signed in");

                await _neo4j.Cypher
                    .Match("(u1:User)-[st:SUBSCRIBED_TO]->(u2:User)")
                    .Where("u2.Id IN $userIds")
                    .AndWhere((User u1) => u1.Id == userId)
                    .WithParam("userIds", userIds)
                    .Delete("st")
                    .ExecuteWithoutResultsAsync();

                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize]
        [HttpPut("UnsubscribeFrom2")]
        public async Task<ActionResult> UnsubscribeFrom2([FromBody] List<long> userIds)
        {
            try
            {
                var claims = HttpContext.User.Claims;

                var userId = Int32.Parse(claims.Where(c => c.Type == "Id").FirstOrDefault()?.Value ?? "-1");

                if (userId == -1)
                    return Unauthorized("Error user not signed in");

                var query = await _neo4j.Cypher
                    .Match("(u1:User)-[st:SUBSCRIBED_TO]->(u2:User)")
                    .Where("u2.Id IN $userIds")
                    .AndWhere((User u1) => u1.Id == userId)
                    .WithParam("userIds", userIds)
                    .Delete("st")
                    .Return(u2 => u2.As<User>().Id)
                    .ResultsAsync;

                var lista = query.ToList();

                return Ok(lista.Count);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpGet("DoUserFollowWriter/{userId}/{followedId}")]
        public async Task<ActionResult> DoUserFollowWriter([FromRoute] long userId, [FromRoute] long followedId)
        {
            try
            {
                if (userId < 1 || followedId < 1)
                    throw new Exception("IDS ARE BELOW 1");

                var lista = (await _neo4j.Cypher
                    .Match("(u1:User)-[st:SUBSCRIBED_TO]->(u2:User)")
                    .Where((User u1, User u2) => u1.Id == userId && u2.Id == followedId)
                    .Return((u1, u2) => new
                    {
                        id1 = u1.As<User>().Id,
                        id2 = u2.As<User>().Id
                    }).ResultsAsync).ToList();



                return Ok(lista.Count != 0);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize]
        [HttpGet("DoIFollowHim/{followedId}")]
        public async Task<ActionResult> DoIFollowHim([FromRoute] long followedId)
        {
            try
            {
                 var claims = HttpContext.User.Claims;

                var userId = Int32.Parse(claims.Where(c => c.Type == "Id").FirstOrDefault()?.Value ?? "-1");


                if (userId < 1 || followedId < 1)
                    throw new Exception("IDS ARE BELOW 1");

                var lista = (await _neo4j.Cypher
                    .Match("(u1:User)-[st:SUBSCRIBED_TO]->(u2:User)")
                    .Where((User u1, User u2) => u1.Id == userId && u2.Id == followedId)
                    .Return((u1, u2) => new
                    {
                        id1 = u1.As<User>().Id,
                        id2 = u2.As<User>().Id
                    }).ResultsAsync).ToList();



                return Ok(lista.Count != 0);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        //edit user 
        [Authorize]
        [HttpPut("UpdateUser/{id}")]
        public async Task<ActionResult> UpdateUser([FromRoute] long id, [FromBody] UserDTO editedUser)
        {
            try
            {
                var user = await _neo4j.Cypher
                    .Match("(u:User)")
                    .Where((User u) => u.Id == id)
                    .Return(u => u.As<User>())
                    .ResultsAsync;

                var u = user.ToList().First();
                u.Name = editedUser.Name;
                u.Email = editedUser.Email;
                u.Phone = editedUser.Phone;
                u.ImageUrl = editedUser.ImageUrl;
                u.Country = editedUser.Country;
                u.City = editedUser.City;
                u.Role = editedUser.Role;

                await _neo4j.Cypher
                    .Match("(uu:User)")
                    .Where((User uu) => uu.Id == id)
                    .Set("uu=$us")
                    .WithParam("us", u)
                    .ExecuteWithoutResultsAsync();

                return Ok(u);
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize]
        [HttpGet("getWritersByPrefix/{prefix}")]
        public async Task<ActionResult> GetWritersByPrefix([FromRoute] string prefix)
        {
            try
            {
                var loc = await _neo4j.Cypher
                    .Match("(u:User)")
                    .Where($"u.Name starts with '{prefix}'")
                    .AndWhere((User u) => u.Role == "Author")
                    .Return(u => new AuthorInfoDTO
                    {
                        Id = u.As<User>().Id,
                        Name = u.As<User>().Name
                    })
                    .ResultsAsync;
                return Ok(loc.ToList());
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize]
        [HttpGet("getWriterByName/{name}")]
        public async Task<ActionResult> GetWriterByName([FromRoute] string name)
        {
            try
            {
                var loc = await _neo4j.Cypher
                    .Match("(u:User)")
                    .Where((User u) => u.Name == name)
                    .Return(u => new AuthorInfoDTO
                    {
                        Id = u.As<User>().Id,
                        Name = u.As<User>().Name
                    })
                    .ResultsAsync;
                return Ok(loc.ToList());
            }
            catch (Exception ex) {
                return BadRequest(ex);
            }
        }
    }
}
