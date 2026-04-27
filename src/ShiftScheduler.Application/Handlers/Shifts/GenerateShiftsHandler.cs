using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShiftScheduler.Application.Common;
using ShiftScheduler.Application.DTOs;
using ShiftScheduler.Application.Repositories;
using ShiftScheduler.Domain.Entities;
using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Application.Handlers.Shifts;

public class GenerateShiftsHandler(
    IUserRepository userRepository,
    IShiftRepository shiftRepository,
    IShiftRequestRepository shiftRequestRepository) // İzin kontrolü için eklendi
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IShiftRepository _shiftRepository = shiftRepository;
    private readonly IShiftRequestRepository _shiftRequestRepository = shiftRequestRepository;

    // FR-02.4: Hafta sonu katsayısı (Görseldeki kurala göre ağırlıklandırıldı)
    private const double WeekendMultiplier = 5.0; 

    public async Task<ResponsePayload<GenerateShiftsResult>> HandleAsync(GenerateShiftsRequest request)
    {
        if (request.Month < 1 || request.Month > 12)
            return Response.RuleViolation<GenerateShiftsResult>("Geçersiz ay değeri.");

        var allUsers = await _userRepository.ListAllAsync();
        // Sadece asistanlar (Görsel kuralı: Uzman listesi ayrı tutulur)
        var doctors = allUsers
            .Where(u => u.Role == Role.Asistan && (!request.DepartmentId.HasValue || u.DepartmentId == request.DepartmentId.Value))
            .ToList();

        if (doctors.Count == 0)
            return Response.RuleViolation<GenerateShiftsResult>("Atama yapılacak asistan bulunamadı.");

        // O aya ait tüm izin taleplerini getir (FR-03.3 Çözümü)
        var allRequests = await _shiftRequestRepository.ListAllAsync();
        var approvedRequests = allRequests
            .Where(r => r.Status == RequestStatus.Approved && r.RequestDate.Month == request.Month && r.RequestDate.Year == request.Year)
            .ToList();

        var existingShifts = (request.DepartmentId.HasValue
                ? await _shiftRepository.ListByDepartmentAsync(request.DepartmentId.Value)
                : await _shiftRepository.ListAllAsync())
            .Where(s => s.Date.Year == request.Year)
            .ToList();

        var lastShiftDate = doctors.ToDictionary(
            d => d.Id,
            d => existingShifts.Where(s => s.UserId == d.Id).Select(s => (DateTime?)s.Date).Max());

        var shiftScores = doctors.ToDictionary(
            d => d.Id,
            d => CalculateScore(existingShifts.Where(s => s.UserId == d.Id)));

        int daysInMonth = DateTime.DaysInMonth(request.Year, request.Month);
        var assignedShifts = new List<Shift>();
        var unassignedDays = new List<DateTime>();

        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(request.Year, request.Month, day);
            var shiftType = GetShiftType(date);
            bool isWeekend = shiftType == ShiftType.HaftaSonu;

            // FR-02.3: O gün izinli olanları belirle
            var busyUserIds = approvedRequests
                .Where(r => r.RequestDate.Date == date.Date)
                .Select(r => r.UserId)
                .ToList();

            // UYGUN ADAYLARI FİLTRELE
            var eligible = doctors
                .Where(d => !IsConsecutiveDay(lastShiftDate[d.Id], date)) // FR-02.2 (Ardışık Gün)
                .Where(d => !busyUserIds.Contains(d.Id))                 // FR-02.3 (İzin Kontrolü)
                .OrderBy(d => {
                    // PUANLAMA MANTIĞI (Görseldeki kurallar)
                    double baseScore = shiftScores[d.Id];
                    // Hafta sonu ise kıdemliyi korumak için puanı katla
                    double seniorityEffect = d.SeniorityYear * (isWeekend ? 20.0 : 5.0); 
                    return baseScore + seniorityEffect;
                })
                .ToList();

            if (eligible.Count == 0)
            {
                unassignedDays.Add(date);
                continue;
            }

            var selected = eligible.First();

            var shift = new Shift
            {
                UserId = selected.Id,
                Date = date,
                Type = shiftType,
                IsApproved = false // Görsel: Başhekim onayına düşecek
            };

            var saved = await _shiftRepository.SaveAsync(shift);
            assignedShifts.Add(saved);

            // Durumu güncelle
            lastShiftDate[selected.Id] = date;
            shiftScores[selected.Id] += (shiftType == ShiftType.HaftaIci ? 1.0 : WeekendMultiplier);
        }

        return Response.Ok(new GenerateShiftsResult
        {
            AssignedCount = assignedShifts.Count,
            UnassignedDays = unassignedDays,
            AssignedShifts = assignedShifts.Select(s => new ShiftDto(s.Id, s.UserId, s.Date, (int)s.Type, s.IsApproved)).ToList()
        });
    }

    private static bool IsConsecutiveDay(DateTime? lastShift, DateTime today)
    {
        if (!lastShift.HasValue) return false;
        return lastShift.Value.Date == today.AddDays(-1).Date;
    }

    private static ShiftType GetShiftType(DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday
            ? ShiftType.HaftaSonu
            : ShiftType.HaftaIci;
    }

    private static double CalculateScore(IEnumerable<Shift> shifts)
    {
        return shifts.Sum(s => s.Type == ShiftType.HaftaIci ? 1.0 : WeekendMultiplier);
    }
}
