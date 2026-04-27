using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Application.DTOs;

public record ShiftRequestDto(
    int Id,
    int RequesterId,
    int TargetDoctorId,
    int ShiftId,
    RequestStatus Status
);
