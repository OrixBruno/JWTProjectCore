using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using JWTProjectCore.Core.JWT.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;

namespace JWTProjectCore.Core {
    public static class Bootstrapper {

        public static IServiceCollection AddSetupIoC (this IServiceCollection services, IConfiguration config, bool isDev) {
            services.AddRedisConfig (config);
            services.AddJwtConfig (config);
            services.AddSwaggerConfig ();
            return services;
        }
        public static void AddSwaggerConfig (this IServiceCollection services) {
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new Info
                    {
                        Title = "Api JWT Teste",
                        Version = "v1",
                        Description = "",
                        Contact = new Contact { Name = "Orix" }
                    });
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                c.DescribeAllEnumsAsStrings();

                var security = new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", new string[] { }},
                };
                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });
                c.AddSecurityRequirement(security);
            });
        }
        public static void AddRedisConfig (this IServiceCollection services, IConfiguration configuration) {
            services.AddDistributedRedisCache (options => {
                options.Configuration =
                    Environment.GetEnvironmentVariable("REDIS_HOST");
                options.InstanceName = "APIJwt";
            });
        }
        public static void AddJwtConfig (this IServiceCollection services, IConfiguration configuration) {
            var signingConfigurations = new SigningConfigurations ();
            services.AddSingleton (signingConfigurations);

            var tokenConfigurations = new TokenConfigurations ();
            new ConfigureFromConfigurationOptions<TokenConfigurations> (
                    configuration.GetSection ("TokenConfigurations"))
                .Configure (tokenConfigurations);
            services.AddSingleton (tokenConfigurations);

            services.AddAuthentication (authOptions => {
                authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer (bearerOptions => {
                var paramsValidation = bearerOptions.TokenValidationParameters;
                paramsValidation.IssuerSigningKey = signingConfigurations.Key;
                paramsValidation.ValidAudience = tokenConfigurations.Audience;
                paramsValidation.ValidIssuer = tokenConfigurations.Issuer;

                // Valida a assinatura de um token recebido
                paramsValidation.ValidateIssuerSigningKey = true;

                // Verifica se um token recebido ainda é válido
                paramsValidation.ValidateLifetime = true;

                // Tempo de tolerância para a expiração de um token (utilizado
                // caso haja problemas de sincronismo de horário entre diferentes
                // computadores envolvidos no processo de comunicação)
                paramsValidation.ClockSkew = TimeSpan.Zero;
            });

            services.AddAuthorization (auth => {
                auth.AddPolicy ("Bearer", new AuthorizationPolicyBuilder ()
                    .AddAuthenticationSchemes (JwtBearerDefaults.AuthenticationScheme‌​)
                    .RequireAuthenticatedUser ().Build ());
            });
        }
    }
}