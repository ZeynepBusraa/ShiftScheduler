using System;
using System.Collections.Generic;

namespace ShiftScheduler.Application.DTOs;

/// <summary>Algoritma sonucu: kaç nöbet atandı, hangi günler boş kaldı.</summary>
public class GenerateShiftsResult
{
    /// <summary>Başarıyla atanan nöbet sayısı.</summary>
    public int AssignedCount { get; set; }

    /// <summary>
    /// FR-02.5: Algoritmanın atayamadığı günler.
    /// Başhekim bu günlere manuel atama yapabilir.
    /// </summary>
    public List<DateTime> UnassignedDays { get; set; } = new();

    /// <summary>Oluşturulan tüm nöbetlerin listesi.</summary>
    public List<ShiftDto> AssignedShifts { get; set; } = new();

    /// <summary>Oluşturulan nöbet listesinin ID'si.</summary>
    public int ShiftListId { get; set; }
}
