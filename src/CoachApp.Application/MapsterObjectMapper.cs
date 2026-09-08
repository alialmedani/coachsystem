using MapsterMapper;
using Volo.Abp.ObjectMapping;

namespace CoachApp;

/// <summary>
/// Routes ABP's <see cref="Volo.Abp.ObjectMapping.IObjectMapper"/> through Mapster.
/// Registered as the app's <see cref="IAutoObjectMappingProvider"/> so every
/// <c>ObjectMapper.Map&lt;TSource, TDestination&gt;()</c> call that has no explicit
/// <c>IObjectMapper&lt;TSource, TDestination&gt;</c> resolves via Mapster.
/// </summary>
public class MapsterObjectMapper : IAutoObjectMappingProvider
{
    private readonly IMapper _mapper;

    public MapsterObjectMapper(IMapper mapper)
    {
        _mapper = mapper;
    }

    public TDestination Map<TSource, TDestination>(object source)
    {
        return _mapper.Map<TDestination>(source);
    }

    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        return _mapper.Map(source, destination);
    }
}
