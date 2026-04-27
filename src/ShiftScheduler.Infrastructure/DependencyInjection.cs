using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShiftScheduler.Infrastructure.Persistence;
using ShiftScheduler.Application.Repositories; // Arayüzler (Interface) için
using ShiftScheduler.Infrastructure.Repositories; // Gerçek sınıflar (Implementation) için
using ShiftScheduler.Infrastructure.Authentication;

namespace ShiftScheduler.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            
        // Token Servisi
        services.AddScoped<ShiftScheduler.Application.Services.ITokenService, TokenService>();

        // Repository Kayıtları
        services.AddScoped<IShiftRepository, ShiftRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        
        // KRİTİK: İzin Talepleri Repositoriesi (Tek satır yeterli)
        services.AddScoped<IShiftRequestRepository, ShiftRequestRepository>();
            
        return services;
    }
}
