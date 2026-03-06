using Claims.Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Claims.API.Filters;

public class ValidationExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is ValidationException ex)
        {
            context.Result = new BadRequestObjectResult(new ValidationProblemDetails
            {
                Title = "Validation failed",
                Detail = ex.Message,
                Status = 400,
                Errors = { ["ValidationErrors"] = ex.Errors.ToArray() }
            });

            context.ExceptionHandled = true;
        }
    }
}
