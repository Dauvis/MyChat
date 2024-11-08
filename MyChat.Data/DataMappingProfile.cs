using AutoMapper;
using MyChat.Common.Enums;
using System.Reflection;

namespace MyChat.Data
{
    public class DataMappingProfile : Profile
    {
        public DataMappingProfile()
        {
            CreateMap<Models.UserInfo, Common.DTO.UserInfoDTO>().ReverseMap();

            CreateMap<Common.DTO.UserProfileDocumentDTO, Models.UserProfileDocument>()
                .ForMember(dest => dest.PartitionKey, opt => opt.MapFrom(src => src.AuthProvider.ToString()));
            CreateMap<Models.UserProfileDocument, Common.DTO.UserProfileDocumentDTO>()
                .ForMember(dest => dest.AuthProvider, opt => opt.MapFrom(src => Enum.Parse<AuthenticationProvidersType>(src.PartitionKey)));
        }
    }
}
