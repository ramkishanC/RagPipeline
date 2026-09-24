using Microsoft.Extensions.Configuration;
using RagPipeline.Application.Interfaces.Services;
using RagPipeline.Infrastructure.Models.Ollama;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace RagPipeline.Infrastructure.Services
{
    public class OllamaAnswerGenerationService : IAnswerGenerationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _model;

        public OllamaAnswerGenerationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _model = configuration["Ollama:ChatModel"] ?? throw new InvalidOperationException("Ollama chat model is not configured.");
        }

        public async Task<string> GenerateAnswerAsync(string query, IReadOnlyList<string> context, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException("Query cannot be empty.",nameof(query));
            }

            if (context.Count == 0)
            {
                return "I could not find relevant information to answer the question.";
            }

            var combinedContext = string.Join("\n\n---\n\n",context);

            var prompt = $"""
            You are a helpful assistant answering questions using the provided context.

            Rules:
            - Answer using only the provided context.
            - Do not invent information.
            - If the context does not contain the answer, say that the information is not available in the provided context.
            - Keep the answer concise and directly answer the question.

            Context:
            {combinedContext}

            Question:
            {query}
            """;

            var request = new OllamaChatRequest
            {
                Model = _model,
                Stream = false,
                Messages =
                [
                    new OllamaChatMessage
                {
                    Role = "user",
                    Content = prompt
                }
                ]
            };

            var response = await _httpClient.PostAsJsonAsync("api/chat", request, cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(cancellationToken);

            if (result is null || result.Message is null)
            {
                throw new InvalidOperationException("Ollama returned an empty response.");
            }

            return result.Message.Content;
        }
    }
}
