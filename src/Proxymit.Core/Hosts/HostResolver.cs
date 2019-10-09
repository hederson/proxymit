using Microsoft.AspNetCore.Http;
using Proxymit.Core.Configs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Proxymit.Core.Hosts
{
    public class HostResolver
    {
        private readonly IDomainConfigurationLoader domainConfigurationLoader;

        public HostResolver(IDomainConfigurationLoader domainConfigurationLoader)
        {
            this.domainConfigurationLoader = domainConfigurationLoader;
        }

        public Uri HostUri(HttpRequest httpRequest)
        {
            var hostRules = domainConfigurationLoader.GetConfigurations();
            
            var protocol = httpRequest.IsHttps ? "https" : "http";

            foreach (var hostRule in hostRules.Where(x => x.Domain == httpRequest.Host.Host).OrderByDescending(x => x.RuleLength).ToList())
            {
                if (!string.IsNullOrWhiteSpace(hostRule.Path))
                {
                    if(httpRequest.Path.StartsWithSegments(hostRule.Path))
                    {
                        return new Uri($"{protocol}://{hostRule.Destination}{httpRequest.Path}");
                    }
                }
                else
                {
                    return new Uri($"{protocol}://{hostRule.Destination}{httpRequest.Path}");
                }
            }

            return null;
        }
    }
}
