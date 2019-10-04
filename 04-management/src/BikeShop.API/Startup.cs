using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Steeltoe.CloudFoundry.Connector.MySql;
using Steeltoe.CloudFoundry.Connector.MySql.EFCore;
using Steeltoe.Extensions.Configuration.ConfigServer;
using BikeShop.API.Data;
using BikeShop.API.Models;
using BikeShop.API.Repositories;
using BikeShop.API.Services;

//next 4 lines added in lab #4
using Steeltoe.Management.CloudFoundry;
using Steeltoe.Management.Endpoint.Info;
using Steeltoe.Common.HealthChecks;
using BikeShop.API.Contributors;


namespace BikeShop.API
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddMySqlConnection(Configuration);
            services.AddTransient<BicycleService>();
            services.AddTransient<BicycleRepository>();
            services.AddDbContext<BicycleDbContext>(o => o.UseMySql(Configuration, "mysql"));
            services.AddOptions();
            // services.ConfigureConfigServerClientOptions(Configuration);

            // var test = Configuration.GetSection("headerMessage");
            // if(test.Value == null)
            // {
            //     Console.WriteLine("No 'headerMessage' section in configuration!");
            // }
            services.Configure<HeaderMessageConfiguration>(Configuration.GetSection("headerMessage"));

            //next 3 lines added in lab #4
            services.AddCloudFoundryActuators(Configuration);
            services.AddScoped<IHealthContributor, BicycleContributor>();
            services.AddSingleton<IHealthContributor, DemoContributor>();
            // and these 2 as well
            services.AddSingleton<IInfoContributor, BicycleInfoContributor>();
            services.AddSingleton<IOperationCounter<Bicycle>, OperationCounter<Bicycle>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider sp)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseMvc();

            //next line added in lab #4
             app.UseCloudFoundryActuators();

            BicycleDbInitialize.init(app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope().ServiceProvider);

            var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            BicycleDbInitialize.init(scope.ServiceProvider);
        }
    }
}
