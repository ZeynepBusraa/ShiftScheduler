namespace ShiftScheduler.Application.DTOs.Auth;

// Eski haline yeni kuralları ve UI için gereken bilgileri ekledik
public record LoginResponse(
    string Token, 
    int UserId, 
    string FullName,
    string Role, 
    bool IsSenior, 
    int? DepartmentId, 
    string? DepartmentName, 
    int RemainingChangeRequests
);