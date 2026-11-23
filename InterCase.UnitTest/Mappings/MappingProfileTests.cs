using InterCase.UnitTest.Helpers;

namespace InterCase.UnitTest.Mappings
{
    [TestClass]
    public sealed class MappingProfileTests
    {
        [TestMethod]
        public void MappingProfile_ConfigurationIsValid()
        {
            OrderServiceContext.Mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}
