using System.Collections.Concurrent;
using BusinessLogic.ServiceInterfaces;

namespace BusinessLogic.Services;

public class TokenService : ITokenService
{
    private readonly ConcurrentDictionary<Guid, string>? _tokens = new ConcurrentDictionary<Guid, string>();
    
    public async Task AddToken(Guid userId, string token)
    {
        _tokens[userId] = token;
        
        await Task.CompletedTask;
    }

    public async Task RemoveToken(string token)
    {
        var userId = _tokens
            .FirstOrDefault(x => x.Value == token).Key;
        if (userId != Guid.Empty)
        {
            _tokens.TryRemove(userId, out _);
        }

        await Task.CompletedTask;
    }

    public async Task<Guid> GetUserIdByToken(string token)
    {
        var userId = _tokens
            .FirstOrDefault(x => x.Value == token).Key;
        await Task.CompletedTask;
        return userId;
    }

    public async Task ClearTokens()
    {
        _tokens.Clear();
        await Task.CompletedTask;
    }
}