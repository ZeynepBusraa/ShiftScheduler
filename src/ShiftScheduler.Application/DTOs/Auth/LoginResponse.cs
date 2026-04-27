namespace ShiftScheduler.Application.DTOs.Auth;

public record LoginResponse(string Token, int UserId, string Role, int? DepartmentId);
