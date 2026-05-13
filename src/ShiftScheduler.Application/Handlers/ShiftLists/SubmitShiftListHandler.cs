using System.Threading.Tasks;
using ShiftScheduler.Application.Common;
using ShiftScheduler.Application.DTOs;
using ShiftScheduler.Application.Repositories;
using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Application.Handlers.ShiftLists;

/// <summary>
/// Nöbet listesini başhekime onaya sunar.
/// Sadece listeyi hazırlayan kişi onaya sunabilir ve liste Taslak durumunda olmalı.
/// </summary>
public class SubmitShiftListHandler(
    IShiftListRepository shiftListRepository,
    IUserRepository userRepository)
{
    private readonly IShiftListRepository _shiftListRepository = shiftListRepository;
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<ResponsePayload<ShiftListDto>> HandleAsync(int listId, int callerUserId)
    {
        var list = await _shiftListRepository.FindByIdAsync(listId);
        if (list == null)
            return Response.RuleViolation<ShiftListDto>("Nöbet listesi bulunamadı.");

        if (list.Status != ApprovalStatus.Taslak)
            return Response.RuleViolation<ShiftListDto>("Yalnızca taslak durumundaki listeler onaya sunulabilir.");

        // Sadece listeyi hazırlayan kişi onaya sunabilir
        if (list.PreparedByUserId != callerUserId)
        {
            // Veya başhekim
            var caller = await _userRepository.FindByIdAsync(callerUserId);
            if (caller?.Role != Role.Bashekim)
                return Response.RuleViolation<ShiftListDto>("Bu listeyi onaya sunma yetkiniz yok.");
        }

        list.Status = ApprovalStatus.OnayaSunuldu;
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
