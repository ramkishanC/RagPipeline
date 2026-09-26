using System;
using System.Collections.Generic;
using System.Text;

namespace RagPipeline.Application.DTOs.Auth
{
    public class TokenRequest
    {
        public string ExternalUserId { get; set; } = string.Empty;
    }
}
