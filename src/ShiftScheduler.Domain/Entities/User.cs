using System.Collections.Generic;
using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Role Role { get; set; }
    public int SeniorityYear { get; set; }
    
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public ICollection<Shift> Shifts { get; set; } = new List<Shift>();
}
