using System.ComponentModel.DataAnnotations;
using InterCase.Common.Helpers;

namespace InterCase.Common.Dtos.Orders
{
    public sealed record CreateOrderRequest
    {
        [Required(ErrorMessage = Localization.Keys.UserIdRequired)]
        [MaxLength(64, ErrorMessage = Localization.Keys.UserIdMaxLength)]
        public string UserId { get; init; } = string.Empty;

        [Required(ErrorMessage = Localization.Keys.ProductIdRequired)]
        [MaxLength(64, ErrorMessage = Localization.Keys.ProductIdMaxLength)]
        public string ProductId { get; init; } = string.Empty;

        [Required(ErrorMessage = Localization.Keys.QuantityRequired)]
        [Range(1, 1000, ErrorMessage = Localization.Keys.QuantityRange)]
        public int? Quantity { get; init; }

        [Required(ErrorMessage = Localization.Keys.PaymentMethodRequired)]
        [EnumDataType(typeof(PaymentMethod), ErrorMessage = Localization.Keys.PaymentMethodInvalid)]
        public PaymentMethod? PaymentMethod { get; init; }
    }
}
