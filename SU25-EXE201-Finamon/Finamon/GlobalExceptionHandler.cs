using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net;
using FluentValidation;

namespace Finamon
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(
                $"[ERROR] {DateTime.UtcNow:O} - {exception.GetType().Name}: {exception.Message}");

            var problemDetails = new ProblemDetails
            {
                Detail = exception.Message,
                Instance = httpContext.Request.Path,
                Type = $"https://httpstatuses.com/500" // mặc định Internal Server Error
            };

            switch (exception)
            {
                case BadHttpRequestException:
                    problemDetails.Status = (int)HttpStatusCode.BadRequest;
                    problemDetails.Title = "Bad Request";
                    problemDetails.Type = "https://httpstatuses.com/400";
                    break;

                case KeyNotFoundException:
                    problemDetails.Status = (int)HttpStatusCode.NotFound;
                    problemDetails.Title = "Resource Not Found";
                    problemDetails.Type = "https://httpstatuses.com/404";
                    break;

                case ArgumentNullException:
                    problemDetails.Status = (int)HttpStatusCode.BadRequest;
                    problemDetails.Title = "Missing Required Parameter";
                    problemDetails.Type = "https://httpstatuses.com/400";
                    break;

                case UnauthorizedAccessException:
                    problemDetails.Status = (int)HttpStatusCode.Unauthorized;
                    problemDetails.Title = "Unauthorized";
                    problemDetails.Type = "https://httpstatuses.com/401";
                    break;

                case FluentValidation.ValidationException validationEx:
                    problemDetails.Status = (int)HttpStatusCode.BadRequest;
                    problemDetails.Title = "Validation Failed";
                    problemDetails.Type = "https://httpstatuses.com/400";

                    problemDetails.Extensions["errors"] = validationEx.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray());
                    break;

                case DbUpdateException:
                    problemDetails.Status = (int)HttpStatusCode.Conflict;
                    problemDetails.Title = "Database Update Error";
                    problemDetails.Type = "https://httpstatuses.com/409";
                    break;

                default:
                    problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                    problemDetails.Title = "Internal Server Error";
                    break;
            }

            httpContext.Response.StatusCode = problemDetails.Status ?? 500;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
