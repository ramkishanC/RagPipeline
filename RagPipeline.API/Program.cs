using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Pgvector.EntityFrameworkCore;
using RagPipeline.API.Services;
using RagPipeline.API.Swagger;
using RagPipeline.Application.Interfaces.Repositories;
using RagPipeline.Application.Interfaces.Services;
using RagPipeline.Application.Services;
using RagPipeline.Infrastructure.Persistence;
using RagPipeline.Infrastructure.Repositories;
using RagPipeline.Infrastructure.Services;
using System.Reflection;
using System.Text;

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
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                }

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter JWT token without the Bearer prefix."
                });

                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
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
            builder.Services.AddScoped<ICurrentUserService, ClaimsCurrentUserService>();
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
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<RagPipeline.Infrastructure.Consumers.DocumentIngestionConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
                    {
                        h.Username(builder.Configuration["RabbitMQ:Username"]);
                        h.Password(builder.Configuration["RabbitMQ:Password"]);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
