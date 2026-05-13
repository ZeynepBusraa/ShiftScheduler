using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShiftScheduler.Domain.Entities;
using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Application.Repositories;

public interface IShiftListRepository
{
    Task<ShiftList?> FindByIdAsync(int id);

    /// <summary>Bir bölüme ait tüm nöbet listelerini getirir.</summary>
    Task<List<ShiftList>> ListByDepartmentAsync(int departmentId);

    /// <summary>Tüm bölümlerin nöbet listelerini getirir (Başhekim için).</summary>
    Task<List<ShiftList>> ListAllAsync();

    /// <summary>Başhekim onayı bekleyen listeleri getirir.</summary>
    Task<List<ShiftList>> ListPendingApprovalAsync();

    /// <summary>Belirli yıl/ay/bölüm/tip için liste getirir.</summary>
    Task<ShiftList?> FindByMonthAsync(int year, int month, int departmentId, ShiftListType listType);

    Task<ShiftList> SaveAsync(ShiftList entity);
}
