using System;
using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Domain.Entities;

public class Shift
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public ShiftType Type { get; set; }
    public bool IsApproved { get; set; }

    public User? User { get; set; }
}
