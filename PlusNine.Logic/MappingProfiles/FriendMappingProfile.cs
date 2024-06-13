using AutoMapper;
using PlusNine.Entities.DbSet;
using PlusNine.Entities.Dtos.Requests;
using PlusNine.Entities.Dtos.Responses;

namespace PlusNine.Logic.MappingProfiles
{
    public class FriendMappingProfile : Profile
    {
        public FriendMappingProfile()
        {
            CreateMap<User, UserSearchResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName));

            CreateMap<(Guid userId, Guid receiverId), FriendRequest>()
                .ForMember(dest => dest.SenderId, opt => opt.MapFrom(src => src.userId))
                .ForMember(dest => dest.ReceiverId, opt => opt.MapFrom(src => src.receiverId))
                .ForMember(dest => dest.FriendShipStatus, opt => opt.MapFrom(src => FriendRequestStatus.Pending))
                .ForMember(dest => dest.AddedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => 1));

            CreateMap<FriendRequest, Friendship>()
                .ForMember(dest => dest.User1Id, opt => opt.MapFrom(src => src.SenderId))
                .ForMember(dest => dest.User2Id, opt => opt.MapFrom(src => src.ReceiverId))
                .ForMember(dest => dest.AddedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => 1));

        }
    }
}