namespace RagPipeline.Application.DTOs.KnowledgeBases
{
    public class KnowledgeBaseAccessResponse
    {
        public Guid KnowledgeBaseId { get; set; }

        public string ExternalUserId { get; set; } = string.Empty;

        public bool Access { get; set; }
    }
}
