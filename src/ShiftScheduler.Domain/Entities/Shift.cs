using System;
using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Domain.Entities;

public class Shift
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ShiftListId { get; set; } 
    public DateTime Date { get; set; }
    public ShiftType Type { get; set; }

    public User? User { get; set; }
    public ShiftList? ShiftList { get; set; }
}