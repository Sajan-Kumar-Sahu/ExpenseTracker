using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Interfaces.Repositories;
using ExpenseTracker.Persistence.Common;
using ExpenseTracker.Persistence.Context;
using ExpenseTracker.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ExpenseTrackerDbContext>(
                options =>
                    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }
}
