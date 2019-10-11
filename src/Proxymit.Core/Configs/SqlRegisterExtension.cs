using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Proxymit.Core.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Proxymit.Core.Configs
{
    public static class SqlRegisterExtension
    {
        public static IServiceCollection AddSqlite(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ProxymitDbContext>(options =>
            {
                options.UseSqlite(configuration.GetConnectionString("Default"));
            });
            return services;
        }
    }
}
