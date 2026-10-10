using Soenneker.Esbuild.Util.Registrars;
using Soenneker.Utils.File.Registrars;
using Soenneker.Utils.Directory.Registrars;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.JavaScript.Minifier.Abstract;

namespace Soenneker.JavaScript.Minifier.Registrars;

/// <summary>
/// A utility for minifying JavaScript
/// </summary>
public static class JavaScriptMinifierRegistrar
{
    /// <summary>
    /// Adds <see cref="IJavaScriptMinifier"/> as a singleton service. <para/>
    /// </summary>
    public static IServiceCollection AddJavaScriptMinifierAsSingleton(this IServiceCollection services)
    {
        services.AddEsbuildUtilAsSingleton().AddFileUtilAsSingleton().AddDirectoryUtilAsSingleton();
        services.TryAddSingleton<IJavaScriptMinifier, JavaScriptMinifier>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IJavaScriptMinifier"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddJavaScriptMinifierAsScoped(this IServiceCollection services)
    {
        services.AddEsbuildUtilAsScoped().AddFileUtilAsScoped().AddDirectoryUtilAsScoped();
        services.TryAddScoped<IJavaScriptMinifier, JavaScriptMinifier>();

        return services;
    }
}
