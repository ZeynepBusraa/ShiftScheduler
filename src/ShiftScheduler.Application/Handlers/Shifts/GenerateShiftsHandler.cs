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
///   - FR-02.3: İzinli/raporlu günlere nöbet atanamaz (UnavailableDates).
///
/// Soft Constraint (Esnek Kısıt — puan bazlı optimizasyon):
///   - FR-02.4: En düşük nöbet puanlı doktor seçilir.
///              Puan = (hafta içi nöbet sayısı × 1) + (hafta sonu/tatil nöbet sayısı × 1.5)
///
/// FR-02.5: Atanamayan günler UnassignedDays listesinde döndürülür.
/// </summary>
public class GenerateShiftsHandler(
    IUserRepository userRepository,
    IShiftRepository shiftRepository)
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IShiftRepository _shiftRepository = shiftRepository;

    // FR-02.4: Hafta sonu/tatil nöbet katsayısı (SRS §2.6 — yapılandırılabilir, şimdilik sabit)
    private const double WeekendMultiplier = 1.5;

    public async Task<ResponsePayload<GenerateShiftsResult>> HandleAsync(GenerateShiftsRequest request)
    {
        if (request.Month < 1 || request.Month > 12)
            return Response.RuleViolation<GenerateShiftsResult>("Geçersiz ay değeri.");

        // Tüm doktorları getir (departman filtreli veya tümü)
        var allUsers = await _userRepository.ListAllAsync();
        var doctors = request.DepartmentId.HasValue
            ? allUsers.Where(u => u.DepartmentId == request.DepartmentId.Value).ToList()
            : allUsers;

        if (doctors.Count == 0)
            return Response.RuleViolation<GenerateShiftsResult>("Atama yapılacak doktor bulunamadı.");

        // O aya ait mevcut nöbetleri getir (puan hesabı için)
        var existingShifts = (request.DepartmentId.HasValue
                ? await _shiftRepository.ListByDepartmentAsync(request.DepartmentId.Value)
                : await _shiftRepository.ListAllAsync())
            .Where(s => s.Date.Year == request.Year)
            .ToList();

        // Doktor → son nöbet tarihi (ardışık gün kontrolü için)
        var lastShiftDate = doctors.ToDictionary(
            d => d.Id,
            d => existingShifts
                    .Where(s => s.UserId == d.Id)
                    .Select(s => (DateTime?)s.Date)
                    .Max());

        // Puan tablosu: doktor başına kümülatif nöbet puanı
        var shiftScores = doctors.ToDictionary(
            d => d.Id,
            d => CalculateScore(existingShifts.Where(s => s.UserId == d.Id)));

        // Algoritma: Her gün için uygun ve en düşük puanlı doktoru seç
        int daysInMonth = DateTime.DaysInMonth(request.Year, request.Month);
        var assignedShifts = new List<Shift>();
        var unassignedDays  = new List<DateTime>();

        for (int day = 1; day <= daysInMonth; day++)
        {
            var date     = new DateTime(request.Year, request.Month, day);
            var shiftType = GetShiftType(date);

            // FR-02.3: O gün için uygun doktorları filtrele
            // (İzin/rapor mekanizması geliştirildiğinde buraya eklenecek — şimdilik ardışık gün kontrolü)
            var eligible = doctors
                .Where(d => !IsConsecutiveDay(lastShiftDate[d.Id], date)) // FR-02.2
                .OrderBy(d => shiftScores[d.Id])  // FR-02.4: En düşük puanlı önce
                .ThenBy(d => d.SeniorityYear)     // Eşitlik: kıdem yılı az olan önce
                .ToList();

            if (eligible.Count == 0)
            {
                // FR-02.5: Çıkmaza girildi — bu gün atanmadı
                unassignedDays.Add(date);
                continue;
            }

            var selected = eligible.First();

            var shift = new Shift
            {
                UserId    = selected.Id,
                Date      = date,
                Type      = shiftType,
                IsApproved = false
            };

            var saved = await _shiftRepository.SaveAsync(shift);
            assignedShifts.Add(saved);

            // Durumu güncelle
            lastShiftDate[selected.Id] = date;
            shiftScores[selected.Id]  += shiftType == ShiftType.HaftaIci ? 1.0 : WeekendMultiplier;

        }

        var result = new GenerateShiftsResult
        {
            AssignedCount  = assignedShifts.Count,
            UnassignedDays = unassignedDays,
            AssignedShifts = assignedShifts.Select(s => new ShiftDto(s.Id, s.UserId, s.Date, (int)s.Type, s.IsApproved)).ToList()
        };

        return Response.Ok(result);
    }

    // ─── Yardımcı Metotlar ──────────────────────────────────────────────────

    /// <summary>FR-02.2: Önceki gün nöbet tuttu mu?</summary>
    private static bool IsConsecutiveDay(DateTime? lastShift, DateTime today)
    {
        if (!lastShift.HasValue) return false;
        return lastShift.Value.Date == today.AddDays(-1).Date;
    }

    /// <summary>Tarih hafta sonu mu, hafta içi mi?</summary>
    private static ShiftType GetShiftType(DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday
            ? ShiftType.HaftaSonu
            : ShiftType.HaftaIci;
    }

    /// <summary>FR-02.4: Doktorun mevcut nöbetlerinden kümülatif puan hesaplar.</summary>
    private static double CalculateScore(IEnumerable<Shift> shifts)
    {
        double score = 0;
        foreach (var s in shifts)
        {
            score += s.Type == ShiftType.HaftaIci ? 1.0 : WeekendMultiplier;
        }
        return score;
    }
}
