using System.Collections.Generic;
using System.Linq;

namespace Proxymit.Core.Configs
{
    public interface IDomainConfigurationLoader
    {
        IQueryable<DomainConfiguration> GetConfigurations();
    }
}