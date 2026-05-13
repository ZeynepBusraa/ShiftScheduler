using System.Collections.Generic;
using System.Threading.Tasks;
using ShiftScheduler.Domain.Entities;
using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Application.Repositories;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByIdAsync(int id);
    Task<List<User>> ListAllAsync();

    /// <summary>Belirli bölüm ve roldeki kullanıcıları getirir (en kıdemli kontrolü için).</summary>
    Task<List<User>> ListByDepartmentAndRoleAsync(int departmentId, Role role);

    Task<User> SaveAsync(User entity);
    Task DeleteAsync(User entity);
}
