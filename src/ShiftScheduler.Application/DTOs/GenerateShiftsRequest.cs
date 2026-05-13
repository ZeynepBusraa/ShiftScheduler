using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Application.DTOs;

/// <summary>Aylık nöbet atama algoritmasını tetiklemek için istek.</summary>
public class GenerateShiftsRequest
{
    public int Year { get; set; }
    public int Month { get; set; }
    
    /// <summary>Hangi departman için nöbet oluşturulsun?</summary>
    public int DepartmentId { get; set; }

    /// <summary>
    /// Asistan mı yoksa Uzman nöbet listesi mi oluşturulsun?
    /// Asistan = 0, Uzman = 1
    /// </summary>
    public ShiftListType ListType { get; set; }
}
