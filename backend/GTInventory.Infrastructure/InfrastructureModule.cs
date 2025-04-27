using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using GTInventory.Infrastructure.Data;
using GTInventory.Domain.Interfaces;
using GTInventory.Application.Services;

namespace GTInventory.Infrastructure;

public static class InfrastructureModule
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<GTInventoryDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserService, LdapUserService>();

        return services;
    }
}
