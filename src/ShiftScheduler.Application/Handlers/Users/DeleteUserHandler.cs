using System.Threading.Tasks;
using ShiftScheduler.Application.Common;
using ShiftScheduler.Application.Repositories;

namespace ShiftScheduler.Application.Handlers.Users;

public class DeleteUserHandler(IUserRepository repository)
{
    private readonly IUserRepository _repository = repository;

    public async Task<ResponsePayload<bool>> HandleAsync(int id)
    {
        var user = await _repository.FindByIdAsync(id);
        if (user == null)
        {
            return new ResponsePayload<bool>
            {
                Success = false,
                Code = "NOT_FOUND",
                Message = "Kullanıcı bulunamadı."
            };
        }

        await _repository.DeleteAsync(user);

        return Response.Ok(true);
    }
}
