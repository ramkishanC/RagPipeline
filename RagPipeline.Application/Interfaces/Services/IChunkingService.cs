namespace RagPipeline.Application.Interfaces.Services
{
    public interface IChunkingService
    {
        List<string> Chunk(
            string text,
            int chunkSize = 1000,
            int overlap = 200);
    }
}
