using Npgsql;

namespace SchoolAdmission.Api.Infrastructure.Data;

public static class DbConnectionHelper
{
    public static string ResolveConnectionString(IConfiguration configuration)
    {
        var rawUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrWhiteSpace(rawUrl))
        {
            if (rawUrl.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
                rawUrl.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            {
                var uri = new Uri(rawUrl);
                var userInfo = uri.UserInfo.Split(':');
                var builder = new NpgsqlConnectionStringBuilder
                {
                    Host = uri.Host,
                    Port = uri.Port > 0 ? uri.Port : 5432,
                    Username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : string.Empty,
                    Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
                    Database = uri.AbsolutePath.TrimStart('/'),
                    SslMode = SslMode.Prefer
                };
                return builder.ConnectionString;
            }

            return rawUrl;
        }

        var connString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(connString))
        {
            return connString;
        }

        // Local development default
        return "Host=localhost;Port=5432;Database=school_admission;Username=postgres;Password=postgres";
    }
}
