namespace RagPipeline.Application.DTOs.KnowledgeBases
{
    public class CreateKnowledgeBaseRequest
    {
        public string ExternalUserId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }
}
