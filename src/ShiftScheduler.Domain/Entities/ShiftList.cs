using System.Collections.Generic;
using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Domain.Entities;

public class ShiftList
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    
    /// <summary>Bu listenin asistan mı yoksa uzman nöbet listesi mi olduğu.</summary>
    public ShiftListType ListType { get; set; }
    
    /// <summary>Hangi bölüme ait olduğu.</summary>
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }
    
    public ApprovalStatus Status { get; set; } 
    
    public int PreparedByUserId { get; set; }
    public User? PreparedByUser { get; set; }

    public ICollection<Shift> Shifts { get; set; } = new List<Shift>();
}