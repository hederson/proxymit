using Certes.Acme;
using System;
using System.Collections.Generic;
using System.Text;

namespace Proxymit.Core.LetsEncrypt
{
    public class LetsEncryptOptions
    {
        public bool AcceptTermsOfService { get; set; }
        public string EmailAddress { get; set; }

        public bool UseStagingServer
        {
            get => AcmeServer == WellKnownServers.LetsEncryptStagingV2;
            set => AcmeServer = value
                    ? WellKnownServers.LetsEncryptStagingV2
                    : WellKnownServers.LetsEncryptV2;
        }

        internal Uri AcmeServer { get; set; }

        public int DaysBefore { get; set; }

        private string _encryptionPassword = string.Empty;
    }
}
