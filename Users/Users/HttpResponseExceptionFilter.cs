using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Users.API
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var statusCode = context.Exception switch
            {
                ArgumentException or InvalidOperationException => 400, // Bad Request
                KeyNotFoundException => 404,                          // Not Found
                _ => 500                                              // Server Error
            };

            context.Result = new ObjectResult(new
            {
                error = true,
                message = context.Exception.Message,
                type = context.Exception.GetType().Name
            })
            {
                StatusCode = statusCode
            };

            context.ExceptionHandled = true;
        }
    }
}
