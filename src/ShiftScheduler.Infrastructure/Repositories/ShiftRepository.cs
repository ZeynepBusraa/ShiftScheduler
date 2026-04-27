using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShiftScheduler.Application.Repositories;
using ShiftScheduler.Domain.Entities;
using ShiftScheduler.Infrastructure.Persistence;

namespace ShiftScheduler.Infrastructure.Repositories;

public class ShiftRepository(AppDbContext dbContext) : IShiftRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public Task DeleteAsync(Shift entity)
    {
        _dbContext.Shifts.Remove(entity);
        return _dbContext.SaveChangesAsync();
    }

    public Task<List<Shift>> ListAllAsync()
    {
        return _dbContext.Shifts.ToListAsync();
    }

    public Task<List<Shift>> ListByDepartmentAsync(int departmentId)
    {
        return _dbContext.Shifts
            .Include(s => s.User)
            .Where(s => s.User.DepartmentId == departmentId)
            .ToListAsync();
    }

    public async Task<Shift?> FindByIdAsync(int id)
    {
        return await _dbContext.Shifts.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Shift> SaveAsync(Shift entity)
    {
        if (entity.Id == 0)
        {
            await _dbContext.Shifts.AddAsync(entity);
        }
        else
        {
            _dbContext.Shifts.Update(entity);
        }
        
        await _dbContext.SaveChangesAsync();
        return entity;
    }
}
