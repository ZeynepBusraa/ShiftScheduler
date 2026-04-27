namespace ShiftScheduler.Application.DTOs;

public class ShiftRequestDto
{
    public int Id { get; set; }
    public int RequesterId { get; set; }
    public int TargetDoctorId { get; set; }
    public int ShiftId { get; set; }
    /// <summary>0=Bekliyor, 1=AsistanOnayladi, 2=BashekimOnayladi, 3=Reddedildi</summary>
    public int Status { get; set; }
}

