using JobFinders.Api.Models;
using JobFinders.Domain.Entities;
using JobFinders.Domain.Interfaces;
using JobFinders.Domain.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using System.Text.Json;

namespace JobFinders.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class InterceptRequestAttribute : Attribute, IFilterFactory
    {
        public bool IsReusable => false;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
            return new InterceptRequestFilter(unitOfWork);
        }
    }

    public class InterceptRequestFilter(IUnitOfWork unitOfWork) : IAsyncActionFilter
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context is null)
            {
                return;
            }

            var executedContext = await next();

            var user = context.HttpContext.User;
            var isAuthenticated = user?.Identity?.IsAuthenticated ?? false;

            if (!isAuthenticated)
            {
                return;
            }

            if (context.ActionArguments.Values.OfType<JobsRequest>().FirstOrDefault() is not { } request)
            {
                return;
            }

            if (request is null)
            {
                return;
            }

            var userId = user?.Claims.FirstOrDefault(x => x.Type == "id")?.Value;

            var jobSearchRequest = new JobSearchRequest
            {
                UserId = int.Parse(userId ?? ""),
                Speciality = request.Speciality,
                Sources = JsonSerializer.Serialize(request.Sources ?? []),
                TimeSent = DateTime.UtcNow,
            };

            await _unitOfWork.JobSearchRequests.CreateAsync(jobSearchRequest);
            await _unitOfWork.SaveChangesAsync();

            if (executedContext.Result is ObjectResult { Value: JobsResponse jobsResponse })
            {
                var topJobGroup = jobsResponse?.JobGroups?.FirstOrDefault(group => group.Any(job => job?.Title?.Contains(request.Speciality!, StringComparison.InvariantCultureIgnoreCase) ?? false));

            }
        }
    }
}