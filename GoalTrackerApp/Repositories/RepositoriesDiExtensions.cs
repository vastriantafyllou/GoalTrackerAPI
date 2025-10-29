namespace GoalTrackerApp.Repositories;

// extension method to add repository services to the ioc container
public static class RepositoriesDiExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}