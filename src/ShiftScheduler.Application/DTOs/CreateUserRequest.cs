using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Application.DTOs;

public record CreateUserRequest(
    string Email,
    string Password,
    Role Role,
    int SeniorityYear,
    int? DepartmentId
);
