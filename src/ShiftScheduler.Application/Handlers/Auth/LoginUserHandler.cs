using System;
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

    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public async Task<ResponsePayload<LoginResponse>> HandleAsync(LoginRequest request)
    {
        var user = await _userRepository.FindByEmailAsync(request.Email);

        // Kullanıcı bulunamadıysa genel hata döndür (timing attack önlemi)
        if (user == null)
        {
            return new ResponsePayload<LoginResponse>
            {
                Success = false,
                Code = "UNAUTHORIZED",
                Message = "Geçersiz e-posta veya şifre."
            };
        }

        // FR-01.2: Hesap kilitli mi?
        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
        {
            var remainingMinutes = (int)Math.Ceiling((user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
            return new ResponsePayload<LoginResponse>
            {
                Success = false,
                Code = "ACCOUNT_LOCKED",
                Message = $"Hesabınız kilitlendi. {remainingMinutes} dakika sonra tekrar deneyin."
            };
        }

        // Şifre kontrolü
        if (user.PasswordHash != request.Password)
        {
            // FR-01.2: Başarısız deneme sayacını artır
            user.FailedLoginCount++;

            if (user.FailedLoginCount >= MaxFailedAttempts)
            {
                user.LockoutEnd = DateTime.UtcNow.Add(LockoutDuration);
                user.FailedLoginCount = 0; // Kilitleme sonrası sayacı sıfırla
                await _userRepository.SaveAsync(user);

                return new ResponsePayload<LoginResponse>
                {
                    Success = false,
                    Code = "ACCOUNT_LOCKED",
                    Message = $"Çok fazla hatalı giriş. Hesabınız {LockoutDuration.TotalMinutes} dakika kilitlendi."
                };
            }

            await _userRepository.SaveAsync(user);

            int remaining = MaxFailedAttempts - user.FailedLoginCount;
            return new ResponsePayload<LoginResponse>
            {
                Success = false,
                Code = "UNAUTHORIZED",
                Message = $"Geçersiz e-posta veya şifre. {remaining} hakkınız kaldı."
            };
        }

        // Başarılı giriş — sayacı sıfırla
        user.FailedLoginCount = 0;
        user.LockoutEnd = null;
        await _userRepository.SaveAsync(user);

        var token = _tokenService.GenerateToken(user);
        var response = new LoginResponse(token, user.Id, user.Role.ToString(), user.DepartmentId);

        return Response.Ok(response);
    }
}

