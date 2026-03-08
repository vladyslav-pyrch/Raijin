using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Raijin.CombinatoricsService.Application;

public static class ApplicationModule
{
    public static Assembly Assembly => typeof(ApplicationModule).Assembly;

    public static IServiceCollection AddApplication(this IServiceCollection services) => services;

}
