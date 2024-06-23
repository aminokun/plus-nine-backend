using AutoMapper;
using PlusNine.Entities.DbSet;
using PlusNine.Entities.Dtos.Requests;

namespace PlusNine.Logic.MappingProfiles
{
    public class RequestToDomain : Profile
    {
        public RequestToDomain()
        {
            CreateMap<CreateObjectiveRequest, Objective>()
                .ForMember(
                    dest => dest.Status,
                opt => opt
                    .MapFrom(src => 1)
                )
                .ForMember(
                dest => dest.AddedDate,
                opt => opt
                    .MapFrom(src => DateTime.Now)
                )
                .ForMember(
                dest => dest.UpdatedDate,
                opt => opt
                    .MapFrom(src => DateTime.Now)
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
                dest => dest.Progress,
                opt => opt
                    .MapFrom(src => src.Progress)
                )
                .ForMember(
                    dest => dest.Completed,
                    opt => opt
                    .MapFrom(src => src.Completed )
                );



            CreateMap<UpdateObjectiveRequest, Objective>()
               .ForMember(
                dest => dest.Id,
                opt => opt
                .MapFrom(src => src.ObjectiveId)
               )
               .ForMember(
               dest => dest.UpdatedDate,
               opt => opt
                   .MapFrom(src => DateTime.Now)
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
                dest => dest.Progress,
                opt => opt
                    .MapFrom(src => src.Progress)
                )
                .ForMember(
                    dest => dest.Completed,
                    opt => opt
                    .MapFrom(src => src.Completed)
                )
                .ForMember(
                    dest => dest.Status,
                opt => opt
                    .MapFrom(src => 1)
                );
        }
    }
}
