using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TaskAPI.Data;
using Microsoft.EntityFrameworkCore;
using AppInsightsDemo.CustomSettings;
using TaskAPI.Lib;
using TaskAPI.Middleware;

namespace TaskAPI
{
    public class Startup
    {
        
        // Reference injection to use configuration interface.
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();

            // Entity Framework: Register the context with dependency injection
            services.AddDbContext<MyDatabaseContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));


            // Setup Swagger Document
            SetupSwaggerDocument(services);

            // Setup custom settings
            SetupCustomSettings(services);

            // Setup the application insights integration
            SetupApplicationInsights(services);

            // DEMO: Enable to allow manual handling of model binding errors
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            // Enable multi-stream read - This will allow to capture multiple errors
            services.AddTransient<EnableMultipleStreamReadMiddleware>();

        }


        /// <summary>
        /// Sets up the swagger documents
        /// </summary>
        /// <param name="services">The service collection</param>

        private void SetupSwaggerDocument(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Task API",
                    Version = "v1",
                    Description = "Task API which implement GET, POST, PUT, ets operations required by the Task App.",
                    TermsOfService = new Uri("http://www.fluminense.com.br"),
                    Contact = new OpenApiContact
                    {
                        Name = "Ricardo Frazao",
                        Email = "dontcare@nowhere.co"
                    },

                });

                // ** Added based on instructions in class.
                // Use method name as operationId so that ADD REST Client... will work
                c.CustomOperationIds(apiDesc =>
                {
                    return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null;
                });



                // ** Added according instructions during the classe. Location where xml is saved.
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);


            });
        }


        /// <summary>
        /// Sets up custom, strongly typed settings
        /// </summary>
        /// <param name="services">The service colleciton</param>
        private void SetupCustomSettings(IServiceCollection services)
        {
            // Strongly Typed Settings:
            // ** DON'T FORGET TO READ : https://weblog.west-wind.com/posts/2016/May/23/Strongly-Typed-Configuration-Settings-in-ASPNET-Core

            // Add support for injection of IOptions<T>
            services.AddOptions();

            // Add the class that represnets the settings for the CustomerLimits section 
            // in the JSON settings
            services.Configure<CustomSettings>(Configuration.GetSection(nameof(CustomSettings)));

            // Support Generic IConfiguration access for generic string access
            services.AddSingleton<IConfiguration>(Configuration);
        }

        
        /// <summary>
        /// Setup the application insights integration
        /// </summary>
        /// <param name="services">The services collection</param>
        private void SetupApplicationInsights(IServiceCollection services)
        {
            // Access settings
            // ** PENDING : ApplicationInsights applicationInsightsSettings = Configuration.GetSection("ApplicationInsights").Get<ApplicationInsights>();

            // Setup app insights
            // ** PENDING : APP INSIGHTS services.AddApplicationInsightsTelemetry(applicationInsightsSettings.InstrumentationKey);

            // Setup live monitering key so authentication is enabled allowing filtering of events
            // ** PENDING : APP INSIGHTS services.ConfigureTelemetryModule<QuickPulseTelemetryModule>((module, _) => module.AuthenticationApiKey = applicationInsightsSettings.AuthenticationApiKey);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            SetupSwaggerJsonGenerationAndUI(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable multi-stream read
            app.UseMultipleStreamReadMiddleware();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }



        /// <summary>
        /// Sets up the Swagger JSON file and Swagger Interactive UI
        /// </summary>
        /// <param name="app">The application builder</param>
        private static void SetupSwaggerJsonGenerationAndUI(IApplicationBuilder app)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger(c =>
            {
                // Use the older 2.0 format so the ADD REST Client... will work
                c.SerializeAsV2 = true;
            });

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            //       specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task API V1");

                // Serve the Swagger UI at the app's root (http://localhost:<port>)
                c.RoutePrefix = string.Empty;
            });

            /*
            // ** Added to use the older 2.0 format so add rest client will work.
            app.UseSwagger(c => { c.SerializeAsV2 = true; });

            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskAPI v1");

                // Serve the Swagger UI at the app's root 
                // (http://localhost:<port>)
                c.RoutePrefix = string.Empty;
            });

            */

        }

    }
}
