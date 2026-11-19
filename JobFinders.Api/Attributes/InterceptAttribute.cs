using JobFinders.Application.Models;
using JobFinders.Domain.Entities;
using JobFinders.Domain.Interfaces;
using JobFinders.Domain.Models;
using JobFinders.Domain.Services;

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
            var executedContext = await next();

            if (context is null)
            {
                return;
            }

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

            var userId = int.Parse(user?.Claims.FirstOrDefault(x => x.Type == "id")?.Value ?? "");
            var serializedSources = JsonSerializer.Serialize(request.Sources ?? []);

            var searchResult = new SearchResult
            {
                UserId = userId,
                Speciality = request.Speciality,
                Sources = serializedSources,
                TimeSent = TimeService.Now,
            };

            var hasEqualResults = _unitOfWork.SearchResults.All
                    .Where(sr => sr.TimeSent >= TimeService.Today && sr.UserId == userId)
                    .Any(sr => (sr.Location ?? "") == request.Location && sr.Speciality == request.Speciality && sr.Sources == serializedSources);

            if (!hasEqualResults && executedContext.Result is ObjectResult { Value: JobsSearchResult jobsResponse })
            {
                char[] separators = [' ', '-', '\\', '/', '|', ','];

                IEnumerable<string> getParts(string? line) => line?.ToUpper()?.Split(separators).Where(x => !string.IsNullOrWhiteSpace(x)) ?? [];

                bool predicate(Job job, JobsRequest request)
                {
                    var titleParts = getParts(job?.Title);
                    var specialityParts = getParts(request?.Speciality);
                    return titleParts?.Any(tp => specialityParts!.Any(sp => tp.Contains(sp))) ?? false;
                }

                var topJobGroup = jobsResponse?.JobGroups?.FirstOrDefault(group => group.Any(job => predicate(job, request)));
                var jsonResponse = JsonSerializer.Serialize(topJobGroup);
                searchResult.Response = jsonResponse;
                searchResult.Location = request.Location == string.Empty ? null : request.Location;

                await _unitOfWork.SearchResults.CreateAsync(searchResult);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}