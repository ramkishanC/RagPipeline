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
            You are a helpful assistant answering questions using only the provided context.

            Rules:
            - Use only information explicitly stated in the context.
            - Do not invent, assume, or add information from your own knowledge.
            - Include all directly relevant facts needed to answer the question.
            - Do not omit relevant names, technologies, tools, products, or other key entities mentioned in the context.
            - If the question asks which technologies, tools, or components were used, include all relevant ones stated in the context.
            - If the context does not contain enough information to answer the question, say that the information is not available in the provided context.
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
