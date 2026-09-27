using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Cognitask.Api.Configuration;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtSettings>(
            configuration.GetSection("JwtSettings"));

        var jwtSettings =
            configuration
                .GetSection("JwtSettings")
                .Get<JwtSettings>()
            ?? throw new InvalidOperationException(
                "JwtSettings configuration is missing.");

        if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
        {
            throw new InvalidOperationException(
                "JWT SecretKey is missing.");
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                jwtSettings.SecretKey));

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer =
                            jwtSettings.Issuer,

                        ValidAudience =
                            jwtSettings.Audience,

                        IssuerSigningKey = key,

                        ClockSkew = TimeSpan.Zero
                    };
            });

        return services;
    }
}