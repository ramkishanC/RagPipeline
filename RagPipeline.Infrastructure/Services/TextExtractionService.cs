using System.Text;
using RagPipeline.Application.Interfaces.Services;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace RagPipeline.Infrastructure.Services
{
    public class TextExtractionService : ITextExtractionService
    {
        public async Task<string> ExtractAsync( Stream stream, string fileName, CancellationToken cancellationToken = default)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".pdf" => ExtractPdf(stream, cancellationToken),
                ".txt" or ".md" => await ExtractTextAsync(stream, cancellationToken),
                _ => throw new NotSupportedException($"File type '{extension}' is not supported.")
            };
        }

        private static string ExtractPdf(Stream stream, CancellationToken cancellationToken)
        {
            using var document = PdfDocument.Open(stream);
            var builder = new StringBuilder();

            foreach (var page in document.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var text = ContentOrderTextExtractor.GetText(page);

                builder.AppendLine(text);
                builder.AppendLine();
            }

            return builder.ToString();
        }

        private static async Task<string> ExtractTextAsync(Stream stream, CancellationToken cancellationToken)
        {
            using var reader = new StreamReader(stream, leaveOpen: true);
            return await reader.ReadToEndAsync(cancellationToken);
        }
    }
}
