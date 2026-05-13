using System;
using System.Threading.Tasks;
using ShiftScheduler.Application.Common;
using ShiftScheduler.Application.DTOs;
using ShiftScheduler.Application.Repositories;
using ShiftScheduler.Domain.Entities;
using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Application.Handlers.ShiftRequests;

/// <summary>
/// FR-03.3: Asistan/Uzman doktorların nöbet değişim talebi oluşturması.
/// - Ay içi en fazla 2 talep oluşturulabilir.
/// - Talep önce en kıdemli asistan/uzmana gider (Status = Bekliyor).
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

        // Talep eden kişiyi getir
        var requester = await _userRepository.FindByIdAsync(requesterId);
        if (requester == null)
            return Response.RuleViolation<ShiftRequestDto>("Kullanıcı bulunamadı.");

        // Hedef doktor var mı?
        var targetDoctor = await _userRepository.FindByIdAsync(dto.TargetDoctorId);
        if (targetDoctor == null)
            return Response.RuleViolation<ShiftRequestDto>("Hedef doktor bulunamadı.");

        // Aynı doktora talep yapılamaz
        if (requesterId == dto.TargetDoctorId)
            return Response.RuleViolation<ShiftRequestDto>("Kendinizle nöbet değişimi yapamazsınız.");

        // Aynı bölümden olmalı
        if (requester.DepartmentId != targetDoctor.DepartmentId)
            return Response.RuleViolation<ShiftRequestDto>("Yalnızca aynı bölümdeki doktorlarla nöbet değişimi yapabilirsiniz.");

        // Aynı rol olmalı (asistan-asistan, uzman-uzman)
        if (requester.Role != targetDoctor.Role)
            return Response.RuleViolation<ShiftRequestDto>("Yalnızca aynı roldeki doktorlarla nöbet değişimi yapabilirsiniz.");

        // Ay içi max 2 talep kontrolü
        var shiftDate = shift.Date;
        var monthStart = new DateTime(shiftDate.Year, shiftDate.Month, 1);
        var monthEnd = monthStart.AddMonths(1);

        var allRequests = await _requestRepository.ListByRequesterAsync(requesterId);
        int monthlyCount = allRequests.Count(r =>
            r.Status != RequestStatus.Reddedildi);

        // Nöbet tarihine bakarak ay bazlı say
        var monthlyActiveRequests = 0;
        foreach (var req in allRequests)
        {
            if (req.Status == RequestStatus.Reddedildi) continue;
            var reqShift = await _shiftRepository.FindByIdAsync(req.ShiftId);
            if (reqShift != null && reqShift.Date >= monthStart && reqShift.Date < monthEnd)
                monthlyActiveRequests++;
        }

        if (monthlyActiveRequests >= 2)
            return Response.RuleViolation<ShiftRequestDto>(
                $"{shiftDate:MMMM yyyy} ayı için zaten 2 değişim talebiniz mevcut. Ay içi maksimum 2 talep oluşturabilirsiniz.");

        // Aynı nöbet için zaten bekleyen bir talep var mı?
        bool alreadyPending = allRequests.Exists(r =>
            r.ShiftId == dto.ShiftId &&
            (r.Status == RequestStatus.Bekliyor || r.Status == RequestStatus.KidemliOnayladi));

        if (alreadyPending)
            return Response.RuleViolation<ShiftRequestDto>("Bu nöbet için zaten bekleyen bir değişim talebiniz var.");

        var request = new ShiftRequest
        {
            RequesterId = requesterId,
            TargetDoctorId = dto.TargetDoctorId,
            ShiftId = dto.ShiftId,
            Status = RequestStatus.Bekliyor
        };

        var saved = await _requestRepository.SaveAsync(request);

        return Response.SaveSuccess(new ShiftRequestDto
        {
            Id = saved.Id,
            RequesterId = saved.RequesterId,
            TargetDoctorId = saved.TargetDoctorId,
            ShiftId = saved.ShiftId,
            Status = (int)saved.Status
        });
    }
}
