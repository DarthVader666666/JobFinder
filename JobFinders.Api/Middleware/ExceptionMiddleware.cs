namespace JobFinders.Api.Middleware
{
    public class ExceptionMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var cancellationToken = context.RequestAborted;

                if (cancellationToken.IsCancellationRequested)
                {
                    context.Response.StatusCode = 499;
                }
                else
                {
                    context.Response.StatusCode = 500;
                }

                await context.Response.WriteAsJsonAsync(new
                {
                    errorText = ex.Message
                });
            }
        }
    }
}
