using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using RagPipeline.Application.DTOs.Auth;
using System.Text;

namespace RagPipeline.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("token")]
        [AllowAnonymous]
        public IActionResult GenerateToken([FromBody] TokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ExternalUserId))
            {
                return BadRequest("ExternalUserId is required.");
            }

            var keyBytes = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);
            var securityKey = new SymmetricSecurityKey(keyBytes);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new Dictionary<string, object>
            {
                { JwtRegisteredClaimNames.Sub, request.ExternalUserId },
                { JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString() }
            };

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                Claims = claims,
                Expires = DateTime.UtcNow.AddMinutes(60),
                SigningCredentials = credentials
            };

            var handler = new JsonWebTokenHandler();
            var tokenString = handler.CreateToken(descriptor);

            return Ok(new { token = tokenString });
        }
    }
}
