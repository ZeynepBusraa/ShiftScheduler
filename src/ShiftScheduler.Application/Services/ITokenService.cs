using ShiftScheduler.Domain.Entities;

namespace ShiftScheduler.Application.Services;

public interface ITokenService
{
    string GenerateToken(User user);
}
