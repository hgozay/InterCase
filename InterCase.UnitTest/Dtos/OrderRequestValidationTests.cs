using InterCase.Common;
using InterCase.Common.Dtos.Orders;
using InterCase.Common.Dtos.Paging;
using InterCase.Common.Helpers;
using InterCase.UnitTest.Helpers;

namespace InterCase.UnitTest.Dtos
{
    [TestClass]
    public sealed class OrderRequestValidationTests
    {
        private static CreateOrderRequest ValidRequest() => new()
        {
            UserId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            Quantity = 2,
            PaymentMethod = PaymentMethod.CreditCard
        };

        [TestMethod]
        public void CreateOrderRequest_WhenAllFieldsAreValid_HasNoErrors()
        {
            var errors = ModelValidator.Validate(ValidRequest());

            Assert.IsEmpty(errors);
        }

        [TestMethod]
        public void CreateOrderRequest_WhenEmpty_ReturnsRequiredKeyForEachField()
        {
            var errors = ModelValidator.Validate(new CreateOrderRequest());

            CollectionAssert.AreEquivalent(
                new[]
                {
                    Localization.Keys.UserIdRequired,
                    Localization.Keys.ProductIdRequired,
                    Localization.Keys.QuantityRequired,
                    Localization.Keys.PaymentMethodRequired
                },
                errors.ToArray());
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1001)]
        public void CreateOrderRequest_WhenQuantityIsOutOfRange_ReturnsQuantityRangeKey(int quantity)
        {
            var errors = ModelValidator.Validate(ValidRequest() with { Quantity = quantity });

            Assert.AreEqual(Localization.Keys.QuantityRange, Assert.ContainsSingle(errors));
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(1000)]
        public void CreateOrderRequest_WhenQuantityIsOnTheBoundary_HasNoErrors(int quantity)
        {
            var errors = ModelValidator.Validate(ValidRequest() with { Quantity = quantity });

            Assert.IsEmpty(errors);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(99)]
        public void CreateOrderRequest_WhenPaymentMethodIsUndefined_ReturnsPaymentMethodInvalidKey(int paymentMethod)
        {
            var errors = ModelValidator.Validate(ValidRequest() with { PaymentMethod = (PaymentMethod)paymentMethod });

            Assert.AreEqual(Localization.Keys.PaymentMethodInvalid, Assert.ContainsSingle(errors));
        }

        [TestMethod]
        public void CreateOrderRequest_WhenIdsAreEmptyGuids_ReturnsEmptyGuidKeys()
        {
            var errors = ModelValidator.Validate(ValidRequest() with { UserId = Guid.Empty, ProductId = Guid.Empty });

            CollectionAssert.AreEquivalent(
                new[] { Localization.Keys.UserIdEmpty, Localization.Keys.ProductIdEmpty },
                errors.ToArray());
        }

        [TestMethod]
        public void UpdateOrderRequest_WhenEmpty_ReturnsRequiredKeyForEachField()
        {
            var errors = ModelValidator.Validate(new UpdateOrderRequest());

            CollectionAssert.AreEquivalent(
                new[]
                {
                    Localization.Keys.ProductIdRequired,
                    Localization.Keys.QuantityRequired,
                    Localization.Keys.PaymentMethodRequired
                },
                errors.ToArray());
        }

        [TestMethod]
        [DataRow(0, 10, "PageInvalid")]
        [DataRow(1, 0, "PageSizeInvalid")]
        [DataRow(1, PageRequest.MaxPageSize + 1, "PageSizeInvalid")]
        public void PageRequest_WhenOutOfRange_ReturnsMatchingKey(int page, int pageSize, string expectedKey)
        {
            var errors = ModelValidator.Validate(new PageRequest { Page = page, PageSize = pageSize });

            Assert.AreEqual(expectedKey, Assert.ContainsSingle(errors));
        }
    }
}
