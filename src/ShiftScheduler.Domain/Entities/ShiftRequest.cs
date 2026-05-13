using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Domain.Entities;

public class ShiftRequest
{
    public int Id { get; set; }
    public int RequesterId { get; set; }
    public int TargetDoctorId { get; set; }
    public int ShiftId { get; set; }
    public RequestStatus Status { get; set; }

    // Navigation properties
    public User? Requester { get; set; }
    public User? TargetDoctor { get; set; }
    public Shift? Shift { get; set; }
}
