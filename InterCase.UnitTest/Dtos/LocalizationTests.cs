using InterCase.Common.Helpers;

namespace InterCase.UnitTest.Dtos
{
    [TestClass]
    public sealed class LocalizationTests
    {
        [TestMethod]
        public void Get_WhenKeyExists_ReturnsMessageInRequestedLanguage()
        {
            Assert.AreEqual("Username or password is incorrect.", Localization.Get(Localization.Keys.InvalidCredentials, Localization.English));
            Assert.AreEqual("Kullanıcı adı ya da şifre hatalı.", Localization.Get(Localization.Keys.InvalidCredentials, Localization.Turkish));
        }

        [TestMethod]
        public void Get_WhenKeyIsUnknown_ReturnsTheKeyItself()
        {
            Assert.AreEqual("NoSuchKey", Localization.Get("NoSuchKey", Localization.Turkish));
        }

        [TestMethod]
        [DataRow("de")]
        [DataRow("")]
        public void Get_WhenLanguageIsNotSupported_FallsBackToEnglish(string language)
        {
            Assert.AreEqual(
                Localization.Get(Localization.Keys.QuantityRange, Localization.English),
                Localization.Get(Localization.Keys.QuantityRange, language));
        }

        [TestMethod]
        [DataRow("TR", "tr")]
        [DataRow("en", "en")]
        [DataRow("fr", "en")]
        [DataRow(null, "en")]
        public void Normalize_ReturnsSupportedLowercaseLanguageOrDefault(string? language, string expected)
        {
            Assert.AreEqual(expected, Localization.Normalize(language));
        }

        [TestMethod]
        public void Keys_EveryKeyHasMessages()
        {
            var missing = typeof(Localization.Keys)
                .GetFields()
                .Where(f => f.IsLiteral)
                .Select(f => (string)f.GetRawConstantValue()!)
                .Where(key => !Localization.HasKey(key))
                .ToList();

            Assert.IsEmpty(missing);
        }
    }
}
