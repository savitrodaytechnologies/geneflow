using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using GeneFlow.Infrastructure.Data;
using GeneFlow.Infrastructure.Services;

namespace GeneFlow.Tests.Helpers;

/// <summary>
/// Shared factory helpers for creating in-memory DB and services for tests.
/// </summary>
public static class TestFactory
{
    /// <summary>Creates a fresh in-memory GeneFlowDbContext for each test.</summary>
    public static GeneFlowDbContext CreateDbContext(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<GeneFlowDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;
        var ctx = new GeneFlowDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    /// <summary>Creates a JwtTokenService with test configuration.</summary>
    public static JwtTokenService CreateJwtService()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "TestSecretKey_AtLeast32CharactersLong_ForHmacSha256!",
                ["Jwt:Issuer"] = "GeneFlowTestApi",
                ["Jwt:Audience"] = "GeneFlowTestApp",
                ["Jwt:ExpiryMinutes"] = "60",
            })
            .Build();
        return new JwtTokenService(config);
    }
}
