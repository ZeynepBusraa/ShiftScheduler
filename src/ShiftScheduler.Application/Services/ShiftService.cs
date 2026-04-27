using System.Collections.Generic;
using System.Threading.Tasks;
using ShiftScheduler.Application.Common;
using ShiftScheduler.Application.DTOs;
using ShiftScheduler.Application.Handlers.Shifts;
using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Application.Services;

public class ShiftService(
    FindShiftHandler findHandler,
    ListShiftsHandler listHandler,
    SaveShiftHandler saveHandler,
    GenerateShiftsHandler generateHandler) : IShiftService
{
    private readonly FindShiftHandler _findHandler = findHandler;
    private readonly ListShiftsHandler _listHandler = listHandler;
    private readonly SaveShiftHandler _saveHandler = saveHandler;
    private readonly GenerateShiftsHandler _generateHandler = generateHandler;

    public Task<ResponsePayload<ShiftDto>> FindAsync(int id)
        => _findHandler.HandleAsync(id);

    public Task<ResponsePayload<List<ShiftDto>>> ListAsync(Role userRole, int? departmentId)
        => _listHandler.HandleAsync(userRole, departmentId);

    public Task<ResponsePayload<ShiftDto>> SaveAsync(ShiftDto dto)
        => _saveHandler.HandleAsync(dto);

    public Task<ResponsePayload<GenerateShiftsResult>> GenerateAsync(GenerateShiftsRequest request)
        => _generateHandler.HandleAsync(request);
}

