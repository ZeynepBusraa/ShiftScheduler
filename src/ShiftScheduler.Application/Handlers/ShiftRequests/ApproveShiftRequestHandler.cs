using System.Threading.Tasks;
using ShiftScheduler.Application.Common;
using ShiftScheduler.Application.DTOs;
using ShiftScheduler.Application.Repositories;
using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Application.Handlers.ShiftRequests;

/// <summary>
/// FR-03.5: Başhekim talebi onaylarsa nöbetleri gerçek anlamda takas eder
/// ve veritabanına işler. Sadece KidemliOnayladi durumundaki talepler başhekime gelir.
/// </summary>
public class ApproveShiftRequestHandler(
    IShiftRequestRepository requestRepository,
    IShiftRepository shiftRepository)
{
    private readonly IShiftRequestRepository _requestRepository = requestRepository;
    private readonly IShiftRepository _shiftRepository = shiftRepository;

    public async Task<ResponsePayload<ShiftRequestDto>> HandleAsync(int requestId, bool approve)
    {
        var request = await _requestRepository.FindByIdAsync(requestId);

        if (request == null)
            return Response.RuleViolation<ShiftRequestDto>("Talep bulunamadı.");

        // Sadece en kıdemli tarafından onaylanmış talepler başhekime gelir
        if (request.Status != RequestStatus.KidemliOnayladi)
            return Response.RuleViolation<ShiftRequestDto>("Bu talep başhekim onayı aşamasında değil.");

        if (!approve)
        {
            request.Status = RequestStatus.Reddedildi;
            var rejected = await _requestRepository.SaveAsync(request);
            return Response.Ok(MapToDto(rejected));
        }

        // FR-03.5: Nöbeti takas et — nöbetin sahibini değiştir
        var shift = await _shiftRepository.FindByIdAsync(request.ShiftId);
        if (shift == null)
            return Response.RuleViolation<ShiftRequestDto>("İlgili nöbet bulunamadı.");

        // Nöbetin sahibini hedef doktora değiştir
        shift.UserId = request.TargetDoctorId;
        await _shiftRepository.SaveAsync(shift);

        request.Status = RequestStatus.BashekimOnayladi;
        var saved = await _requestRepository.SaveAsync(request);

        return Response.Ok(MapToDto(saved));
    }

    private static ShiftRequestDto MapToDto(Domain.Entities.ShiftRequest r) => new()
    {
        Id             = r.Id,
        RequesterId    = r.RequesterId,
        TargetDoctorId = r.TargetDoctorId,
        ShiftId        = r.ShiftId,
        Status         = (int)r.Status
    };
}
