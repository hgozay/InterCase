using InterCase.Common.Dtos.Users;

namespace InterCase.Service.Interfaces
{
    public interface IUserService
    {
        Task<UserResponse?> AuthenticateAsync(string username, string password, CancellationToken ct = default);
        Task RecordLoginAsync(Guid userId, string tokenId, string accessToken, DateTime expiresAt, CancellationToken ct = default);
        Task<IReadOnlyList<UserResponse>> GetDemoUsersAsync(CancellationToken ct = default);
    }
}
