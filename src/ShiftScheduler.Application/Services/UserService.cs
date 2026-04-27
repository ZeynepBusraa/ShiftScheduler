using System.Collections.Generic;
using System.Threading.Tasks;
using ShiftScheduler.Application.Common;
using ShiftScheduler.Application.DTOs;
using ShiftScheduler.Application.Handlers.Users;

namespace ShiftScheduler.Application.Services;

public class UserService(
    CreateUserHandler createHandler,
    ListUsersHandler listHandler,
    DeleteUserHandler deleteHandler) : IUserService
{
    private readonly CreateUserHandler _createHandler = createHandler;
    private readonly ListUsersHandler _listHandler = listHandler;
    private readonly DeleteUserHandler _deleteHandler = deleteHandler;

    public Task<ResponsePayload<UserDto>> CreateAsync(CreateUserRequest request)
    {
        return _createHandler.HandleAsync(request);
    }

    public Task<ResponsePayload<List<UserDto>>> ListAsync()
    {
        return _listHandler.HandleAsync();
    }

    public Task<ResponsePayload<bool>> DeleteAsync(int id)
    {
        return _deleteHandler.HandleAsync(id);
    }
}
