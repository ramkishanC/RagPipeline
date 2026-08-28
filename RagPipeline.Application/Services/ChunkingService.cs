using RagPipeline.Application.Interfaces.Services;

namespace RagPipeline.Application.Services
{
    public class ChunkingService : IChunkingService
    {
        public List<string> Chunk(
            string text,
            int chunkSize = 1000,
            int overlap = 200)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return [];
            }

            if (chunkSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(chunkSize));
            }

            if (overlap < 0 || overlap >= chunkSize)
            {
                throw new ArgumentOutOfRangeException(nameof(overlap));
            }

            var chunks = new List<string>();
            var start = 0;

            while (start < text.Length)
            {
                var length = Math.Min(chunkSize, text.Length - start);
                var end = start + length;

                // Avoid splitting a word when possible.
                if (end < text.Length)
                {
                    while (end > start && !char.IsWhiteSpace(text[end - 1]))
                    {
                        end--;
                    }

                    if (end == start)
                    {
                        end = Math.Min(start + chunkSize, text.Length);
                    }
                }

                var chunk = text[start..end].Trim();

                if (!string.IsNullOrWhiteSpace(chunk))
                {
                    chunks.Add(chunk);
                }

                if (end >= text.Length)
                {
                    break;
                }

                start = Math.Max(end - overlap, start + 1);
            }

            return chunks;
        }
    }
}
