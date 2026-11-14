using System;
using System.Collections.Generic;
using System.Text;

namespace RagPipeline.Application.DTOs.Auth
{
    public class UserRegistrationDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
