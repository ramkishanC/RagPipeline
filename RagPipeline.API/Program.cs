using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Pgvector.EntityFrameworkCore;
using RagPipeline.Application.Interfaces.Repositories;
using RagPipeline.Application.Interfaces.Services;
using RagPipeline.Application.Services;
using RagPipeline.Infrastructure.Persistence;
using RagPipeline.Infrastructure.Repositories;
using RagPipeline.Infrastructure.Services;
using RagPipeline.API.Services;
using RagPipeline.API.Swagger;

namespace RagPipeline.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.OperationFilter<CurrentUserHeaderOperationFilter>();
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                }
            });

            builder.Services.AddDbContext<RagDbContext>(options =>
            {
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptions =>
                    {
                        npgsqlOptions.UseVector();
                    });
            });

            builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
            builder.Services.AddScoped<IVectorSearchRepository, VectorSearchRepository>();
            builder.Services.AddScoped<IKeywordSearchRepository, KeywordSearchRepository>();
            builder.Services.AddScoped<IKnowledgeBaseRepository, KnowledgeBaseRepository>();
            builder.Services.AddScoped<IKnowledgeBaseService, KnowledgeBaseService>();
            builder.Services.AddScoped<ICurrentUserService, HeaderCurrentUserService>();
            builder.Services.AddScoped<IDocumentService, DocumentService>();
            builder.Services.AddScoped<ITextExtractionService, TextExtractionService>();
            builder.Services.AddScoped<IChunkingService, ChunkingService>();
            builder.Services.AddScoped<IVectorSearchService, VectorSearchService>();
            builder.Services.AddScoped<IKeywordSearchService, KeywordSearchService>();
            builder.Services.AddScoped<IHybridSearchService, HybridSearchService>();
            builder.Services.AddScoped<IRerankedSearchService, RerankedSearchService>();
            builder.Services.AddHttpClient<IEmbeddingService, OllamaEmbeddingService>(client =>
            {
                var baseUrl = builder.Configuration["Ollama:BaseUrl"] ?? throw new InvalidOperationException("Ollama BaseUrl is missing.");

                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromMinutes(2);
            });
            builder.Services.AddHttpClient<IRerankerService, RerankerService>(client =>
            {
                var baseUrl = builder.Configuration["Reranker:BaseUrl"] ?? throw new InvalidOperationException("Reranker BaseUrl is missing.");

                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromMinutes(2);
            });
            builder.Services.AddHttpClient<IAnswerGenerationService, OllamaAnswerGenerationService>(client =>
            {
                var baseUrl = builder.Configuration["Ollama:BaseUrl"] ?? throw new InvalidOperationException("Ollama BaseUrl is missing.");

                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromMinutes(5);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
