using System.Text.Json.Serialization;

namespace RagPipeline.Application.DTOs.KnowledgeBases
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public class CreateKnowledgeBaseRequest
    {
        public string Name { get; set; } = string.Empty;
    }
}
