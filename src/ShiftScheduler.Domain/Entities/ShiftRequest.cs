using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Domain.Entities;

public class ShiftRequest
{
    public int Id { get; set; }
    public int RequesterId { get; set; }
    public int TargetDoctorId { get; set; }
    public int ShiftId { get; set; }
    public RequestStatus Status { get; set; }
}
