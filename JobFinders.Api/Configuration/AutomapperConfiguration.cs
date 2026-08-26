using AutoMapper;

using JobFinders.Api.Models;
using JobFinders.Domain.Models;

namespace JobFinders.Api.Configuration
{
    public static class AutomapperConfiguration
    {
        public static void ConfigureAutomapper(this IServiceCollection services) 
        {
            services.AddSingleton(provider =>
            {
                var config = new MapperConfiguration(autoMapperConfig =>
                {
                    autoMapperConfig.CreateMap<JobsRequest, JobsQuery>();
                }, provider.GetRequiredService<ILoggerFactory>());

                return config.CreateMapper();
            });
        }
    }
}
