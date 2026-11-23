using System.Security.Claims;
using InterCase.API.Controllers;
using InterCase.API.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace InterCase.UnitTest.Api
{
    [TestClass]
    public sealed class OwnershipTests
    {
        public sealed record Resource(Guid OwnerId);

        public sealed class ProbeController : BaseController
        {
            public ActionResult? CheckOwner(Guid userId) => DenyIfNotOwner(userId);

            public ActionResult? CheckResource(Resource? resource) => DenyIfNotFoundOrNotOwner(resource, r => r.OwnerId);
        }

        private static ClaimsPrincipal PrincipalWithSubject(string? subject) =>
            new(new ClaimsIdentity(
                subject is null ? [] : [new Claim(JwtRegisteredClaimNames.Sub, subject)],
                "Test"));

        private static ProbeController ControllerFor(Guid userId) => new()
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = PrincipalWithSubject(userId.ToString()) }
            }
        };

        [TestMethod]
        public void GetUserId_ReadsTheSubjectClaim()
        {
            var userId = Guid.NewGuid();

            Assert.AreEqual(userId, PrincipalWithSubject(userId.ToString()).GetUserId());
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("demo01")]
        public void GetUserId_WhenSubjectIsMissingOrNotAGuid_ReturnsNull(string? subject)
        {
            Assert.IsNull(PrincipalWithSubject(subject).GetUserId());
        }

        [TestMethod]
        public void Owns_IsTrueOnlyForTheSignedInUser()
        {
            var userId = Guid.NewGuid();
            var principal = PrincipalWithSubject(userId.ToString());

            Assert.IsTrue(principal.Owns(userId));
            Assert.IsFalse(principal.Owns(Guid.NewGuid()));
            Assert.IsFalse(PrincipalWithSubject(null).Owns(Guid.Empty));
        }

        [TestMethod]
        public void DenyIfNotOwner_ForTheSignedInUser_AllowsTheRequest()
        {
            var userId = Guid.NewGuid();

            Assert.IsNull(ControllerFor(userId).CheckOwner(userId));
        }

        [TestMethod]
        public void DenyIfNotOwner_ForAnotherUser_ReturnsForbid()
        {
            var result = ControllerFor(Guid.NewGuid()).CheckOwner(Guid.NewGuid());

            Assert.IsInstanceOfType<ForbidResult>(result);
        }

        [TestMethod]
        public void DenyIfNotFoundOrNotOwner_WhenResourceIsMissing_ReturnsNotFound()
        {
            var result = ControllerFor(Guid.NewGuid()).CheckResource(null);

            Assert.IsInstanceOfType<NotFoundResult>(result);
        }

        [TestMethod]
        public void DenyIfNotFoundOrNotOwner_WhenResourceBelongsToAnotherUser_ReturnsForbid()
        {
            var result = ControllerFor(Guid.NewGuid()).CheckResource(new Resource(Guid.NewGuid()));

            Assert.IsInstanceOfType<ForbidResult>(result);
        }

        [TestMethod]
        public void DenyIfNotFoundOrNotOwner_WhenResourceBelongsToTheUser_AllowsTheRequest()
        {
            var userId = Guid.NewGuid();

            Assert.IsNull(ControllerFor(userId).CheckResource(new Resource(userId)));
        }
    }
}
