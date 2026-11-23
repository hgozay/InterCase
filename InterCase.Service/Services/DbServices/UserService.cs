using AutoMapper;
using InterCase.Common;
using InterCase.Common.Dtos.Users;
using InterCase.Repository.Entities;
using InterCase.Repository.Interfaces;
using InterCase.Service.Helpers;
using InterCase.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace InterCase.Service.Services.DbServices
{
    public sealed class UserService(
        IRepositoryManager repositoryManager,
        IMapper mapper,
        TimeProvider timeProvider,
        ILogger<UserService> logger) : IUserService
    {
        private static readonly string UnknownUserHash = PasswordHasher.Hash(Guid.NewGuid().ToString("N"));

        public async Task<UserResponse?> AuthenticateAsync(string username, string password, CancellationToken ct = default)
        {
            var normalized = username.Trim().ToLowerInvariant();
            var user = await repositoryManager.User.GetByUsernameAsync(normalized, trackChanges: false, ct);
            var passwordMatches = PasswordHasher.Verify(password, user?.PasswordHash ?? UnknownUserHash);

            if (user is null || !passwordMatches)
            {
                logger.LogWarning("Sign-in rejected for {Username}", normalized);
                return null;
            }

            return mapper.Map<UserResponse>(user);
        }

        public async Task RecordLoginAsync(Guid userId, string tokenId, string accessToken, DateTime expiresAt, CancellationToken ct = default)
        {
            var user = await repositoryManager.User.GetByPublicIdAsync(userId, trackChanges: true, ct)
                ?? throw new NotFoundException($"User {userId} was not found.");

            user.Logins.Add(new UserLogin
            {
                LoggedInAt = timeProvider.GetUtcNow().UtcDateTime,
                TokenId = tokenId,
                AccessToken = accessToken,
                ExpiresAt = expiresAt
            });
            await repositoryManager.SaveChangesAsync(ct);

            logger.LogInformation("User {UserId} signed in; token {TokenId} expires at {ExpiresAt}", userId, tokenId, expiresAt);
        }

        public async Task<IReadOnlyList<UserResponse>> GetDemoUsersAsync(CancellationToken ct = default)
        {
            var users = await repositoryManager.User.GetDemoUsersAsync(ct);
            return mapper.Map<List<UserResponse>>(users);
        }
    }
}
