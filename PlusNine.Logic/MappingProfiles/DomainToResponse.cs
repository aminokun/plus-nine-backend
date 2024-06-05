using AutoMapper;
using PlusNine.Entities.DbSet;
using PlusNine.Entities.Dtos.Responses;

namespace PlusNine.Logic.MappingProfiles
{
    public class DomainToResponse : Profile
    {
        public DomainToResponse()
        {
            CreateMap<Objective, GetObjectiveResponse>()
                .ForMember(
                    dest => dest.UserId,
                    opt => opt
                        .MapFrom(src => src.UserId)
                )
                .ForMember(
                    dest => dest.ObjectiveId,
                    opt => opt
                        .MapFrom(src => src.Id)
                )
                .ForMember(
                    dest => dest.ObjectiveName,
                    opt => opt
                        .MapFrom(src => src.ObjectiveName)
                )
                .ForMember(
                    dest => dest.CurrentAmount,
                    opt => opt
                    .MapFrom(src => src.CurrentAmount)
                )
                .ForMember(
                    dest => dest.AmountToComplete,
                    opt => opt
                    .MapFrom(src => src.AmountToComplete)
                )
                .ForMember(
                    dest => dest.Completed,
                    opt => opt
                    .MapFrom(src => src.Completed)
                );
        }

    }
}
