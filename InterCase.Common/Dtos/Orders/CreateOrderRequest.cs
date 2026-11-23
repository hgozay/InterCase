using System.ComponentModel.DataAnnotations;
using InterCase.Common.Helpers;

namespace InterCase.Common.Dtos.Orders
{
    public sealed record CreateOrderRequest
    {
        [Required(ErrorMessage = Localization.Keys.UserIdRequired)]
        [NotEmptyGuid(ErrorMessage = Localization.Keys.UserIdEmpty)]
        public Guid? UserId { get; init; }

        [Required(ErrorMessage = Localization.Keys.ProductIdRequired)]
        [NotEmptyGuid(ErrorMessage = Localization.Keys.ProductIdEmpty)]
        public Guid? ProductId { get; init; }

        [Required(ErrorMessage = Localization.Keys.QuantityRequired)]
        [Range(1, 1000, ErrorMessage = Localization.Keys.QuantityRange)]
        public int? Quantity { get; init; }

        [Required(ErrorMessage = Localization.Keys.PaymentMethodRequired)]
        [EnumDataType(typeof(PaymentMethod), ErrorMessage = Localization.Keys.PaymentMethodInvalid)]
        public PaymentMethod? PaymentMethod { get; init; }
    }
}
