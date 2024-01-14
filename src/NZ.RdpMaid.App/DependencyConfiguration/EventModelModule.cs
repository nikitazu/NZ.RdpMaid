using Microsoft.Extensions.DependencyInjection;

namespace NZ.RdpMaid.App.DependencyConfiguration
{
    internal static class EventModelModule
    {
        public static IServiceCollection AddEventModelModule(this IServiceCollection services) => services
            .AddMediatR(config =>
            {
                config.Lifetime = ServiceLifetime.Singleton;
                config.RegisterServicesFromAssembly(typeof(EventModelModule).Assembly);
            });
    }
}