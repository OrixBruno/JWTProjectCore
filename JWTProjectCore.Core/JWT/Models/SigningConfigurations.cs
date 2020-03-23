using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace JWTProjectCore.Core.JWT.Models {
    public class SigningConfigurations {
        public SecurityKey Key { get; }

        private readonly SigningCredentials signingCredentials;

        public SigningCredentials GetSigningCredentials()
        {
            return signingCredentials;
        }

        public SigningConfigurations () {
            using (var provider = new RSACryptoServiceProvider (2048)) {
                Key = new RsaSecurityKey (provider.ExportParameters (true));
            }

            signingCredentials = new SigningCredentials (
                Key, SecurityAlgorithms.RsaSha256Signature);
        }
    }
}