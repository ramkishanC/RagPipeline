using System;
using System.Collections.Generic;
using System.Text;

namespace RagPipeline.Application.DTOs.Search
{
    public class AskResponse
    {
        public string Query { get; set; } = string.Empty;

        public string Answer { get; set; } = string.Empty;
    }
}
