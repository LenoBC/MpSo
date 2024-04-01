using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MpSo.Common.Exceptions;

namespace MpSo.Api.Filters;

public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
{
    private readonly Dictionary<Type, Action<ExceptionContext>> _exceptionHandlers;

    public ApiExceptionFilterAttribute()
    {
        _exceptionHandlers = new Dictionary<Type, Action<ExceptionContext>>
        {
            { typeof(ValidationException), HandleValidationException },
            { typeof(FailedToFetchTagsException), HandleFailedToFetchTagsException },
            { typeof(ConflictException), HandleConflictException }
        };
    }

    public override void OnException(ExceptionContext context)
    {
        HandleException(context);

        base.OnException(context);
    }

    private void HandleException(ExceptionContext context)
    {
        Type type = context.Exception.GetType();
        if (_exceptionHandlers.TryGetValue(type, out Action<ExceptionContext>? value))
        {
            value.Invoke(context);
            return;
        }

        if (!context.ModelState.IsValid)
        {
            HandleInvalidModelStateException(context);
            return;
        }

        HandleUnknownException(context);
    }

    private void HandleValidationException(ExceptionContext context)
    {
        ValidationException? exception = context.Exception as ValidationException;

        ValidationProblemDetails details = new(exception!.Errors)
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        };

        context.Result = new BadRequestObjectResult(details);

        context.ExceptionHandled = true;
    }

    private static void HandleInvalidModelStateException(ExceptionContext context)
    {
        ValidationProblemDetails details = new(context.ModelState)
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        };

        context.Result = new BadRequestObjectResult(details);

        context.ExceptionHandled = true;
    }

    private void HandleFailedToFetchTagsException(ExceptionContext context)
    {
        FailedToFetchTagsException? exception = context.Exception as FailedToFetchTagsException;

        ProblemDetails details = new()
        {
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.3",
            Title = "Failed to fetch tags from the API.",
            Detail = exception!.Message,
        };

        context.Result = new ObjectResult(details) { StatusCode = StatusCodes.Status502BadGateway };

        context.ExceptionHandled = true;
    }

    private void HandleConflictException(ExceptionContext context)
    {
        ConflictException? exception = context.Exception as ConflictException;

        ProblemDetails details = new()
        {
            Status = StatusCodes.Status409Conflict,
            Title = "A conflict occurred while processing your request.",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            Detail = exception!.Message
        };

        context.Result = new ObjectResult(details) { StatusCode = StatusCodes.Status409Conflict };

        context.ExceptionHandled = true;
    }

    private static void HandleUnknownException(ExceptionContext context)
    {
        ProblemDetails details = new()
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An error occurred while processing your request.",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        };

        context.Result = new ObjectResult(details) { StatusCode = StatusCodes.Status500InternalServerError };

        context.ExceptionHandled = true;
    }
}
