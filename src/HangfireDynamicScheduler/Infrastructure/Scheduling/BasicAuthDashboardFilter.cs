using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Hangfire.Dashboard;
using HangfireDynamicScheduler.Configurations;

namespace HangfireDynamicScheduler.Infrastructure.Scheduling;

public sealed class BasicAuthDashboardFilter : IDashboardAuthorizationFilter
{
    private readonly DashboardCredentials _credentials;

    public BasicAuthDashboardFilter(DashboardCredentials credentials)
    {
        _credentials = credentials;
    }

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        if (IsAuthorized(httpContext.Request.Headers.Authorization.ToString()))
            return true;

        httpContext.Response.Headers.WWWAuthenticate = "Basic realm=\"Hangfire Dashboard\"";
        return false;
    }

    private bool IsAuthorized(string header)
    {
        if (string.IsNullOrEmpty(_credentials.Username) || string.IsNullOrEmpty(_credentials.Password))
            return false;

        if (!AuthenticationHeaderValue.TryParse(header, out var authorization)
            || !"Basic".Equals(authorization.Scheme, StringComparison.OrdinalIgnoreCase)
            || authorization.Parameter is null)
            return false;

        var buffer = new byte[authorization.Parameter.Length];
        if (!Convert.TryFromBase64String(authorization.Parameter, buffer, out var length))
            return false;

        var decoded = Encoding.UTF8.GetString(buffer, 0, length);
        var separator = decoded.IndexOf(':');
        if (separator < 0)
            return false;

        return Matches(decoded[..separator], _credentials.Username) & Matches(decoded[(separator + 1)..], _credentials.Password);
    }

    private static bool Matches(string provided, string expected) =>
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(provided), Encoding.UTF8.GetBytes(expected));
}
