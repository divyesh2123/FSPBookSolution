using AutoMapper;
using FSPBook.Service.Mappings;

namespace FSPBook.Tests.Helpers;

public static class TestMapper
{
    public static readonly IMapper Instance =
        new MapperConfiguration(cfg => cfg.AddProfile<EntityMappingProfile>()).CreateMapper();
}
