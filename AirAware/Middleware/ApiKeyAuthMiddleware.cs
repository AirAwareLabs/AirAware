using Microsoft.Extensions.Configuration;

namespace AirAware.Middleware;

public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private const string ApiKeyHeaderName = "X-API-KEY";

    public ApiKeyAuthMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
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

        // Get the valid API key from configuration
        var validApiKey = _configuration.GetValue<string>("ApiKey");

        // Validate the API key
        if (string.IsNullOrWhiteSpace(validApiKey) || !validApiKey.Equals(extractedApiKey))
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("Invalid API Key");
            return;
        }

        // If valid, continue to the next middleware
        await _next(context);
    }
}
