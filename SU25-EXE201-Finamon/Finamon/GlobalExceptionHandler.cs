using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Finamon
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(
                $"An error occurred while processing your request: {exception.Message}");
            var problemDetails = new ProblemDetails
            {
                Detail = exception.Message
            };

            switch (exception)
            {
                case BadHttpRequestException:
                    problemDetails.Status = (int)HttpStatusCode.BadRequest;
                    problemDetails.Title = exception.GetType().Name;
                    break;
                // Add more later
                default:
                    problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                    problemDetails.Title = "Internal Server Error";
                    break;
            }
            httpContext.Response.StatusCode = problemDetails.Status.Value;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
