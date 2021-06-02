using System.Collections.Generic;

namespace Ocelot.Configuration
{
    public class AuthenticationOptions
    {
        public AuthenticationOptions(List<string> allowedScopes, string policy, string authenticationProviderKey)
        {
            AllowedScopes = allowedScopes;
            Policy = policy;
            AuthenticationProviderKey = authenticationProviderKey;
        }

        public List<string> AllowedScopes { get; private set; }
        public string AuthenticationProviderKey { get; private set; }

        public string Policy { get; private set; }
    }
}
