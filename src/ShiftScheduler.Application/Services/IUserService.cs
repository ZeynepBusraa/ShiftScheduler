using System.Collections.Generic;
using System.Threading.Tasks;
using ShiftScheduler.Application.Common;
using ShiftScheduler.Application.DTOs;

namespace ShiftScheduler.Application.Services;

public interface IUserService
{
    Task<ResponsePayload<UserDto>> CreateAsync(CreateUserRequest request);
    Task<ResponsePayload<List<UserDto>>> ListAsync();
    Task<ResponsePayload<bool>> DeleteAsync(int id);
}
