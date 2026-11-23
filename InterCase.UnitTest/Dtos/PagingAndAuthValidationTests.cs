using InterCase.Common.Dtos.Auth;
using InterCase.Common.Dtos.Languages;
using InterCase.Common.Dtos.Paging;
using InterCase.Common.Helpers;
using InterCase.UnitTest.Helpers;

namespace InterCase.UnitTest.Dtos
{
    [TestClass]
    public sealed class PagingAndAuthValidationTests
    {
        [TestMethod]
        public void PageRequest_Defaults_AreFirstPageOfTen()
        {
            var request = new PageRequest();

            Assert.AreEqual(1, request.Page);
            Assert.AreEqual(PageRequest.DefaultPageSize, request.PageSize);
            Assert.IsEmpty(ModelValidator.Validate(request));
        }

        [TestMethod]
        public void PageRequest_WhenPageSizeIsTheMaximum_HasNoErrors()
        {
            var errors = ModelValidator.Validate(new PageRequest { Page = 3, PageSize = PageRequest.MaxPageSize });

            Assert.IsEmpty(errors);
        }

        [TestMethod]
        [DataRow(0, 10, 0)]
        [DataRow(1, 10, 1)]
        [DataRow(10, 10, 1)]
        [DataRow(11, 10, 2)]
        [DataRow(21, 10, 3)]
        [DataRow(13, 5, 3)]
        public void PagedResponse_TotalPages_RoundsUp(int totalCount, int pageSize, int expectedPages)
        {
            var response = new PagedResponse<string>([], 1, pageSize, totalCount);

            Assert.AreEqual(expectedPages, response.TotalPages);
        }

        [TestMethod]
        public void TokenRequest_WhenEmpty_ReturnsRequiredKeys()
        {
            var errors = ModelValidator.Validate(new TokenRequest());

            CollectionAssert.AreEquivalent(
                new[] { Localization.Keys.UsernameRequired, Localization.Keys.PasswordRequired },
                errors.ToArray());
        }

        [TestMethod]
        [DataRow("en")]
        [DataRow("tr")]
        public void SetLanguageRequest_WhenLanguageIsSupported_HasNoErrors(string language)
        {
            var errors = ModelValidator.Validate(new SetLanguageRequest { Language = language });

            Assert.IsEmpty(errors);
        }

        [TestMethod]
        [DataRow("de")]
        [DataRow("EN")]
        [DataRow("en-US")]
        public void SetLanguageRequest_WhenLanguageIsNotSupported_ReturnsNotSupportedKey(string language)
        {
            var errors = ModelValidator.Validate(new SetLanguageRequest { Language = language });

            Assert.AreEqual(Localization.Keys.LanguageNotSupported, Assert.ContainsSingle(errors));
        }

        [TestMethod]
        public void SetLanguageRequest_WhenLanguageIsEmpty_ReturnsRequiredKey()
        {
            var errors = ModelValidator.Validate(new SetLanguageRequest { Language = string.Empty });

            Assert.AreEqual(Localization.Keys.LanguageRequired, Assert.ContainsSingle(errors));
        }

        [TestMethod]
        public void NotEmptyGuid_TreatsOnlyTheEmptyGuidAsInvalid()
        {
            var attribute = new NotEmptyGuidAttribute();

            Assert.IsFalse(attribute.IsValid(Guid.Empty));
            Assert.IsTrue(attribute.IsValid(Guid.NewGuid()));
            Assert.IsTrue(attribute.IsValid(null));
        }
    }
}
