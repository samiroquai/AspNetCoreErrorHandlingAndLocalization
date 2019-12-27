//using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using WebApplication1.Infrastructure;
using WebApplication1.Resources;
using Microsoft.OpenApi.Models;
using System.IO;

namespace WebApplication1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddControllers(options =>
                options.Filters.Add<BusinessExceptionFilter>()).AddDataAnnotationsLocalization(o=>
                {
                var type = typeof(LocalizedResources);
                var assemblyName = new AssemblyName(type.GetTypeInfo().Assembly.FullName);
                var factory = services.BuildServiceProvider().GetService<IStringLocalizerFactory>();
                var localizer = factory.Create("ValidationResources", assemblyName.Name);
                o.DataAnnotationLocalizerProvider = (t, f) => localizer;
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Error handling with Localization demo API", Version = "v1" });
                var filePath = Path.Combine(System.AppContext.BaseDirectory, "WebApplication1.xml");
                c.IncludeXmlComments(filePath);
                c.OperationFilter<SwaggerAcceptLanguageHeaderOperationFilter>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var supportedCultures = new List<CultureInfo>() {
                new CultureInfo("en-US"),
                new CultureInfo("fr-BE") };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                // Formatting numbers, dates, etc.
                SupportedCultures = supportedCultures,
                // UI strings that we have localized.
                SupportedUICultures = supportedCultures,
                //la propriété suivante semble nécessiter ASP.NET CORE 3.1
               // ApplyCurrentCultureToResponseHeaders = true

            });
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{
            app.UseExceptionHandler("/api/Error");
            //}



            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Error handling with Localization demo API");
            });
        }
    }
}
