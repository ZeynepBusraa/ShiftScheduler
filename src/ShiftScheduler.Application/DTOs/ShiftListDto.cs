using System;
using System.Collections.Generic;

namespace ShiftScheduler.Application.DTOs;

/// <summary>Nöbet listesi özet bilgileri.</summary>
public class ShiftListDto
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int ListType { get; set; }   // 0 = Asistan, 1 = Uzman
    public int Status { get; set; }     // ApprovalStatus enum int değeri
    public int PreparedByUserId { get; set; }
    public string PreparedByUserName { get; set; } = string.Empty;
    public List<ShiftDto> Shifts { get; set; } = new();
}
