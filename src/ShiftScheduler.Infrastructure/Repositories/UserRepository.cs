using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShiftScheduler.Application.Repositories;
using ShiftScheduler.Domain.Entities;
using ShiftScheduler.Domain.Enums;
using ShiftScheduler.Infrastructure.Persistence;

namespace ShiftScheduler.Infrastructure.Repositories;

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public Task DeleteAsync(User entity)
    {
        _dbContext.Users.Remove(entity);
        return _dbContext.SaveChangesAsync();
    }

    public Task<User?> FindByEmailAsync(string email)
    {
        return _dbContext.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public Task<User?> FindByIdAsync(int id)
    {
        return _dbContext.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public Task<List<User>> ListAllAsync()
    {
        return _dbContext.Users
            .Include(u => u.Department)
            .ToListAsync();
    }

    /// <summary>
    /// Belirli bir bölüm ve roldeki tüm kullanıcıları getirir.
    /// En kıdemli kontrolü ve nöbet algoritması için kullanılır.
    /// </summary>
    public Task<List<User>> ListByDepartmentAndRoleAsync(int departmentId, Role role)
    {
        return _dbContext.Users
            .Where(u => u.DepartmentId == departmentId && u.Role == role)
            .ToListAsync();
    }

    public async Task<User> SaveAsync(User entity)
    {
        if (entity.Id == 0)
        {
            await _dbContext.Users.AddAsync(entity);
        }
        else
        {
            _dbContext.Users.Update(entity);
        }

        await _dbContext.SaveChangesAsync();
        return entity;
    }
}
