using AutoMapper;
using InterCase.Common;
using InterCase.Common.Dtos.Orders;
using InterCase.Repository.Entities;

namespace InterCase.Service.Mappings
{
    public sealed class MappingProfile : Profile
    {
        public const string CorrelationIdKey = nameof(OrderPlacedMessage.CorrelationId);

        public MappingProfile()
        {
            CreateMap<CreateOrderRequest, Order>(MemberList.Source)
                .ForMember(d => d.UserId, o => o.Condition((_, destination) => destination.Id == 0));

            CreateMap<Order, OrderResponse>()
                .ForCtorParam(nameof(OrderResponse.Id), o => o.MapFrom(s => s.PublicId));

            CreateMap<Order, OrderPlacedMessage>()
                .ForCtorParam(nameof(OrderPlacedMessage.OrderId), o => o.MapFrom(s => s.Id))
                .ForCtorParam(nameof(OrderPlacedMessage.CorrelationId), o => o.MapFrom((_, context) => (string)context.Items[CorrelationIdKey]));
        }
    }
}
