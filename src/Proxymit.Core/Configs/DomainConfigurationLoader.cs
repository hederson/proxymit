using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            return new List<DomainConfiguration>
            {
                new DomainConfiguration
                {
                   Destination = "localhost:57475",
                   Domain = "engine.local",
                   Path = "",
                   ChallengeType = "",
                   HttpsRedirect = false
                },
                new DomainConfiguration
                {
                   Destination = "github.com",
                   Domain = "git.local",
                   Path = "",
                   ChallengeType = "",
                   HttpsRedirect = true
                }
            }.AsQueryable();
        }
    }
}
