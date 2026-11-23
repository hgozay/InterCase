using AutoMapper;
using InterCase.Common;
using InterCase.Common.Dtos.Orders;
using InterCase.Common.Dtos.Users;
using InterCase.Repository.Entities;

namespace InterCase.Service.Mappings
{
    public sealed class MappingProfile : Profile
    {
        public const string CorrelationIdKey = nameof(OrderPlacedMessage.CorrelationId);

        public MappingProfile()
        {
            CreateMap<CreateOrderRequest, Order>(MemberList.Source)
                .ForSourceMember(s => s.UserId, o => o.DoNotValidate())
                .ForMember(d => d.UserId, o => o.Ignore());

            CreateMap<UpdateOrderRequest, Order>(MemberList.Source);

            CreateMap<Order, OrderResponse>()
                .ForCtorParam(nameof(OrderResponse.Id), o => o.MapFrom(s => s.PublicId))
                .ForCtorParam(nameof(OrderResponse.UserId), o => o.MapFrom(s => s.User.PublicId));

            CreateMap<Order, OrderPlacedMessage>()
                .ForCtorParam(nameof(OrderPlacedMessage.OrderId), o => o.MapFrom(s => s.Id))
                .ForCtorParam(nameof(OrderPlacedMessage.UserId), o => o.MapFrom(s => s.User.PublicId))
                .ForCtorParam(nameof(OrderPlacedMessage.CorrelationId), o => o.MapFrom((_, context) => (string)context.Items[CorrelationIdKey]));

            CreateMap<User, UserResponse>()
                .ForCtorParam(nameof(UserResponse.Id), o => o.MapFrom(s => s.PublicId));
        }
    }
}
