using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShiftScheduler.Application.Repositories;
using ShiftScheduler.Domain.Entities;
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
        return _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public Task<User?> FindByIdAsync(int id)
    {
        return _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public Task<List<User>> ListAllAsync()
    {
        return _dbContext.Users.ToListAsync();
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
