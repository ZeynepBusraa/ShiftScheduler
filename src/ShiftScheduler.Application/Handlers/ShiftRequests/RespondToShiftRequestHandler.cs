using System.Threading.Tasks;
using ShiftScheduler.Application.Common;
using ShiftScheduler.Application.DTOs;
using ShiftScheduler.Application.Repositories;
using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Application.Handlers.ShiftRequests;

/// <summary>
/// FR-03.4: Karşı taraftaki asistanın talebi kabul veya reddetmesi.
/// Kabul → Status = AsistanOnayladi (başhekim paneline düşer)
/// Red   → Status = Reddedildi
/// </summary>
public class RespondToShiftRequestHandler(IShiftRequestRepository requestRepository)
{
    private readonly IShiftRequestRepository _requestRepository = requestRepository;

    public async Task<ResponsePayload<ShiftRequestDto>> HandleAsync(
        int requestId, int respondingDoctorId, bool accept)
    {
        var request = await _requestRepository.FindByIdAsync(requestId);

        if (request == null)
            return Response.RuleViolation<ShiftRequestDto>("Talep bulunamadı.");

        // Sadece hedef doktor cevap verebilir
        if (request.TargetDoctorId != respondingDoctorId)
            return Response.RuleViolation<ShiftRequestDto>("Bu talebi yanıtlama yetkiniz yok.");

        // Sadece 'Bekliyor' durumundaki talepler yanıtlanabilir
        if (request.Status != RequestStatus.Bekliyor)
            return Response.RuleViolation<ShiftRequestDto>("Bu talep zaten yanıtlanmış.");

        request.Status = accept ? RequestStatus.AsistanOnayladi : RequestStatus.Reddedildi;

        var saved = await _requestRepository.SaveAsync(request);

        return Response.Ok(new ShiftRequestDto
        {
            Id             = saved.Id,
            RequesterId    = saved.RequesterId,
            TargetDoctorId = saved.TargetDoctorId,
            ShiftId        = saved.ShiftId,
            Status         = (int)saved.Status
        });
    }
}
