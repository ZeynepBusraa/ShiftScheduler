using System.Threading.Tasks;
using ShiftScheduler.Application.Common;
using ShiftScheduler.Application.DTOs;
using ShiftScheduler.Application.Repositories;
using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Application.Handlers.ShiftLists;

/// <summary>
/// Başhekim nöbet listesini onaylar veya reddeder.
/// - Onaylanırsa: Status = Onaylandi (kesinleşir)
/// - Reddedilirse: Status = Taslak (hazırlayan yeniden düzenleyip tekrar sunabilir)
/// </summary>
public class ApproveShiftListHandler(IShiftListRepository shiftListRepository)
{
    private readonly IShiftListRepository _shiftListRepository = shiftListRepository;

    public async Task<ResponsePayload<ShiftListDto>> HandleAsync(int listId, bool approve)
    {
        var list = await _shiftListRepository.FindByIdAsync(listId);
        if (list == null)
            return Response.RuleViolation<ShiftListDto>("Nöbet listesi bulunamadı.");

        if (list.Status != ApprovalStatus.OnayaSunuldu)
            return Response.RuleViolation<ShiftListDto>("Bu liste başhekim onayı aşamasında değil.");

        // Onaylanırsa kesinleşir; reddedilirse Taslak'a döner → hazırlayan yeniden düzenleyebilir
        list.Status = approve ? ApprovalStatus.Onaylandi : ApprovalStatus.Taslak;
        var saved = await _shiftListRepository.SaveAsync(list);

        return Response.Ok(new ShiftListDto
        {
            Id = saved.Id,
            Year = saved.Year,
            Month = saved.Month,
            DepartmentId = saved.DepartmentId,
            ListType = (int)saved.ListType,
            Status = (int)saved.Status,
            PreparedByUserId = saved.PreparedByUserId
        });
    }
}

