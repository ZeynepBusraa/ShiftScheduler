using ShiftScheduler.Domain.Enums;

namespace ShiftScheduler.Application.DTOs; // Eğer senin dosyanın namespace'i farklıysa orayı değiştirmene gerek yok

public record CreateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    Role Role,
    bool IsSenior,
    int SeniorityYear,
    int DepartmentId
);