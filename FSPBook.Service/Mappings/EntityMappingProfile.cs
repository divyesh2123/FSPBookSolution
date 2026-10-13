using FSPBook.Data.Entities;
using FSPBook.ViewModel;

namespace FSPBook.Service.Mappings;

public class EntityMappingProfile : AutoMapper.Profile
{
    public EntityMappingProfile()
    {
        CreateMap<Post, PostViewModel>()
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom((src, _) => src.Author?.FullName ?? "Unknown"));

        // Posts are only populated when explicitly requested (see ProfileService.GetProfileWithPostsAsync).
        CreateMap<Profile, ProfileViewModel>()
            .ForMember(dest => dest.Posts, opt => opt.Ignore());
    }
}
