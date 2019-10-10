﻿using Microsoft.Extensions.DependencyInjection;
using Proxymit.Core.Configs;
using Proxymit.Core.Hosts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Proxymit.Core.Extensions
{
    public static class ProxymitDependencyInjection
    {
        public static IServiceCollection AddProxymit(this IServiceCollection services)
        {            
            services.AddHttpClient<HostClient>();
            services.AddScoped<IDomainConfigurationLoader, DomainConfigurationLoader>();
            services.AddScoped<HostResolver>();

            return services;
        }
    }
}
