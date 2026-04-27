namespace ShiftScheduler.Application.DTOs;

/// <summary>Nöbet değişim talebi oluşturmak için gelen istek.</summary>
public class CreateShiftRequestDto
{
    /// <summary>Takas istenen nöbetin ID'si (kendi nöbeti).</summary>
    public int ShiftId { get; set; }

    /// <summary>Takas yapılmak istenen doktorun ID'si.</summary>
    public int TargetDoctorId { get; set; }
}
