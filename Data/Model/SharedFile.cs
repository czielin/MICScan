using Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Model
{
    public class SharedFile
    {
        public int Id { get; set; }
        public int SharedVulnerabilityId { get; set; }
        public string Name { get; set; }
        public VulnerabilityState VulnerabilityState { get; set; }
        public byte[] Content { get; set; }
    }
}
