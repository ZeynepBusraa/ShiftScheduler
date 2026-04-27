using System.Collections.Generic;
using System.Threading.Tasks;
using ShiftScheduler.Domain.Entities;

namespace ShiftScheduler.Application.Repositories;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByIdAsync(int id);
    Task<List<User>> ListAllAsync();
    Task<User> SaveAsync(User entity);
    Task DeleteAsync(User entity);
}
