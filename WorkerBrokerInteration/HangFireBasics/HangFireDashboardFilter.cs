using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;
using WorkerBrokerInteration.Configurations;

namespace Financeiro.Services.Worker
{
    public class HangFireDashboardFilter : IDashboardAuthorizationFilter
    {
        private readonly string _userName;
        private readonly string _password;

        public HangFireDashboardFilter(IOptions<ConfigHangFire> config)
        {
            _userName = config.Value.DashboardAuth?.Username ?? string.Empty;
            _password = config.Value.DashboardAuth?.Password ?? string.Empty;
        }

        public bool Authorize([NotNull] DashboardContext context)
        {
            HttpContext httpContext = context.GetHttpContext();

            var header = httpContext.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(header))
            {
                SetChallengeResponse(httpContext);
                return false;
            }

            var authValues = AuthenticationHeaderValue.Parse(header);

            if (!"Basic".Equals(authValues.Scheme, StringComparison.InvariantCultureIgnoreCase))
            {
                SetChallengeResponse(httpContext);
                return false;
            }

            var parameter = Encoding.UTF8.GetString(Convert.FromBase64String(authValues.Parameter));
            var parts = parameter.Split(':', 2);

            var username = parts[0];
            var password = parts[1];

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                SetChallengeResponse(httpContext);
                return false;
            }

            if (username == _userName && password == _password) return true;

            SetChallengeResponse(httpContext);
            return false;
        }

        private static void SetChallengeResponse(HttpContext httpContext)
        {
            httpContext.Response.StatusCode = 401;
            httpContext.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Hangfire Dashboard\"");
        }
    }
}
