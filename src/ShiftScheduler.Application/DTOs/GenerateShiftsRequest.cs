namespace ShiftScheduler.Application.DTOs;

/// <summary>Aylık nöbet atama algoritmasını tetiklemek için istek.</summary>
public class GenerateShiftsRequest
{
    public int Year { get; set; }
    public int Month { get; set; }
    
    /// <summary>Hangi departman için nöbet oluşturulsun? null ise tüm departmanlar.</summary>
    public int? DepartmentId { get; set; }
}
