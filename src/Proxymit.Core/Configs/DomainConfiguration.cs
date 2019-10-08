using System;
using System.Collections.Generic;
using System.Text;

namespace Proxymit.Core.Configs
{
    public class DomainConfiguration
    {
        public string Domain { get; set; }
        public string Path { get; set; }
        public string ChallengeType { get; set; }
        public string Destination { get; set; }

        public int RuleLength { get {
                return Domain.Length + Path.Length;
            } }
    }
}
