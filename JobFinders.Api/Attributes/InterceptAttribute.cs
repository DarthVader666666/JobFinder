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

            var searchResult = new SearchResult
            {
                UserId = int.Parse(userId ?? ""),
                Speciality = request.Speciality,
                Sources = JsonSerializer.Serialize(request.Sources ?? []),
                TimeSent = DateTime.UtcNow,
            };            

            if (executedContext.Result is ObjectResult { Value: JobsResponse jobsResponse })
            {
                bool predicate(Job job, JobsRequest request)
                {
                    var titleParts = job?.Title?.ToUpper().Split(' ', '-');
                    var specialityParts = request?.Speciality?.ToUpper().Split(' ', '-');
                    return titleParts?.Any(tp => specialityParts!.Any(sp => tp.Contains(sp))) ?? false;
                }

                var topJobGroup = jobsResponse?.JobGroups?.FirstOrDefault(group => group.Any(job => predicate(job, request)));
                var jsonResponse = JsonSerializer.Serialize(topJobGroup);
                searchResult.Response = jsonResponse;
            }

            await _unitOfWork.SearchResults.CreateAsync(searchResult);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}