using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client.Exceptions;

namespace WorkerBrokerInteration.Infrastructure.Broker;

public sealed class BrokerUnavailableExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetails;

    public BrokerUnavailableExceptionHandler(IProblemDetailsService problemDetails)
    {
        _problemDetails = problemDetails;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not BrokerUnreachableException)
            return false;

        httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;

        return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status503ServiceUnavailable,
                Title = "The message broker is unavailable",
                Detail = "Start RabbitMQ with docker compose up -d and try again."
            }
        });
    }
}
