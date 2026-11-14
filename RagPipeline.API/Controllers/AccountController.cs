using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RagPipeline.Application.DTOs.Auth;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RagPipeline.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AccountController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto dto)
        {
            var client = _httpClientFactory.CreateClient();
            var authority = _configuration["Keycloak:Authority"] ?? "http://localhost:8080/realms/rag-platform";
            var adminTokenUrl = authority.Replace("/realms/rag-platform", "/realms/master/protocol/openid-connect/token");

            // 1. Get Keycloak Admin Token (using admin credentials for backend provisioning)
            var adminForm = new Dictionary<string, string>
            {
                { "client_id", "admin-cli" },
                { "username", "admin" },
                { "password", "admin" },
                { "grant_type", "password" }
            };

            var tokenResponse = await client.PostAsync(adminTokenUrl, new FormUrlEncodedContent(adminForm));
            if (!tokenResponse.IsSuccessStatusCode)
            {
                return StatusCode(500, "Failed to authenticate with Keycloak admin authority.");
            }

            var tokenJson = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>();
            var accessToken = tokenJson.GetProperty("access_token").GetString();

            // 2. Create User Payload for Keycloak
            var userPayload = new
            {
                username = dto.Username,
                email = dto.Email,
                firstName = dto.FirstName,
                lastName = dto.LastName,
                enabled = true,
                emailVerified = true,
                credentials = new[]
                {
                    new { type = "password", value = dto.Password, temporary = false }
                }
            };

            var createUserUrl = authority.Replace("/realms/rag-platform", "/admin/realms/rag-platform/users");
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, createUserUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(userPayload), Encoding.UTF8, "application/json")
            };
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var createResponse = await client.SendAsync(requestMessage);
            if (!createResponse.IsSuccessStatusCode)
            {
                var errorContent = await createResponse.Content.ReadAsStringAsync();
                return BadRequest(new { message = "User creation failed in Keycloak", details = errorContent });
            }

            return Ok(new { message = "Account created successfully. You can now sign in." });
        }
    }
}
