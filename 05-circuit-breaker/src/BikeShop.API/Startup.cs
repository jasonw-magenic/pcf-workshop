﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Steeltoe.CloudFoundry.Connector.MySql;
using Steeltoe.CloudFoundry.Connector.MySql.EFCore;
using Steeltoe.Common.HealthChecks;
using Steeltoe.Management.CloudFoundry;
using Steeltoe.Management.Endpoint.Info;
using BikeShop.API.Contributors;
using BikeShop.API.Data;
using BikeShop.API.Models;
using BikeShop.API.Repositories;
using BikeShop.API.Services;

//next 2 lines added in lab #4
using Steeltoe.CircuitBreaker.Hystrix;
using BikeShop.API.Commands;

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
            services.AddCloudFoundryActuators(Configuration);
            services.AddScoped<IHealthContributor, BicycleContributor>();
            services.AddSingleton<IHealthContributor, DemoContributor>();
            services.AddSingleton<IInfoContributor, BicycleInfoContributor>();
            services.AddSingleton<IOperationCounter<Bicycle>, OperationCounter<Bicycle>>();
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "BikeShop API", Version = "v1" });
            });

            //next 2 lines added in lab #5
            services.AddHystrixCommand<GetAllBicyclesCommand>("BicycleService", Configuration);
            services.AddHystrixCommand<UpdateBicycleCommand>("BicycleService", Configuration);
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
            app.UseCloudFoundryActuators();
            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "BikeShop.API v1");
            });

            //next line added in lab #5
            app.UseHystrixRequestContext();

            // BicycleDbInitialize.init(app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope().ServiceProvider);
            using(var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                BicycleDbInitialize.init(scope.ServiceProvider);
            }
        }
    }
}
