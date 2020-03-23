using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using JWTProjectCore.Core.JWT.Enum;
using JWTProjectCore.Core.JWT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace JWTProjectCore.Api.Controllers {
    [Route ("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase {
        public AuthController () { }
        // POST api/auth
        [AllowAnonymous]
        [HttpPost ("Login")]
        public IActionResult Post ([FromBody] AccessCredentials credenciais, [FromServices] SigningConfigurations signingConfigurations, [FromServices] TokenConfigurations tokenConfigurations, [FromServices] IDistributedCache cache) {
            bool credenciaisValidas = false;
            if (credenciais != null && !String.IsNullOrWhiteSpace (credenciais.Usuario)) { 
                if (credenciais.GrantType == GrantType.Password)
                {
                    // credenciaisValidas = (usuarioBase != null &&
                    //         credenciais.UserID == usuarioBase.UserID &&
                    //         credenciais.AccessKey == usuarioBase.AccessKey);
                    credenciaisValidas = true;
                } else if (credenciais.GrantType == GrantType.RefreshToken)
                {
                    if (!String.IsNullOrWhiteSpace(credenciais.RefreshToken))
                    {
                        RefreshTokenData refreshTokenBase = null;

                        string strTokenArmazenado =
                            cache.GetString(credenciais.RefreshToken);
                        if (!String.IsNullOrWhiteSpace(strTokenArmazenado))
                        {
                            refreshTokenBase = JsonConvert
                                .DeserializeObject<RefreshTokenData>(strTokenArmazenado);
                        }

                        // credenciaisValidas = (refreshTokenBase != null &&
                        //     credenciais.UserID == refreshTokenBase.UserID &&
                        //     credenciais.RefreshToken == refreshTokenBase.RefreshToken);
                        credenciaisValidas = true;
                        // Elimina o token de refresh já que um novo será gerado
                        if (credenciaisValidas)
                            cache.Remove(credenciais.RefreshToken);
                    }

                }
            }
            if (credenciaisValidas) {
                return GenerateToken (
                    credenciais.Usuario, signingConfigurations,
                    tokenConfigurations, cache);
            }
            else
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new {
                    authenticated = false,
                    message = "Falha ao autenticar"
                });
            }
        }
        private ActionResult GenerateToken(string userID,
            SigningConfigurations signingConfigurations,
            TokenConfigurations tokenConfigurations,
            IDistributedCache cache)
        {
            ClaimsIdentity identity = new ClaimsIdentity(
                new GenericIdentity(userID, "Login"),
                new[] {
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                        new Claim(JwtRegisteredClaimNames.UniqueName, userID)
                }
            );

            DateTime dataCriacao = DateTime.Now;
            DateTime dataExpiracao = dataCriacao +
                TimeSpan.FromSeconds(tokenConfigurations.Seconds);
            
            // Calcula o tempo máximo de validade do refresh token
            // (o mesmo será invalidado automaticamente pelo Redis)
            TimeSpan finalExpiration =
                TimeSpan.FromSeconds(tokenConfigurations.FinalExpiration);

            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = tokenConfigurations.Issuer,
                Audience = tokenConfigurations.Audience,
                SigningCredentials = signingConfigurations.GetSigningCredentials(),
                Subject = identity,
                NotBefore = dataCriacao,
                Expires = dataExpiracao
            });
            var token = handler.WriteToken(securityToken);

            var resultado = new
            {
                authenticated = true,
                created = dataCriacao.ToString("yyyy-MM-dd HH:mm:ss"),
                expiration = dataExpiracao.ToString("yyyy-MM-dd HH:mm:ss"),
                accessToken = token,
                refreshToken = Guid.NewGuid().ToString().Replace("-", String.Empty),
                message = "OK"
            };

            // Armazena o refresh token em cache através do Redis 
            var refreshTokenData = new RefreshTokenData();
            refreshTokenData.RefreshToken = resultado.refreshToken;
            refreshTokenData.UserID = userID;

            DistributedCacheEntryOptions opcoesCache =
                new DistributedCacheEntryOptions();
            opcoesCache.SetAbsoluteExpiration(finalExpiration);
            cache.SetString(resultado.refreshToken,
                JsonConvert.SerializeObject(refreshTokenData),
                opcoesCache);

            return Ok(resultado);
        }
    }
}
