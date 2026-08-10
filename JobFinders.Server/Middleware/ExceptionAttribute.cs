namespace JobFinders.Server.Middleware
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
                    await context.Response.WriteAsJsonAsync(new
                    {
                        errorText = ex.Message
                    });
                }                
            }
        }
    }
}
