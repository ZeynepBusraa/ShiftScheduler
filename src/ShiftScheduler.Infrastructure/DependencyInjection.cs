using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShiftScheduler.Infrastructure.Persistence;

namespace ShiftScheduler.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            
        services.AddScoped<ShiftScheduler.Application.Services.ITokenService, ShiftScheduler.Infrastructure.Authentication.TokenService>();
        services.AddScoped<ShiftScheduler.Application.Repositories.IShiftRepository, ShiftScheduler.Infrastructure.Repositories.ShiftRepository>();
        services.AddScoped<ShiftScheduler.Application.Repositories.IUserRepository, ShiftScheduler.Infrastructure.Repositories.UserRepository>();
            
        return services;
    }
}
