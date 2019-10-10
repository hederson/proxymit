using Microsoft.AspNetCore.Http;
using Proxymit.Core.Configs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Options;

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

        public virtual Uri GetHostUri(HttpRequest httpRequest, DomainConfiguration domainConfiguration)
        {
            var protocol = httpRequest.IsHttps ? "https" : "http";
            return new Uri($"{protocol}://{domainConfiguration.Destination}{httpRequest.Path}");
        }

        public virtual DomainConfiguration GetMathingCofinguration(HttpRequest request)
        {
            var hostRules = domainConfigurationLoader.GetConfigurations();
            foreach (var hostRule in hostRules.Where(x => x.Domain == request.Host.Host).OrderByDescending(x => x.RuleLength).ToList())
            {
                if (!string.IsNullOrWhiteSpace(hostRule.Path))
                {
                    if (request.Path.StartsWithSegments(hostRule.Path))
                    {
                        return hostRule;
                    }
                }
                else
                {
                    return hostRule;
                }
            }

            return null;
        }
    }
}
