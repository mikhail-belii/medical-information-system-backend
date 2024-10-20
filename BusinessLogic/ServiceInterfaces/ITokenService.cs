using Common.DbModels;

namespace BusinessLogic.ServiceInterfaces;

public interface ITokenService
{
    public Task AddToken(Guid userId, string token);
    public Task RemoveToken(string? token);
    public Task<Guid> GetUserIdByToken(string token);
}