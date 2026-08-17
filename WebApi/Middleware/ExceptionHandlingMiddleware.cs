using Application.Exceptions;
using Domain.Exceptions;

namespace WebApi.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleAsync(context, exception);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            EmailAlreadyExistsException => (StatusCodes.Status409Conflict, exception.Message),
            InvalidCredentialsException => (StatusCodes.Status401Unauthorized, exception.Message),
            NotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            RoleNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            RoleAlreadyExistsException => (StatusCodes.Status409Conflict, exception.Message),
            SystemRoleCannotBeDeletedException => (StatusCodes.Status400BadRequest, exception.Message),
            RoleInUseException => (StatusCodes.Status400BadRequest, exception.Message),
            PermissionNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            PermissionAlreadyExistsException => (StatusCodes.Status409Conflict, exception.Message),
            DomainException => (StatusCodes.Status400BadRequest, exception.Message),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception.");

        if (context.Response.HasStarted)
            return;

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new { message });
    }
}
