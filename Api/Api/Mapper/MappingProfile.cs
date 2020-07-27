using Api.DTOs;
using Api.Helpers;
using Api.Models;
using AutoMapper;

namespace Api.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<GraphEntity, GraphEntityDTO>()
                .ForMember(destination => destination.OwnersMail, opts => opts.MapFrom(source => source.Owner == null ? Strings.ANONYMOUS : source.Owner.Mail));
        }
    }
}
