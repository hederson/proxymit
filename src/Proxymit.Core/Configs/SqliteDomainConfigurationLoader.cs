using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proxymit.Core.Configs
{
    public class SqliteDomainConfigurationLoader : IDomainConfigurationLoader
    {
        public IQueryable<DomainConfiguration> GetConfigurations()
        {
            throw new NotImplementedException();
        }
    }
}
