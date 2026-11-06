using Microsoft.Extensions.Configuration;
using RagPipeline.Application.Interfaces.Services;
using RagPipeline.Domain.Entities;
using RagPipeline.Infrastructure.Models.Ollama;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace RagPipeline.Infrastructure.Services
{
    public class OllamaQueryRewritingService : IQueryRewritingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _model;

        public OllamaQueryRewritingService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _model = configuration["Ollama:ChatModel"] ?? throw new InvalidOperationException("Ollama chat model is not configured.");
        }

        public async Task<string> RewriteQueryAsync(
            string query,
            IReadOnlyList<ChatMessage> history,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException("Query cannot be empty.", nameof(query));
            }

            // Fast-path: First turn in a conversation needs no rewriting
            if (history == null || history.Count == 0)
            {
                return query;
            }

            var formattedHistory = new StringBuilder();
            foreach (var msg in history)
            {
                var speaker = msg.Role.Equals("assistant", StringComparison.OrdinalIgnoreCase) ? "Assistant" : "User";
                formattedHistory.AppendLine($"{speaker}: {msg.Content}");
            }

            var prompt = $"""
            You are a search query rewriter for a retrieval-augmented generation (RAG) system.
            Given the conversation history and the user's follow-up question, rewrite the follow-up question into a single, standalone search query that contains all necessary context and named entities (such as project names, technologies, or components) from the conversation history.

            Rules:
            1. Replace pronouns like "it", "they", "that", or "this" with the exact entities they refer to in the conversation history.
            2. If the follow-up question is already standalone and clear, return it unchanged.
            3. Do NOT answer the question.
            4. Return ONLY the rewritten query text, with no quotes, explanations, or prefixes.

            Conversation History:
            {formattedHistory}

            Follow-up Question:
            {query}

            Standalone Search Query:
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
            var rewritten = result?.Message?.Content?.Trim();

            return string.IsNullOrWhiteSpace(rewritten) ? query : rewritten;
        }
    }
}
