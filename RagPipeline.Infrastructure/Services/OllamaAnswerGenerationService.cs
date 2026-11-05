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
    public class OllamaAnswerGenerationService : IAnswerGenerationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _model;

        public OllamaAnswerGenerationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _model = configuration["Ollama:ChatModel"] ?? throw new InvalidOperationException("Ollama chat model is not configured.");
        }

        public async Task<string> GenerateAnswerAsync(string query, IReadOnlyList<string> context, IReadOnlyList<ChatMessage> history, CancellationToken cancellationToken = default)
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
            You are a retrieval-augmented question answering assistant.

            Answer the question using ONLY the provided context.

            Instructions:
            1. Find the exact information in the context that answers the question.
            2. Preserve every relevant named entity from the context.
            3. If the context lists multiple technologies, tools, products, or components relevant to the question, include ALL of them.
            4. Do not choose only one item when multiple relevant items are explicitly stated.
            5. Do not use outside knowledge.
            6. Do not invent information.
            7. Keep the answer concise.
            8. Every factual statement based on the context must end with its source citation.
            9. Put the citation at the end of the sentence, for example:
               "RabbitMQ and MassTransit were used for asynchronous order processing. [1]"
            10. Do not write "SOURCE [1]" or "Source [1]" in the answer.
            11. Use only the citation numbers provided in the context.
            12. Never create a citation number that does not exist in the context.
            13. If the context does not contain enough information, respond exactly:
               "The information is not available in the provided context."

            Context:
            {combinedContext}

            Question:
            {query}

            Answer:
            """;

            var messages = new List<OllamaChatMessage>();

            // Inject prior conversation history
            if (history != null && history.Count > 0)
            {
                foreach (var msg in history)
                {
                    messages.Add(new OllamaChatMessage
                    {
                        Role = msg.Role,
                        Content = msg.Content
                    });
                }
            }

            // Add current RAG prompt
            messages.Add(new OllamaChatMessage
            {
                Role = "user",
                Content = prompt
            });

            var request = new OllamaChatRequest
            {
                Model = _model,
                Stream = false,
                Messages = messages
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
