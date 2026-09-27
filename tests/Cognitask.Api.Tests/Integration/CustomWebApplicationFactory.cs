using Cognitask.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cognitask.Api.Tests.Integration;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private readonly string _databaseName =
        $"CognitaskTestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting(
            "JwtSettings:SecretKey",
            "Test-Only-JWT-Secret-Key-That-Is-Long-Enough-123456789");

        builder.UseSetting(
            "JwtSettings:Issuer",
            "Cognitask.Api");

        builder.UseSetting(
            "JwtSettings:Audience",
            "Cognitask.Client");

        builder.UseSetting(
            "JwtSettings:AccessTokenExpirationMinutes",
            "15");

        builder.UseSetting(
            "JwtSettings:RefreshTokenExpirationDays",
            "7");

        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor =
                services.SingleOrDefault(
                    d => d.ServiceType ==
                         typeof(
                             DbContextOptions<ApplicationDbContext>));

            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddDbContext<ApplicationDbContext>(
                options =>
                {
                    options.UseInMemoryDatabase(
                        _databaseName);
                });
        });
    }
}