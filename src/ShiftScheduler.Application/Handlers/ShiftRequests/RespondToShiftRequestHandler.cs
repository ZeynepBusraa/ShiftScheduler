using System.Linq;
using System.Threading.Tasks;
using ShiftScheduler.Application.Common;
using ShiftScheduler.Application.DTOs;
using ShiftScheduler.Application.Repositories;
using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Application.Handlers.ShiftRequests;

/// <summary>
/// Nöbet değişim talebine yanıt verme — 3 aşamalı iş akışı:
///
/// Adım 1 — Hedef doktor yanıt verir (Bekliyor → HedefOnayladi veya Reddedildi)
///   - Talep hedef doktora (TargetDoctorId) gider; o kabul ederse HedefOnayladi olur.
///   - Reddederse Reddedildi olur.
///
/// Adım 2 — En kıdemli asistan/uzman onaylar (HedefOnayladi → KidemliOnayladi veya Reddedildi)
///   - En kıdemli asistan/uzman onaylarsa KidemliOnayladi olur → başhekim paneline düşer.
///   - En kıdemli, talep sahibi ya da hedef doktorsa bu adımı atlayıp direkt KidemliOnayladi sayılır.
///
/// Adım 3 — Başhekim onaylar (ApproveShiftRequestHandler'da yapılır).
/// </summary>
public class RespondToShiftRequestHandler(
    IShiftRequestRepository requestRepository,
    IUserRepository userRepository)
{
    private readonly IShiftRequestRepository _requestRepository = requestRepository;
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<ResponsePayload<ShiftRequestDto>> HandleAsync(
        int requestId, int respondingUserId, bool accept)
    {
        var request = await _requestRepository.FindByIdAsync(requestId);

        if (request == null)
            return Response.RuleViolation<ShiftRequestDto>("Talep bulunamadı.");

        var respondingUser = await _userRepository.FindByIdAsync(respondingUserId);
        if (respondingUser == null)
            return Response.RuleViolation<ShiftRequestDto>("Kullanıcı bulunamadı.");

        // ─── ADIM 1: Hedef doktor yanıt veriyor (Bekliyor → HedefOnayladi veya Reddedildi) ───
        if (request.Status == RequestStatus.Bekliyor)
        {
            if (request.TargetDoctorId != respondingUserId)
                return Response.RuleViolation<ShiftRequestDto>("Bu talebi yanıtlama yetkiniz yok. Yalnızca hedef doktor yanıtlayabilir.");

            if (!accept)
            {
                request.Status = RequestStatus.Reddedildi;
                var rejected = await _requestRepository.SaveAsync(request);
                return Response.Ok(MapToDto(rejected));
            }

            // Hedef doktor kabul etti → en kıdemli kontrolü yap
            var requester = await _userRepository.FindByIdAsync(request.RequesterId);
            if (requester == null || !requester.DepartmentId.HasValue)
            {
                request.Status = RequestStatus.HedefOnayladi;
                var saved = await _requestRepository.SaveAsync(request);
                return Response.Ok(MapToDto(saved));
            }

            var deptUsers = await _userRepository.ListByDepartmentAndRoleAsync(
                requester.DepartmentId.Value, requester.Role);
            var mostSenior = deptUsers.OrderByDescending(u => u.SeniorityYear).FirstOrDefault();

            // Eğer en kıdemli; talep sahibi ya da hedef doktorsa → kıdemli onayı atlanır
            bool seniorIsParty = mostSenior != null &&
                (mostSenior.Id == request.RequesterId || mostSenior.Id == request.TargetDoctorId);

            request.Status = seniorIsParty
                ? RequestStatus.KidemliOnayladi   // kıdemli zaten taraf → başhekime direkt git
                : RequestStatus.HedefOnayladi;     // kıdemlinin onayı bekleniyor

            var result = await _requestRepository.SaveAsync(request);
            return Response.Ok(MapToDto(result));
        }

        // ─── ADIM 2: En kıdemli asistan/uzman onaylıyor (HedefOnayladi → KidemliOnayladi veya Reddedildi) ───
        if (request.Status == RequestStatus.HedefOnayladi)
        {
            var requester = await _userRepository.FindByIdAsync(request.RequesterId);
            if (requester == null || !requester.DepartmentId.HasValue)
                return Response.RuleViolation<ShiftRequestDto>("Talep sahibi bilgisi bulunamadı.");

            var deptUsers = await _userRepository.ListByDepartmentAndRoleAsync(
                requester.DepartmentId.Value, requester.Role);
            var mostSenior = deptUsers.OrderByDescending(u => u.SeniorityYear).FirstOrDefault();

            if (mostSenior == null || mostSenior.Id != respondingUserId)
                return Response.RuleViolation<ShiftRequestDto>(
                    "Bu talebi onaylama yetkisi sadece bölümün en kıdemli asistan/uzmanına aittir.");

            request.Status = accept ? RequestStatus.KidemliOnayladi : RequestStatus.Reddedildi;

            var saved = await _requestRepository.SaveAsync(request);
            return Response.Ok(MapToDto(saved));
        }

        return Response.RuleViolation<ShiftRequestDto>("Bu talep zaten sonuçlanmış ya da başhekim onayı aşamasında.");
    }

    private static ShiftRequestDto MapToDto(Domain.Entities.ShiftRequest r) => new()
    {
        Id = r.Id,
        RequesterId = r.RequesterId,
        TargetDoctorId = r.TargetDoctorId,
        ShiftId = r.ShiftId,
        Status = (int)r.Status
    };
}
