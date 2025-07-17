using ConfigurableWorkflowEngine.Services;

namespace ConfigurableWorkflowEngine.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddWorkflowServices(this IServiceCollection services)
        {
            services.AddSingleton<IWorkflowService, WorkflowService>();
            return services;
        }
    }
}
