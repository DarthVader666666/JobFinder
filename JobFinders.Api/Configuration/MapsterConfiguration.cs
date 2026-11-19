using Mapster;

using MapsterMapper;

using JobFinders.Domain.Entities;
using JobFinders.Domain.Models;
using JobFinders.Application.Models;

namespace JobFinders.Api.Configuration
{
    public static class MapsterConfiguration
    {
        public static void ConfigureMapster(this IServiceCollection services)
        {
            var config = new TypeAdapterConfig();

            config.NewConfig<JobsRequest, JobsQuery>();
            config.NewConfig<User, UserResponse>();

            services.AddSingleton(config);
            services.AddScoped<IMapper, Mapper>();
        }
    }
}