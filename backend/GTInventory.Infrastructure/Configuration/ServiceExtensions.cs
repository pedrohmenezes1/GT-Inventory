using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using GTInventory.Domain.Interfaces;
using GTInventory.Infrastructure.Services.LdapService;


namespace GTInventory.Infrastructure.Configuration
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            services.Configure<LdapConfig>(configuration.GetSection("Ldap"));
            services.AddScoped<IUserService, LdapUserService>();
            return services;
        }
    }
}