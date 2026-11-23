using System.ComponentModel.DataAnnotations;
using InterCase.Common.Helpers;

namespace InterCase.Common.Dtos.Orders
{
    public sealed record UpdateOrderRequest
    {
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
