using System.Collections.Generic;
using System.Threading.Tasks;
using ShiftScheduler.Domain.Entities;

namespace ShiftScheduler.Application.Repositories;

public interface IShiftRepository
{
    Task<Shift?> FindByIdAsync(int id);
    Task<List<Shift>> ListAllAsync();
    Task<List<Shift>> ListByDepartmentAsync(int departmentId);
    Task<Shift> SaveAsync(Shift entity);
    Task DeleteAsync(Shift entity);
}
