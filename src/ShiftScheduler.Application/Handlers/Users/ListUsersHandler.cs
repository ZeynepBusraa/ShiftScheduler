using System.Collections.Generic;
using System.Threading.Tasks;
using ShiftScheduler.Application.Common;
using ShiftScheduler.Application.DTOs;
using ShiftScheduler.Application.Mappers;
using ShiftScheduler.Application.Repositories;

namespace ShiftScheduler.Application.Handlers.Users;

public class ListUsersHandler(IUserRepository repository, UserDtoMapper mapper)
{
    private readonly IUserRepository _repository = repository;
    private readonly UserDtoMapper _mapper = mapper;

    public async Task<ResponsePayload<List<UserDto>>> HandleAsync()
    {
        var users = await _repository.ListAllAsync();
        return Response.Ok(_mapper.MapList(users));
    }
}
