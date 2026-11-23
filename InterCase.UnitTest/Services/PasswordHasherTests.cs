using InterCase.Service.Helpers;

namespace InterCase.UnitTest.Services
{
    [TestClass]
    public sealed class PasswordHasherTests
    {
        private const string Password = "Demo123!";
        private static readonly Lazy<string> StoredHash = new(() => PasswordHasher.Hash(Password));

        [TestMethod]
        public void Hash_UsesPbkdf2FormatWithSaltAndIterations()
        {
            var parts = StoredHash.Value.Split('$');

            Assert.HasCount(4, parts);
            Assert.AreEqual("PBKDF2-SHA256", parts[0]);
            Assert.AreEqual("100000", parts[1]);
            Assert.HasCount(16, Convert.FromBase64String(parts[2]));
            Assert.HasCount(32, Convert.FromBase64String(parts[3]));
        }

        [TestMethod]
        public void Hash_SamePasswordTwice_ProducesDifferentHashes()
        {
            Assert.AreNotEqual(StoredHash.Value, PasswordHasher.Hash(Password));
        }

        [TestMethod]
        public void Verify_WithTheSamePassword_ReturnsTrue()
        {
            Assert.IsTrue(PasswordHasher.Verify(Password, StoredHash.Value));
        }

        [TestMethod]
        [DataRow("demo123!")]
        [DataRow("Demo123")]
        [DataRow("")]
        public void Verify_WithADifferentPassword_ReturnsFalse(string password)
        {
            Assert.IsFalse(PasswordHasher.Verify(password, StoredHash.Value));
        }

        [TestMethod]
        public void Verify_WhenIterationCountIsChanged_ReturnsFalse()
        {
            var parts = StoredHash.Value.Split('$');
            parts[1] = "99999";

            Assert.IsFalse(PasswordHasher.Verify(Password, string.Join('$', parts)));
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("plain-text-password")]
        [DataRow("MD5$100000$c2FsdA==$a2V5")]
        [DataRow("PBKDF2-SHA256$abc$c2FsdA==$a2V5")]
        [DataRow("PBKDF2-SHA256$0$c2FsdA==$a2V5")]
        [DataRow("PBKDF2-SHA256$-5$c2FsdA==$a2V5")]
        [DataRow("PBKDF2-SHA256$1000$not base64!$a2V5")]
        [DataRow("PBKDF2-SHA256$1000$c2FsdA==$not base64!")]
        public void Verify_WhenStoredHashIsMalformed_ReturnsFalse(string storedHash)
        {
            Assert.IsFalse(PasswordHasher.Verify(Password, storedHash));
        }
    }
}
