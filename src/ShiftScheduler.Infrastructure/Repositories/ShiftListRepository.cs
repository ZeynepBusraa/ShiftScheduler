using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShiftScheduler.Application.Repositories;
using ShiftScheduler.Domain.Entities;
using ShiftScheduler.Domain.Enums;
using ShiftScheduler.Infrastructure.Persistence;

namespace ShiftScheduler.Infrastructure.Repositories;

public class ShiftListRepository(AppDbContext context) : IShiftListRepository
{
    private readonly AppDbContext _context = context;

    public async Task<ShiftList?> FindByIdAsync(int id)
    {
        return await _context.ShiftLists
            .Include(sl => sl.Shifts)
            .FirstOrDefaultAsync(sl => sl.Id == id);
    }

    public async Task<List<ShiftList>> ListByDepartmentAsync(int departmentId)
    {
        return await _context.ShiftLists
            .Include(sl => sl.PreparedByUser)
            .Where(sl => sl.PreparedByUser != null && sl.PreparedByUser.DepartmentId == departmentId)
            .ToListAsync();
    }

    public async Task<List<ShiftList>> ListAllAsync()
    {
        return await _context.ShiftLists.ToListAsync();
    }

    public async Task<List<ShiftList>> ListPendingApprovalAsync()
    {
        // Onay bekleyen listeleri getirir
        return await _context.ShiftLists
            .Where(sl => sl.Status == ApprovalStatus.OnayaSunuldu)
            .ToListAsync();
    }

    public async Task<ShiftList?> FindByMonthAsync(int year, int month, int departmentId, ShiftListType type)
    {
        return await _context.ShiftLists
            .Include(sl => sl.PreparedByUser)
            .FirstOrDefaultAsync(sl => 
                sl.Year == year && 
                sl.Month == month && 
                sl.PreparedByUser != null && 
                sl.PreparedByUser.DepartmentId == departmentId);
    }

    public async Task<ShiftList> SaveAsync(ShiftList shiftList)
    {
        if (shiftList.Id == 0)
            await _context.ShiftLists.AddAsync(shiftList);
        else
            _context.ShiftLists.Update(shiftList);

        await _context.SaveChangesAsync();
        return shiftList;
    }
}