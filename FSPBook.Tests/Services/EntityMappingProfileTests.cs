using AutoMapper;
using FSPBook.OutboundService.Extensions;
using FSPBook.OutboundService.Models;
using FSPBook.Data.Entities;
using FSPBook.Service.Mappings;
using FSPBook.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FSPBook.Tests.Services;

public class EntityMappingProfileTests
{
    [Fact]
    public void Configuration_Is_Valid()
    {
        new MapperConfiguration(cfg => cfg.AddProfile<EntityMappingProfile>()).AssertConfigurationIsValid();
    }

    [Fact]
    public void Portal_Registration_Includes_Entity_And_NewsApi_Profiles()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(typeof(EntityMappingProfile));
        services.AddOutboundServices(new ConfigurationBuilder().Build());

        var mapper = services.BuildServiceProvider().GetRequiredService<IMapper>();

        mapper.ConfigurationProvider.AssertConfigurationIsValid();
        Assert.Equal("Hello", mapper.Map<PostViewModel>(new Post { Content = "Hello" }).Content);
        Assert.Equal("News", mapper.Map<NewsItemViewModel>(new NewsApiArticle { Title = "News" }).Title);
    }
}
