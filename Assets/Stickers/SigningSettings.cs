using System;

namespace Agens.Stickers
{
    [Serializable]
    public class SigningSettings
    {
        public bool AutomaticSigning;
        public string ProvisioningProfile;
        public string ProvisioningProfileSpecifier;
    }
}