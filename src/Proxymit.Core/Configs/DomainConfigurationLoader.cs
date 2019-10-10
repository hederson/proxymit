using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Binder;

namespace Proxymit.Core.Configs
{
    public class DomainConfigurationLoader : IDomainConfigurationLoader
    {
        private readonly IConfiguration configuration;

        public DomainConfigurationLoader(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IQueryable<DomainConfiguration> GetConfigurations()
        {
            return configuration.GetSection("Domains").Get<DomainConfiguration[]>().AsQueryable();
        }
    }
}
