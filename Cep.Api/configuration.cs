using System.Reflection;
using Cep.Domain.Handlers;
using Cep.Domain.Infrastructure.ExternalServices;
using Cep.Domain.Infrastructure.Repository;
using Cep.Infrastructure.Repository;
using FluentValidation;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.OpenApi.Models;
using Refit;
using Serilog;
using StackExchange.Redis;

namespace Cep.Api
{
    public static class Configurations
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IMvcCoreBuilder mvcBuilder, IConfiguration configuration, IWebHostEnvironment environment)
        {
            services.AddScoped<ICepDistributedCache, CepDistributedCache>();
            services.AddRefitClient<ICepExternalService>()
               .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["CEP_ENDPOINT"]));
            services.AddInfrastructureServices(configuration, mvcBuilder);


            return services;
        }
        private static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, IMvcCoreBuilder mvcBuilder)
        {
            var domainAssembly = typeof(CepHandler).Assembly;

            mvcBuilder
                .AddApiExplorer();
               
            services
                 .AddValidatorsFromAssembly(domainAssembly)
                 .AddMediatR(cfg =>
                 {
                    cfg.RegisterServicesFromAssembly(domainAssembly);
                 })
                .AddSwagger(configuration)
                .AddLogging()
                .AddCache(configuration)
                .AddHealthChecks();

            services.AddControllers();

            return services;
        }
        public static IHostBuilder UseSerilogLogging(this IHostBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            SerilogHostBuilderExtensions.UseSerilog(builder);
            return builder;
        }
        private static IServiceCollection AddCache(this IServiceCollection services, IConfiguration configuration)
        {
            if (string.IsNullOrEmpty(configuration["CACHE_INSTANCE_NAME"]))
                throw new ArgumentException("The parameter CACHE_INSTANCE_NAME is null or empty.");
            else if (string.IsNullOrEmpty(configuration["CACHE_CONFIGURATION_URL"]))
                throw new ArgumentException("The parameter CACHE_CONFIGURATION_URL is null or empty.");
            else if (string.IsNullOrEmpty(configuration["CACHE_CONFIGURATION_PORT"]))
                throw new ArgumentException("The parameter CACHE_CONFIGURATION_PORT is null or empty.");
            else if (string.IsNullOrEmpty(configuration["CACHE_TIME_EXPIRED_CACHED"]))
                throw new ArgumentException("The parameter CACHE_TIME_EXPIRED_CACHED is null or empty.");

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration["CACHE_CONFIGURATION_URL"];
                options.InstanceName = configuration["CACHE_INSTANCE_NAME"];
                options.ConfigurationOptions = new ConfigurationOptions()
                {
                    AbortOnConnectFail = false,
                    EndPoints =
                    {
                        {
                            configuration["CACHE_CONFIGURATION_URL"],
                            int.Parse(configuration["CACHE_CONFIGURATION_PORT"])
                        }
                    },
                };
            });

            return services;
        }
        private static IServiceCollection AddSwagger(this IServiceCollection services, IConfiguration configuration)
        {
            var assemblyName = Assembly.GetCallingAssembly().GetName();
            var version = string.Concat("v", assemblyName.Version);
            services.AddSwaggerGen(setup =>
            {
                setup.CustomSchemaIds(x => x.FullName);
                setup.EnableAnnotations();
                setup.DescribeAllParametersInCamelCase();
                var swaggerInfo = new OpenApiInfo
                {
                    Title = assemblyName.Name,
                    Version = version,
                    Description = "Microsservice",
                    TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "teste",
                        Email = "teste@teste.com",
                        Url = new Uri("https://example.com/teste"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "licenses Microsservice",
                        Url = new Uri("https://example.com/license"),
                    }
                };
                setup.SwaggerDoc(swaggerInfo.Version, swaggerInfo);
                setup.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
                var xmlPath = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath,
                string.Concat(assemblyName.Name, ".xml"));
                // Set the comments path for the Swagger JSON and UI.
                if (File.Exists(xmlPath))
                    setup.IncludeXmlComments(xmlPath);
            });
            services.AddSwaggerGenNewtonsoftSupport();
            return services;
        }
        public static IApplicationBuilder UseSwagger(this IApplicationBuilder app, IConfiguration configuration, IWebHostEnvironment environment)
        {
            var assemblyName = Assembly.GetCallingAssembly().GetName();
            var version = string.Concat("v", assemblyName.Version);

            var projectName = configuration["PROJECT_NAME"] ?? string.Empty;

            app.UseSwagger(setup =>
            {
                setup.PreSerializeFilters.Add(
                    (swaggerDoc, httpReq)
                    => swaggerDoc.Servers = new List<OpenApiServer>
                        {
                            new OpenApiServer
                            {
                                Url = environment.EnvironmentName.Equals("Development") ? $"{httpReq.Scheme}://{httpReq.Host.Value}" : $"{httpReq.Scheme}://{httpReq.Host.Value}/{projectName}"
                            }
                        });
            });
            app.UseSwaggerUI(setup =>
            {
                setup.RoutePrefix = string.Empty;
                setup.SwaggerEndpoint($"./swagger/{version}/swagger.json", version);
            });
            return app;
        }
    }

}
