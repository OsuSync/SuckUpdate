using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Pomelo.EntityFrameworkCore.MySql;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SyncUpdate
{
    public class GlobalAPIRequestFilter : IOperationFilter
    {
        private static readonly OpenApiParameter appkeyParameter = new OpenApiParameter()
        {
            Name = "Sync-Client",
            Description = "App descriptor(App描述符)",
            In = ParameterLocation.Header,
            Required = false,
            Schema = new OpenApiSchema()
            {
                Type = "string",
            },
        };

        /// <summary>
        /// Apply add
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null) operation.Parameters = new List<OpenApiParameter>();
            operation.Parameters.Add(appkeyParameter);
        }
    }

    public class Startup
    {
        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            Logger = logger;
        }

        public IConfiguration Configuration { get; }
        public ILogger<Startup> Logger { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<Models.MinecraftContext>(p =>
            {
                var connStrBuilder = new MySqlConnector.MySqlConnectionStringBuilder()
                {
                    Server = Configuration["MySqlHost"],
                    Port = uint.Parse(Configuration["MySqlPort"]),
                    UserID = Configuration["MySqlUser"],
                    Password = Configuration["MySqlPassword"],
                    Database = Configuration["MySqlSchema"],
                };
                Logger.LogInformation($"Initialize database context to {Configuration["MySqlHost"]}");
                var connStr = connStrBuilder.ToString();
                p.UseMySql(connStr, ServerVersion.AutoDetect(connStr));
            });
            services.AddControllers()
            .AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            services.AddLogging();
            services.AddSwaggerGen(c =>
            {
                c.OperationFilter<GlobalAPIRequestFilter>();
                c.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Title = "Sync API",
                    Version = "v1"
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sync API v1"));
            app.UseCors(b => b.AllowAnyOrigin().WithExposedHeaders("Sync-Client"));
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }
}
