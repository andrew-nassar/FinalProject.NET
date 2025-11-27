using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace FinalProject.NET.Services.Middleware
{
    public class JwtAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;

        public JwtAuthMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _config = config;
        }

        public async Task Invoke(HttpContext context)
        {


            var path = context.Request.Path.Value;

            // استثناء بعض المسارات من التحقق
            var allowedPaths = new[] { "/api/auth/Account/specializations", "/api/register-user", "/api/auth/Account/register-user", "/api/auth/Account/confirm-email", "/api/auth/Account/register-lawyer" };
            if (allowedPaths.Any(p => path!.StartsWith(p)))
            {
                await _next(context);
                return;
            }
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();

                    // Load values from appsettings.json
                    var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);
                    var issuer = _config["Jwt:Issuer"];
                    var audience = _config["Jwt:Audience"];

                    // Validate JWT Token
                    var parameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),

                        ValidateIssuer = !string.IsNullOrEmpty(issuer),
                        ValidIssuer = issuer,

                        ValidateAudience = !string.IsNullOrEmpty(audience),
                        ValidAudience = audience,

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    var principal = tokenHandler.ValidateToken(token, parameters, out SecurityToken validatedToken);

                    // Attach user claims to HttpContext
                    context.User = principal;

                    await _next(context);
                    return;
                }
                catch
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Invalid Token");
                    return;
                }
            }

            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Token Missing");
        }
    }
}
