using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace AirAware.Middleware;

public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly byte[] _validApiKeyBytes;
    private const string ApiKeyHeaderName = "X-API-KEY";

    public ApiKeyAuthMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        
        // Cache the valid API key from configuration during construction
        var validApiKey = configuration.GetValue<string>("ApiKey");
        
        // Check for server misconfiguration at startup
        if (string.IsNullOrWhiteSpace(validApiKey))
        {
            throw new InvalidOperationException(
                "API Key is not configured. Please set the 'ApiKey' value in appsettings.json or environment variables.");
        }
        
        _validApiKeyBytes = Encoding.UTF8.GetBytes(validApiKey);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract the API key from the request header
        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("API Key is missing");
            return;
        }

        // Validate the API key using constant-time comparison to prevent timing attacks
        var extractedApiKeyBytes = Encoding.UTF8.GetBytes(extractedApiKey.ToString());
        
        if (!CryptographicOperations.FixedTimeEquals(_validApiKeyBytes, extractedApiKeyBytes))
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("Invalid API Key");
            return;
        }

        // If valid, continue to the next middleware
        await _next(context);
    }
}
