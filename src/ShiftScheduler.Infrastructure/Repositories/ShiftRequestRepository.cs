using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShiftScheduler.Application.Repositories;
using ShiftScheduler.Domain.Entities;
using ShiftScheduler.Domain.Enums;
using ShiftScheduler.Infrastructure.Persistence;

namespace ShiftScheduler.Infrastructure.Repositories;

public class ShiftRequestRepository(AppDbContext dbContext) : IShiftRequestRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public Task<ShiftRequest?> FindByIdAsync(int id)
        => _dbContext.ShiftRequests.FirstOrDefaultAsync(r => r.Id == id);

    public Task<List<ShiftRequest>> ListByRequesterAsync(int requesterId)
        => _dbContext.ShiftRequests
            .Where(r => r.RequesterId == requesterId)
            .ToListAsync();

    public Task<List<ShiftRequest>> ListPendingForTargetDoctorAsync(int targetDoctorId)
        => _dbContext.ShiftRequests
            .Where(r => r.TargetDoctorId == targetDoctorId && r.Status == RequestStatus.Bekliyor)
            .ToListAsync();

    public Task<List<ShiftRequest>> ListPendingChiefApprovalAsync()
        => _dbContext.ShiftRequests
            .Where(r => r.Status == RequestStatus.KidemliOnayladi)
            .ToListAsync();

    public async Task<ShiftRequest> SaveAsync(ShiftRequest entity)
    {
        if (entity.Id == 0)
            await _dbContext.ShiftRequests.AddAsync(entity);
        else
            _dbContext.ShiftRequests.Update(entity);

        await _dbContext.SaveChangesAsync();
        return entity;
    }
}
