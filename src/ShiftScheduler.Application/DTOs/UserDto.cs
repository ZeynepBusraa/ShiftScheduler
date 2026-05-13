using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Application.DTOs;

public record UserDto(
    int Id,
    string Email,
    Role Role,
    int SeniorityYear,
    int? DepartmentId
);