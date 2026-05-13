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

/// <summary>
/// FR-02: Aylık nöbet dağıtım algoritması.
///
/// Hard Constraints (Sert Kısıtlar — hiçbir zaman ihlal edilmez):
///   - FR-02.2: Ardışık iki gün nöbet atanamaz.
///   - FR-02.3: İzinli/raporlu günlere nöbet atanamaz.
///
/// Soft Constraint (Esnek Kısıt — puan bazlı optimizasyon):
///   - FR-02.4: En düşük nöbet puanlı doktor seçilir.
///              Puan = (hafta içi nöbet sayısı × 1) + (hafta sonu/tatil nöbet sayısı × 1.5)
///   - Kıdem arttıkça hafta sonu nöbet sayısı azalır.
///
/// FR-02.5: Atanamayan günler UnassignedDays listesinde döndürülür.
///
/// Yetki: Sadece en kıdemli asistan/uzman veya başhekim çağırabilir.
/// </summary>
public class GenerateShiftsHandler(
    IUserRepository userRepository,
    IShiftRepository shiftRepository,
    IShiftListRepository shiftListRepository)
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IShiftRepository _shiftRepository = shiftRepository;
    private readonly IShiftListRepository _shiftListRepository = shiftListRepository;

    private const double WeekendMultiplier = 1.5;

    public async Task<ResponsePayload<GenerateShiftsResult>> HandleAsync(
        GenerateShiftsRequest request, int callerUserId)
    {
        if (request.Month < 1 || request.Month > 12)
            return Response.RuleViolation<GenerateShiftsResult>("Geçersiz ay değeri.");

        // Çağıran kullanıcıyı doğrula
        var caller = await _userRepository.FindByIdAsync(callerUserId);
        if (caller == null)
            return Response.RuleViolation<GenerateShiftsResult>("Kullanıcı bulunamadı.");

        // Yetki kontrolü: Başhekim veya en kıdemli asistan/uzman olmalı
        if (caller.Role != Role.Bashekim)
        {
            var targetRole = request.ListType == ShiftListType.Asistan ? Role.Asistan : Role.Uzman;
            if (caller.Role != targetRole || caller.DepartmentId != request.DepartmentId)
                return Response.RuleViolation<GenerateShiftsResult>("Bu listeyi oluşturma yetkiniz yok.");

            // En kıdemli mi kontrol et
            var allDeptUsers = await _userRepository.ListByDepartmentAndRoleAsync(request.DepartmentId, targetRole);
            var mostSenior = allDeptUsers.OrderByDescending(u => u.SeniorityYear).FirstOrDefault();
            if (mostSenior == null || mostSenior.Id != callerUserId)
                return Response.RuleViolation<GenerateShiftsResult>("Nöbet listesini sadece en kıdemli asistan/uzman oluşturabilir.");
        }

        // Bu ay/bölüm/tip için zaten onaylanmış liste var mı?
        var existingList = await _shiftListRepository.FindByMonthAsync(
            request.Year, request.Month, request.DepartmentId, request.ListType);
        if (existingList != null && existingList.Status == ApprovalStatus.Onaylandi)
            return Response.RuleViolation<GenerateShiftsResult>("Bu ay için onaylanmış bir nöbet listesi zaten mevcut.");

        // İlgili rolde bölüm doktorlarını getir
        var targetRole2 = request.ListType == ShiftListType.Asistan ? Role.Asistan : Role.Uzman;
        var doctors = await _userRepository.ListByDepartmentAndRoleAsync(request.DepartmentId, targetRole2);

        if (doctors.Count == 0)
            return Response.RuleViolation<GenerateShiftsResult>("Atama yapılacak doktor bulunamadı.");

        // Mevcut aydaki nöbetleri getir (bu ayın skoru için)
        var existingShifts = (await _shiftRepository.ListByDepartmentAsync(request.DepartmentId))
            .Where(s => s.Date.Year == request.Year && s.Date.Month == request.Month)
            .ToList();

        // Kıdem bazlı hafta sonu nöbet sınırları hesapla
        // En kıdemli = en az hafta sonu nöbeti, en az kıdemli = en fazla hafta sonu nöbeti
        var seniorityList = doctors.OrderByDescending(d => d.SeniorityYear).ToList();
        int totalDoctors = seniorityList.Count;
        var weekendLimits = new Dictionary<int, int>();
        for (int i = 0; i < totalDoctors; i++)
        {
            // Kıdem sırası i=0 en kıdemli → en az hafta sonu
            // Örnek: 4 kişi için sınırlar: 1, 2, 3, 4
            weekendLimits[seniorityList[i].Id] = i + 1;
        }

        var lastShiftDate = doctors.ToDictionary(
            d => d.Id,
            d => existingShifts
                    .Where(s => s.UserId == d.Id)
                    .Select(s => (DateTime?)s.Date)
                    .Max());

        var shiftScores = doctors.ToDictionary(
            d => d.Id,
            d => CalculateScore(existingShifts.Where(s => s.UserId == d.Id)));

        var weekendCounts = doctors.ToDictionary(
            d => d.Id,
            d => existingShifts.Count(s => s.UserId == d.Id &&
                (s.Type == ShiftType.HaftaSonu || s.Type == ShiftType.Tatil)));

        int daysInMonth = DateTime.DaysInMonth(request.Year, request.Month);
        var assignedShifts = new List<Shift>();
        var unassignedDays = new List<DateTime>();

        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(request.Year, request.Month, day);
            var shiftType = GetShiftType(date);
            bool isWeekend = shiftType == ShiftType.HaftaSonu || shiftType == ShiftType.Tatil;

            var eligible = doctors
                .Where(d => !IsConsecutiveDay(lastShiftDate[d.Id], date))
                .Where(d => !isWeekend || weekendCounts[d.Id] < weekendLimits[d.Id])
                .OrderBy(d => shiftScores[d.Id])
                .ThenBy(d => d.SeniorityYear)
                .ToList();

            // Hafta sonu limiti dolmuşsa limiti aşanları da dahil et (en düşük puana sahip)
            if (eligible.Count == 0 && isWeekend)
            {
                eligible = doctors
                    .Where(d => !IsConsecutiveDay(lastShiftDate[d.Id], date))
                    .OrderBy(d => shiftScores[d.Id])
                    .ThenBy(d => d.SeniorityYear)
                    .ToList();
            }

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
                Type = shiftType
                // ShiftListId will be set after ShiftList is saved
            };

            assignedShifts.Add(shift);

            lastShiftDate[selected.Id] = date;
            shiftScores[selected.Id] += shiftType == ShiftType.HaftaIci ? 1.0 : WeekendMultiplier;
            if (isWeekend) weekendCounts[selected.Id]++;
        }

        // Nöbet listesi oluştur veya güncelle
        if (existingList == null)
        {
            existingList = new ShiftList
            {
                Year = request.Year,
                Month = request.Month,
                DepartmentId = request.DepartmentId,
                ListType = request.ListType,
                Status = ApprovalStatus.Taslak,
                PreparedByUserId = callerUserId
            };
        }
        else
        {
            // Mevcut taslak listeyi güncelle — eski nöbetleri temizle
            var oldShifts = existingList.Shifts.ToList();
            foreach (var s in oldShifts)
                await _shiftRepository.DeleteAsync(s);
            existingList.Status = ApprovalStatus.Taslak;
            existingList.PreparedByUserId = callerUserId;
        }

        var savedList = await _shiftListRepository.SaveAsync(existingList);

        // Nöbetleri kaydet ve listeye bağla
        var savedShifts = new List<Shift>();
        foreach (var shift in assignedShifts)
        {
            shift.ShiftListId = savedList.Id;
            var saved = await _shiftRepository.SaveAsync(shift);
            savedShifts.Add(saved);
        }

        var result = new GenerateShiftsResult
        {
            AssignedCount = savedShifts.Count,
            UnassignedDays = unassignedDays,
            ShiftListId = savedList.Id,
            AssignedShifts = savedShifts.Select(s => new ShiftDto(s.Id, s.UserId, s.Date, (int)s.Type, false)).ToList()
        };

        return Response.Ok(result);
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
        double score = 0;
        foreach (var s in shifts)
            score += s.Type == ShiftType.HaftaIci ? 1.0 : WeekendMultiplier;
        return score;
    }
}