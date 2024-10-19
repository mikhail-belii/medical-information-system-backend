using DataAccess.Repositories;
using DataAccess.RepositoryInterfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess;

public static class Extensions
{
    public static IServiceCollection AddDataBase(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IDoctorRepository, DoctorRepository>();
        return serviceCollection;
    }
}