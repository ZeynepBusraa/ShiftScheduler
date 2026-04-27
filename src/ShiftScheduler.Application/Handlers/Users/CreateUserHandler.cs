using System.Threading.Tasks;
using ShiftScheduler.Application.Common;
using ShiftScheduler.Application.DTOs;
using ShiftScheduler.Application.Mappers;
using ShiftScheduler.Application.Repositories;

namespace ShiftScheduler.Application.Handlers.Users;

public class CreateUserHandler(IUserRepository repository, UserDtoMapper mapper)
{
    private readonly IUserRepository _repository = repository;
    private readonly UserDtoMapper _mapper = mapper;

    public async Task<ResponsePayload<UserDto>> HandleAsync(CreateUserRequest request)
    {
        var existingUser = await _repository.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return new ResponsePayload<UserDto>
            {
                Success = false,
                Code = "EMAIL_IN_USE",
                Message = "Bu e-posta adresi zaten kullanımda."
            };
        }

        var entity = new ShiftScheduler.Domain.Entities.User
        {
            Email = request.Email,
            PasswordHash = request.Password, // TODO: Hashlenmesi gerekiyor
            Role = request.Role,
            SeniorityYear = request.SeniorityYear,
            DepartmentId = request.DepartmentId
        };

        var savedUser = await _repository.SaveAsync(entity);

        return Response.SaveSuccess(_mapper.Map(savedUser));
    }
}
