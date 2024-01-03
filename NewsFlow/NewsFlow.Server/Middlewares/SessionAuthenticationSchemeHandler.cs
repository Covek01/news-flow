using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using NewsFlow.Server.Models;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace NewsFlow.Server.Middlewares
{
    public class SessionAuthenticationSchemeHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IConnectionMultiplexer _redis;
        public SessionAuthenticationSchemeHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IConnectionMultiplexer redis) : base(options, logger, encoder, clock)
        {
            this._redis = redis;
        }

        protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var db = _redis.GetDatabase();

            string? authHeader = Request.Headers.Authorization;

            if (authHeader == null || !authHeader.StartsWith("SessionId"))
            {
                return AuthenticateResult.Fail("Authentication failed");
            }

            var sessionId = authHeader["SessionId ".Length..].Trim();
            var value = db.StringGet($"sessions:{sessionId}").ToString();

            if (string.IsNullOrEmpty(value))
            {
                return AuthenticateResult.Fail("Authentication failed");
            }

            var user = JsonSerializer.Deserialize<User>(value);

            if (user != null)
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("Id", user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("SessionId", sessionId)
                };

                var claimsIdentity = new ClaimsIdentity(claims, nameof(SessionAuthenticationSchemeHandler));

                var ticket = new AuthenticationTicket(new ClaimsPrincipal(claimsIdentity), this.Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }

            return AuthenticateResult.Fail("Authentication failed");
        }
    }
}
