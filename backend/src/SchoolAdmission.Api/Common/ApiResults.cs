using System.Text.Json;

namespace SchoolAdmission.Api.Common;

public static class ApiResults
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static IResult Ok<T>(T data)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        return Results.Content(json, "application/json", System.Text.Encoding.UTF8, StatusCodes.Status200OK);
    }

    public static IResult Created<T>(string location, T data)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        return new CustomCreatedResult(location, json);
    }

    public static IResult NoContent() => Results.NoContent();

    private sealed class CustomCreatedResult : IResult
    {
        private readonly string _location;
        private readonly string _json;

        public CustomCreatedResult(string location, string json)
        {
            _location = location;
            _json = json;
        }

        public async Task ExecuteAsync(HttpContext httpContext)
        {
            httpContext.Response.StatusCode = StatusCodes.Status201Created;
            httpContext.Response.ContentType = "application/json; charset=utf-8";
            if (!string.IsNullOrEmpty(_location))
            {
                httpContext.Response.Headers.Location = _location;
            }
            await httpContext.Response.WriteAsync(_json);
        }
    }
}

