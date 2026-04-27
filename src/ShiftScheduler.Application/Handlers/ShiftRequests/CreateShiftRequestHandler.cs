using System;
using System.Threading.Tasks;
using ShiftScheduler.Application.Common;
using ShiftScheduler.Application.DTOs;
using ShiftScheduler.Application.Repositories;
using ShiftScheduler.Domain.Entities;
using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Application.Handlers.ShiftRequests;

/// <summary>
/// FR-03.3: Asistan doktorların birbirleriyle nöbet değişim talebi oluşturması.
/// Talep önce karşı tarafa gider (Status = Bekliyor).
/// </summary>
public class CreateShiftRequestHandler(
    IShiftRequestRepository requestRepository,
    IShiftRepository shiftRepository,
    IUserRepository userRepository)
{
    private readonly IShiftRequestRepository _requestRepository = requestRepository;
    private readonly IShiftRepository _shiftRepository = shiftRepository;
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<ResponsePayload<ShiftRequestDto>> HandleAsync(int requesterId, CreateShiftRequestDto dto)
    {
        // Nöbet var mı ve talep eden kişiye mi ait?
        var shift = await _shiftRepository.FindByIdAsync(dto.ShiftId);
        if (shift == null || shift.UserId != requesterId)
            return Response.RuleViolation<ShiftRequestDto>("Belirtilen nöbet bulunamadı veya size ait değil.");

        // Hedef doktor var mı?
        var targetDoctor = await _userRepository.FindByIdAsync(dto.TargetDoctorId);
        if (targetDoctor == null)
            return Response.RuleViolation<ShiftRequestDto>("Hedef doktor bulunamadı.");

        // Aynı doktora talep yapılamaz
        if (requesterId == dto.TargetDoctorId)
            return Response.RuleViolation<ShiftRequestDto>("Kendinizle nöbet değişimi yapamazsınız.");

        // Aynı nöbet için zaten bekleyen bir talep var mı?
        var existing = await _requestRepository.ListByRequesterAsync(requesterId);
        bool alreadyPending = existing.Exists(r =>
            r.ShiftId == dto.ShiftId &&
            (r.Status == RequestStatus.Bekliyor || r.Status == RequestStatus.AsistanOnayladi));

        if (alreadyPending)
            return Response.RuleViolation<ShiftRequestDto>("Bu nöbet için zaten bekleyen bir değişim talebiniz var.");

        var request = new ShiftRequest
        {
            RequesterId   = requesterId,
            TargetDoctorId = dto.TargetDoctorId,
            ShiftId       = dto.ShiftId,
            Status        = RequestStatus.Bekliyor
        };

        var saved = await _requestRepository.SaveAsync(request);

        return Response.SaveSuccess(new ShiftRequestDto
        {
            Id             = saved.Id,
            RequesterId    = saved.RequesterId,
            TargetDoctorId = saved.TargetDoctorId,
            ShiftId        = saved.ShiftId,
            Status         = (int)saved.Status
        });
    }
}
