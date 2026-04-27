using System.Collections.Generic;
using System.Threading.Tasks;
using ShiftScheduler.Domain.Entities;

namespace ShiftScheduler.Application.Repositories;

public interface IShiftRequestRepository
{
    Task<ShiftRequest?> FindByIdAsync(int id);

    /// <summary>Bir doktorun oluşturduğu tüm talepleri getirir.</summary>
    Task<List<ShiftRequest>> ListByRequesterAsync(int requesterId);

    /// <summary>Bir doktora gelen ve asistan onayı bekleyen talepleri getirir.</summary>
    Task<List<ShiftRequest>> ListPendingForTargetDoctorAsync(int targetDoctorId);

    /// <summary>Başhekim onayı bekleyen tüm talepleri getirir.</summary>
    Task<List<ShiftRequest>> ListPendingChiefApprovalAsync();

    Task<ShiftRequest> SaveAsync(ShiftRequest entity);
}
