using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Griddly.Mvc;

public static class GriddlyServiceCollectionExtensions
{
    public static IServiceCollection AddGriddly<TGriddlyConfig>(this IServiceCollection services, Action<GriddlyOptions> configure = null)
        where TGriddlyConfig : class, IGriddlyConfig
    {
        services.TryAddSingleton<IHttpContextAccessor, GriddlyHttpContextAccessor>();
        services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
        services.TryAddScoped<IGriddlyConfig, TGriddlyConfig>();

        configure?.Invoke(new GriddlyOptions());

        return services;
    }
}