using Microsoft.EntityFrameworkCore;
using Proxymit.Core.Configs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Proxymit.Core.Data
{
    public class ProxymitDbContext : DbContext
    {
        public virtual DbSet<DomainConfiguration> DomainConfiguration { get; set; }
    }
}
