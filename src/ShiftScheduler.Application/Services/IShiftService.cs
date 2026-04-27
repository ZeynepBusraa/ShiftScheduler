using System.Collections.Generic;
using System.Threading.Tasks;
using ShiftScheduler.Application.Common;
using ShiftScheduler.Application.DTOs;

using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Application.Services;

public interface IShiftService
{
    Task<ResponsePayload<ShiftDto>> FindAsync(int id);
    Task<ResponsePayload<List<ShiftDto>>> ListAsync(Role userRole, int? departmentId);
    Task<ResponsePayload<ShiftDto>> SaveAsync(ShiftDto dto);
    Task<ResponsePayload<GenerateShiftsResult>> GenerateAsync(GenerateShiftsRequest request);
}
