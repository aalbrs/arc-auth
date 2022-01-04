using System.Collections.Generic;

namespace ArcAuth
{
    public class AppSettings
    {
        public string PortalUrl { get; set; }
        public string PortalAppId { get; set; }
        public string PortalAppSecret { get; set; }

        // Construct redirection and token endpoints
        public string AuthEndpoint { get => $"{PortalUrl}/sharing/rest/oauth2/authorize"; }
        public string TokenEndpoint { get => $"{PortalUrl}/sharing/rest/oauth2/token"; }
        public string UserInfoEndpoint { get => $"{PortalUrl}/sharing/rest/community/self"; }

        public AuthorisationSettings Authorisation { get; set; }

    }

    public class AuthorisationSettings
    {
        public List<PolicySetting> Policies { get; set; }
    }

    public class PolicySetting
    {
        public string Name { get; set; }
        public List<string> Groups { get; set; }
    }
}
