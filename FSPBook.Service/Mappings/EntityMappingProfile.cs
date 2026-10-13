using FSPBook.Data.Entities;
using FSPBook.ViewModel;

namespace FSPBook.Service.Mappings;

public class EntityMappingProfile : AutoMapper.Profile
{
    public EntityMappingProfile()
    {
        CreateMap<Post, PostViewModel>()
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom((src, _) => src.Author?.FullName ?? "Unknown"));

        CreateMap<CreatePostViewModel, Post>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Author, opt => opt.Ignore())
            .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.ProfileId))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.ContentInput))
            .ForMember(dest => dest.DateTimePosted, opt => opt.MapFrom(_ => DateTimeOffset.Now));

        // Posts are only populated when explicitly requested (see ProfileService.GetProfileWithPostsAsync).
        CreateMap<Profile, ProfileViewModel>()
            .ForMember(dest => dest.Posts, opt => opt.Ignore());
    }
}
