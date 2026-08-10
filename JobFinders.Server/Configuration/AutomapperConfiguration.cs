using AutoMapper;

using JobFinders.BLL.Models;
using JobFinders.Server.Models;

namespace JobFinders.Server.Configuration
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
