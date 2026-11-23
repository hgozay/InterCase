using InterCase.Common;
using InterCase.Repository.Entities;
using InterCase.Repository.Interfaces;
using InterCase.Service.Helpers;
using InterCase.Service.Services.DbServices;
using InterCase.UnitTest.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace InterCase.UnitTest.Services
{
    [TestClass]
    public sealed class UserServiceTests
    {
        private const string Password = "Demo123!";
        private static readonly Lazy<string> PasswordHash = new(() => PasswordHasher.Hash(Password));

        private readonly IRepositoryManager _repositories = Substitute.For<IRepositoryManager>();
        private readonly IUserRepository _users = Substitute.For<IUserRepository>();
        private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 11, 23, 9, 30, 0, TimeSpan.Zero));
        private readonly User _user;

        public UserServiceTests()
        {
            _repositories.User.Returns(_users);
            _user = new User
            {
                Id = 3,
                PublicId = Guid.CreateVersion7(),
                Username = "demo01",
                DisplayName = "Ayşe Yılmaz",
                PasswordHash = PasswordHash.Value,
                IsDemo = true
            };
        }

        private UserService CreateService() =>
            new(_repositories, OrderServiceContext.Mapper, _time, NullLogger<UserService>.Instance);

        [TestMethod]
        public async Task AuthenticateAsync_WithCorrectPassword_ReturnsTheUserByPublicId()
        {
            _users.GetByUsernameAsync("demo01", false, Arg.Any<CancellationToken>()).Returns(_user);

            var result = await CreateService().AuthenticateAsync("demo01", Password);

            Assert.IsNotNull(result);
            Assert.AreEqual(_user.PublicId, result.Id);
            Assert.AreEqual("demo01", result.Username);
            Assert.AreEqual("Ayşe Yılmaz", result.DisplayName);
        }

        [TestMethod]
        public async Task AuthenticateAsync_TrimsAndLowercasesTheUsername()
        {
            _users.GetByUsernameAsync("demo01", false, Arg.Any<CancellationToken>()).Returns(_user);

            var result = await CreateService().AuthenticateAsync("  DEMO01 ", Password);

            Assert.IsNotNull(result);
            await _users.Received(1).GetByUsernameAsync("demo01", false, Arg.Any<CancellationToken>());
        }

        [TestMethod]
        public async Task AuthenticateAsync_WithWrongPassword_ReturnsNull()
        {
            _users.GetByUsernameAsync("demo01", false, Arg.Any<CancellationToken>()).Returns(_user);

            var result = await CreateService().AuthenticateAsync("demo01", "wrong-password");

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task AuthenticateAsync_WhenUserDoesNotExist_ReturnsNull()
        {
            _users.GetByUsernameAsync(Arg.Any<string>(), false, Arg.Any<CancellationToken>()).Returns((User?)null);

            var result = await CreateService().AuthenticateAsync("nobody", Password);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task RecordLoginAsync_AddsLoginWithCurrentTimeAndSavesOnce()
        {
            _users.GetByPublicIdAsync(_user.PublicId, true, Arg.Any<CancellationToken>()).Returns(_user);
            var expiresAt = _time.GetUtcNow().UtcDateTime.AddMinutes(60);

            await CreateService().RecordLoginAsync(_user.PublicId, "jti-1", "token-value", expiresAt);

            var login = Assert.ContainsSingle(_user.Logins);
            Assert.AreEqual(_time.GetUtcNow().UtcDateTime, login.LoggedInAt);
            Assert.AreEqual("jti-1", login.TokenId);
            Assert.AreEqual("token-value", login.AccessToken);
            Assert.AreEqual(expiresAt, login.ExpiresAt);
            await _repositories.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [TestMethod]
        public async Task RecordLoginAsync_WhenUserDoesNotExist_ThrowsAndDoesNotSave()
        {
            _users.GetByPublicIdAsync(Arg.Any<Guid>(), true, Arg.Any<CancellationToken>()).Returns((User?)null);

            await Assert.ThrowsExactlyAsync<NotFoundException>(() =>
                CreateService().RecordLoginAsync(Guid.NewGuid(), "jti-1", "token-value", DateTime.UtcNow));

            await _repositories.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
        }

        [TestMethod]
        public async Task GetDemoUsersAsync_MapsUsersToResponses()
        {
            _users.GetDemoUsersAsync(Arg.Any<CancellationToken>()).Returns([_user]);

            var result = await CreateService().GetDemoUsersAsync();

            var demo = Assert.ContainsSingle(result);
            Assert.AreEqual(_user.PublicId, demo.Id);
            Assert.AreEqual(_user.Username, demo.Username);
        }
    }
}
