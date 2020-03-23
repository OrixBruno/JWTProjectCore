using JWTProjectCore.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JWTProjectCore.Api {
    public class Startup {
        private readonly ILogger<Startup> _logger;
        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        public Startup (IConfiguration configuration, IHostingEnvironment environment, ILogger<Startup> logger) {
            _logger = logger;

            Configuration = configuration;
            Environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {
            RegisterServices (services);

            services.AddMvc ().SetCompatibilityVersion (CompatibilityVersion.Version_2_1);
        }
        public void AddRedis (IServiceCollection services) { }
        private void RegisterServices (IServiceCollection services) {
            Bootstrapper.AddSetupIoC (services, Configuration, Environment.IsDevelopment ());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment ()) {
                app.UseDeveloperExceptionPage ();
            } else {
                app.UseHsts ();
            }

            app.UseHttpsRedirection ();
            app.UseMvc ();
            app.UseSwagger ();

            if (Environment.IsDevelopment ()) {
                app.UseSwaggerUI (c => {
                    c.SwaggerEndpoint ($"/swagger/v1/swagger.json", "Api JWT");
                    c.DocumentTitle = "API Orix - JWT";
                    c.RoutePrefix = "jwt-api/api-docs";
                });

            } else {
                app.UseSwaggerUI (c => {
                    c.SwaggerEndpoint ($"./swagger/v1/swagger.json", "Api JWT");
                    c.DocumentTitle = "API Orix - JWT";
                    c.RoutePrefix = "/jwt-api/docs";
                });
            }
        }
    }
}