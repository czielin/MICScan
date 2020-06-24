using Data.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class MicscanContext : DbContext
    {
        public MicscanContext() : base("Micscan")
        {

        }

        public DbSet<SharedVulnerability> SharedVulnerabilities { get; set; }
    }
}
