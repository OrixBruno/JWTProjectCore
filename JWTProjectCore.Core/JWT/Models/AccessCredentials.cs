using JWTProjectCore.Core.JWT.Enum;

namespace JWTProjectCore.Core.JWT.Models {
    public class AccessCredentials {

        public string Usuario { get; set; }
        public string Password { get; set; }
        public string RefreshToken { get; set; }
        public GrantType GrantType { get; set; }
    }
}