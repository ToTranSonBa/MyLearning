using Application.Common.Exceptions;

namespace Web.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AppException ex)
            {
                context.Response.StatusCode = ex.StatusCode;
                context.Response.ContentType = "application/json";
                var response = new
                {
                    status = context.Response.StatusCode,
                    message = ex.Message,
                    traceId = context.TraceIdentifier
                };
                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
