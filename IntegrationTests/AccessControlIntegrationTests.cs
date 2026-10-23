using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RagPipeline.API;
using RagPipeline.Application.DTOs.Documents;
using RagPipeline.Application.DTOs.KnowledgeBases;
using RagPipeline.Application.Interfaces.Services;

namespace IntegrationTests;

public class AccessControlIntegrationTests
{
    private static readonly Guid TenantAKnowledgeBaseId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TenantADocumentId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task GetDocument_ReturnsForbidden_WhenRequestedByDifferentTenant()
    {
        await using var factory = new AccessControlApiFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", "user-002");

        var response = await client.GetAsync($"/api/Documents/{TenantADocumentId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateKnowledgeBase_RejectsExternalUserIdThatDoesNotMatchHeader()
    {
        await using var factory = new AccessControlApiFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", "user-002");

        var response = await client.PostAsJsonAsync("/api/KnowledgeBase", new
        {
            externalUserId = "user-001",
            name = "Spoofed KB"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(factory.KnowledgeBaseService.CreateWasCalled);
    }

    [Fact]
    public async Task CreateKnowledgeBase_UsesHeaderIdentity()
    {
        await using var factory = new AccessControlApiFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", "user-002");

        var response = await client.PostAsJsonAsync("/api/KnowledgeBase", new { name = "User Two KB" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var knowledgeBase = await response.Content.ReadFromJsonAsync<KnowledgeBaseResponse>();
        Assert.Equal("user-002", knowledgeBase?.ExternalUserId);
    }
}

internal sealed class AccessControlApiFactory : WebApplicationFactory<Program>
{
    public FakeKnowledgeBaseService KnowledgeBaseService { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IKnowledgeBaseService>();
            services.RemoveAll<IDocumentService>();
            services.AddSingleton<IKnowledgeBaseService>(KnowledgeBaseService);
            services.AddSingleton<IDocumentService>(new FakeDocumentService());
        });
    }
}

internal sealed class FakeKnowledgeBaseService : IKnowledgeBaseService
{
    private readonly Dictionary<Guid, string> _owners = new()
    {
        [Guid.Parse("11111111-1111-1111-1111-111111111111")] = "user-001"
    };

    public bool CreateWasCalled { get; private set; }

    public Task<KnowledgeBaseResponse> CreateAsync(string externalUserId, string name, CancellationToken cancellationToken = default)
    {
        CreateWasCalled = true;
        var response = new KnowledgeBaseResponse { Id = Guid.NewGuid(), ExternalUserId = externalUserId, Name = name, CreatedAt = DateTime.UtcNow };
        _owners[response.Id] = externalUserId;
        return Task.FromResult(response);
    }

    public Task<bool> HasAccessAsync(Guid knowledgeBaseId, string externalUserId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_owners.TryGetValue(knowledgeBaseId, out var owner) && owner == externalUserId);
    }

    public Task<List<KnowledgeBaseResponse>> GetAllAsync(string externalUserId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new List<KnowledgeBaseResponse>());
    }

    public Task<KnowledgeBaseResponse?> GetByIdAsync(Guid id, string externalUserId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<KnowledgeBaseResponse?>(null);
    }

    public Task<KnowledgeBaseResponse?> UpdateAsync(Guid id, string externalUserId, UpdateKnowledgeBaseRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<KnowledgeBaseResponse?>(null);
    }

    public Task<bool> DeleteAsync(Guid id, string externalUserId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}

internal sealed class FakeDocumentService : IDocumentService
{
    public Task<DocumentResponse> UploadAsync(Guid knowledgeBaseId, Stream stream, string fileName, string contentType, long fileSize, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<DocumentResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id != Guid.Parse("22222222-2222-2222-2222-222222222222"))
        {
            return Task.FromResult<DocumentResponse?>(null);
        }

        return Task.FromResult<DocumentResponse?>(new DocumentResponse { Id = id, KnowledgeBaseId = Guid.Parse("11111111-1111-1111-1111-111111111111"), FileName = "seeded.txt" });
    }

    public Task<List<DocumentResponse>> GetAllAsync(Guid knowledgeBaseId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new List<DocumentResponse>());
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(id == Guid.Parse("22222222-2222-2222-2222-222222222222"));
    }
}
