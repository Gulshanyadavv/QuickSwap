public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    public GlobalExceptionMiddleware(RequestDelegate next) { _next = next; }

    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex switch
            {
                UnauthorizedAccessException => 401,
                KeyNotFoundException => 404,
                _ => 500
            };
            var response = new
            {
                message = ex.Message,
                innerException = ex.InnerException?.Message
            };
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
