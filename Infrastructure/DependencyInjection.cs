using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TitanFitness.Infrastructure.DataContext;
using TitanFitness.Infrastructure.Repositories.Implementations;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString =
            configuration.GetConnectionString("TitanFitnessConnection")
            ?? throw new InvalidOperationException(
                "Connection string was not found.");

        services.AddDbContext<ApplicationDbContext>(
            options =>
                options.UseSqlServer(connectionString));

        services.AddScoped(
            typeof(IReadOnlyRepository<,>),
            typeof(ReadOnlyRepository<,>));

        services.AddScoped(
            typeof(IWriteRepository<>),
            typeof(WriteRepository<>));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}