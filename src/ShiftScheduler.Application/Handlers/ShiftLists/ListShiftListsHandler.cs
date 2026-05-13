using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShiftScheduler.Application.Common;
using ShiftScheduler.Application.DTOs;
using ShiftScheduler.Application.Repositories;
using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Application.Handlers.ShiftLists;

/// <summary>
/// Nöbet listelerini getirir.
/// - Başhekim: Tüm bölümlerin onaya sunulmuş listelerini görür.
/// - Uzman/Asistan: Sadece kendi bölümünün listelerini görür.
/// </summary>
public class ListShiftListsHandler(IShiftListRepository shiftListRepository)
{
    private readonly IShiftListRepository _shiftListRepository = shiftListRepository;

    public async Task<ResponsePayload<List<ShiftListDto>>> HandleAsync(Role userRole, int? departmentId)
    {
        List<ShiftScheduler.Domain.Entities.ShiftList> lists;

        if (userRole == Role.Bashekim)
        {
            lists = await _shiftListRepository.ListAllAsync();
        }
        else
        {
            if (!departmentId.HasValue)
                return Response.Ok(new List<ShiftListDto>());
            lists = await _shiftListRepository.ListByDepartmentAsync(departmentId.Value);
        }

        var dtos = lists.Select(sl => new ShiftListDto
        {
            Id = sl.Id,
            Year = sl.Year,
            Month = sl.Month,
            DepartmentId = sl.DepartmentId,
            DepartmentName = sl.Department?.Name ?? string.Empty,
            ListType = (int)sl.ListType,
            Status = (int)sl.Status,
            PreparedByUserId = sl.PreparedByUserId,
            PreparedByUserName = sl.PreparedByUser?.FullName ?? string.Empty,
            Shifts = sl.Shifts.Select(s => new ShiftDto(s.Id, s.UserId, s.Date, (int)s.Type, 
                sl.Status == ApprovalStatus.Onaylandi)).ToList()
        }).ToList();

        return Response.Ok(dtos);
    }
}
