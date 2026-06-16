using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<ITokenService, JwtTokenService>();

            return services;
        }
    }
}
