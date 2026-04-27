using Microsoft.Extensions.DependencyInjection;
using ShiftScheduler.Application.Handlers.Shifts;
using ShiftScheduler.Application.Mappers;
using ShiftScheduler.Application.Services;

namespace ShiftScheduler.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Mappers
        services.AddScoped<UserDtoMapper>();
        services.AddScoped<ShiftDtoMapper>();
        services.AddScoped<ShiftRequestDtoMapper>();

        // Handlers - Shifts
        services.AddScoped<FindShiftHandler>();
        services.AddScoped<ListShiftsHandler>();
        services.AddScoped<SaveShiftHandler>();

        // Handlers - Users & Auth
        services.AddScoped<ShiftScheduler.Application.Handlers.Auth.LoginUserHandler>();
        services.AddScoped<ShiftScheduler.Application.Handlers.Users.CreateUserHandler>();
        services.AddScoped<ShiftScheduler.Application.Handlers.Users.ListUsersHandler>();
        services.AddScoped<ShiftScheduler.Application.Handlers.Users.DeleteUserHandler>();
        
        // Services
        services.AddScoped<IShiftService, ShiftService>();
        services.AddScoped<IUserService, UserService>();
        
        return services;
    }
}
