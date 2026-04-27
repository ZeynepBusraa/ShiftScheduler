using System.Threading.Tasks;
using ShiftScheduler.Application.Common;
using ShiftScheduler.Application.DTOs.Auth;
using ShiftScheduler.Application.Repositories;
using ShiftScheduler.Application.Services;

namespace ShiftScheduler.Application.Handlers.Auth;

public class LoginUserHandler(IUserRepository userRepository, ITokenService tokenService)
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ITokenService _tokenService = tokenService;

    public async Task<ResponsePayload<LoginResponse>> HandleAsync(LoginRequest request)
    {
        var user = await _userRepository.FindByEmailAsync(request.Email);

        if (user == null || user.PasswordHash != request.Password)
        {
            return new ResponsePayload<LoginResponse>
            {
                Success = false,
                Code = "UNAUTHORIZED",
                Message = "Geçersiz e-posta veya şifre"
            };
        }

        var token = _tokenService.GenerateToken(user);
        var response = new LoginResponse(token, user.Id, user.Role.ToString(), user.DepartmentId);

        return Response.Ok(response);
    }
}
