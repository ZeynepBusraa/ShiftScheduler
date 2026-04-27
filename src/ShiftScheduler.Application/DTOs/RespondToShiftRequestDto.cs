namespace ShiftScheduler.Application.DTOs;

/// <summary>Asistanın talebe cevap vermesi için istek.</summary>
public class RespondToShiftRequestDto
{
    /// <summary>true → kabul etti, false → reddetti.</summary>
    public bool Accept { get; set; }
}
