using AutoMapper;
using FSPBook.OutboundService.Models;
using FSPBook.ViewModel;

namespace FSPBook.OutboundService.Mappings;

public class NewsApiProfile : Profile
{
    public NewsApiProfile()
    {
        CreateMap<NewsApiArticle, NewsItemViewModel>()
            .ForMember(dest => dest.Title, opt => opt.NullSubstitute(string.Empty));
    }
}
