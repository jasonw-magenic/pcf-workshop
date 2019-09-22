# Connectors

## Continuing from Setup

### Open BikeShop

1. Open the solution folder `~/Workspace/BikeShop` in Visual Studio Code.

2. Open a terminal and change directories.

```bash
cd ~/Workspace/BikeShop/
```

---

### Add packages from Nuget

1. Entity Framework

```bash
dotnet add src/BikeShop.API package Microsoft.EntityFrameworkCore
```

2. Pomelo

```bash
dotnet add src/BikeShop.API package Pomelo.EntityFrameworkCore.MySql
```

3. Steeltoe CloudFoundry Connector

```bash
dotnet add src/BikeShop.API package Steeltoe.CloudFoundry.Connector
```

4. Steeltoe CloudFoundry Connector MySql

```bash
dotnet add src/BikeShop.API package Steeltoe.CloudFoundry.Connector.MySql
```

5. Restore all packages

```bash
dotnet restore
```

These references may also be added "manually."

1. Copy and paste the 4 refeneces directly into the BikeShop.API.csproj file, as shown below.

```xml
<ItemGroup>
    <!-- Existing PackageReference elements here -->
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="2.2.6" />
    <PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="2.2.0" />
    <PackageReference Include="Steeltoe.CloudFoundry.Connector" Version="1.1.0" />
    <PackageReference Include="Steeltoe.CloudFoundry.Connector.MySql" Version="1.1.0" />
</ItemGroup>
```

2. Restore all packages

```bash
dotnet restore
```

## Add Models

1. Create a `Models` directory.

In CMD

```cmd
mkdir src\BikeShop.API\Models
```

In Bash

```bash
mkdir -p ./src/BikeShop.API/Models
```

2. Create a `Bicycle.cs` file in the `/src/BikeShop.API/Models` directory.

3. Add the following properties to `Bicycle.cs` file.

<details>

<summary>Bicycle.cs</summary>

```c#
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BikeShop.API.Models
{
    [Table("bicycle")]
    public class Bicycle
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key, Column(Order = 0)]
        public long Id { get; set; }
        
        public string ProductName { get; set; }
        
        public double Price { get; set; }
        
        public string Description{ get; set; }
        
        public string Image { get; set; }
    }
}
```

</details>

---

## Add Database Context

1. Create the `Data` directory.

In CMD

```cmd
mkdir src\BikeShop.API\Data
```

In Bash

```bash
mkdir -p ./src/BikeShop.API/Data
```

2. Create a `BicycleDbContext.cs` file in the `/src/BikeShop.API/Data` directory.

3. Add the following to the `BicycleDbContext.cs` file.

<details>

<summary>BicycleDbContext.cs</summary>

```c#
using BikeShop.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BikeShop.API.Data
{
    public class BicycleDbContext : DbContext
    {
        public DbSet<Bicycle> Bicycles { get; set; }

        public BicycleDbContext(DbContextOptions<BicycleDbContext> dbContextOptions) : base(dbContextOptions)
        {
            
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
          base.OnModelCreating(modelBuilder);
    
          modelBuilder.Entity<Bicycle>(entity =>
          {
              entity.HasKey(e => e.Id);
              entity.Property(e => e.ProductName);
              entity.Property(e => e.Description);
              entity.Property(e => e.Price);
              entity.Property(e => e.Image);
          });
        }
    }
}
```

</details>

---

## Add Bicycle Repository

1. Create the `Repositories` directory.

In CMD

```cmd
mkdir src\BikeShop.API\Repositories
```

In Bash

```bash
mkdir -p ./src/BikeShop.API/Repositories
```

2. Create a `BicycleRepository.cs` file in the `/src/BikeShop.API/Repositories` directory.

3. Add the following to the `BicycleRepository.cs` file.

<details>

<summary>BicycleRepository.cs</summary>

```c#
using System.Collections.Generic;
using System.Threading.Tasks;
using BikeShop.API.Data;
using BikeShop.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BikeShop.API.Repositories
{
    public class BicycleRepository 
    { 
        private readonly BicycleDbContext _bicycleDbContext;

        public BicycleRepository(BicycleDbContext bicycleDbContext)
        {
            _bicycleDbContext = bicycleDbContext;
        }

        public async Task<IEnumerable<Bicycle>> GetAllBicycles()
        {
            return await _bicycleDbContext.Bicycles.ToListAsync();
        }

        public async Task<Bicycle> GetBicycle(long id)
        {
            return await _bicycleDbContext.FindAsync<Bicycle>(id);
        }

        public async Task<int> AddBicycle(Bicycle bicycle)
        {
            
            _bicycleDbContext.Add(bicycle);
            return await _bicycleDbContext.SaveChangesAsync();
        }
    }

}
```

</details>

---

## Add Bicycle Service

1. Create the `Services` directory.

In CMD

```cmd
mkdir src\BikeShop.API\Services
```

In Bash

```bash
mkdir -p ./src/BikeShop.API/Services
```

2. Create a `BicycleService.cs` file in the `/src/BikeShop.API/Services` directory.

3. Add the following to the `BicycleService.cs` file.

<details>

<summary>BicycleService.cs</summary>

```c#
using System.Collections.Generic;
using System.Threading.Tasks;
using BikeShop.API.Models;
using BikeShop.API.Repositories;

namespace BikeShop.API.Services
{
    public class BicycleService
    {
        private readonly BicycleRepository _bicycleRepository;

        public BicycleService(BicycleRepository bicycleRepository)
        {
            _bicycleRepository = bicycleRepository;
        }

        public async Task<IEnumerable<Bicycle>> GetAllBicycles()
        {
            return await _bicycleRepository.GetAllBicycles();
        }

        public async Task<Bicycle> GetBicycle(long id)
        {
            return await _bicycleRepository.GetBicycle(id);
        }

        public async Task<int> AddBicycle(Bicycle bicycle)
        {
            return await _bicycleRepository.AddBicycle(bicycle);
        }
    }
}
```

</details>

---

## Add Bicycle Controller

1. Create a `BicycleController.cs` file in the `/src/BikeShop.API/Controllers` directory.

2. Add the following to the `BicycleController.cs` file.

<details>

<summary>BicycleController.cs</summary>

```c#

using System.Collections.Generic;
using System.Threading.Tasks;
using BikeShop.API.Models;
using BikeShop.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace BikeShop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BicycleController : ControllerBase
    {
        private readonly BicycleService _bicycleService;

        public BicycleController(BicycleService bicycleService)
        {
            _bicycleService = bicycleService;
        }

        [HttpGet]
        public async Task<IEnumerable<Bicycle>> Get()
        {
            return await _bicycleService.GetAllBicycles();
        }

        [HttpGet("{id}")]
        public async Task<Bicycle> Get(long id)
        {
            return await _bicycleService.GetBicycle(id);
        }

        [HttpPost]
        public async Task<int> Add([FromBody] Bicycle bicycle)
        {
            return await _bicycleService.AddBicycle(bicycle);
        }
    }
}
```

</details>

---

## Add Bicycle Database Initializer

1. Create a `BicycleDbInitialize.cs` file in the `/src/BikeShop.API/Data` directory.

2. Add the following to the `BicycleDbInitialize.cs` file.

<details>

<summary>BicycleDbInitialize.cs</summary>

```c#
using System;
using System.Linq;
using BikeShop.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BikeShop.API.Data
{
    public static class BicycleDbInitialize
    {
        public static void init(IServiceProvider serviceProvider)
        {
            using (var context = new BicycleDbContext(serviceProvider.GetRequiredService<DbContextOptions<BicycleDbContext>>()))
            {
                context.Database.EnsureCreated();
                
                if (context.Bicycles.Any()) return;
                context.Bicycles.Add(new Bicycle
                {
                    Description = "Schwin",
                    Image = "img",
                    Price = 899.99,
                    ProductName = "Schwin Mountian Bike"
                });
                context.Bicycles.Add(new Bicycle
                {
                    Description = "Nishiki",
                    Image = "img",
                    Price = 399.99,
                    ProductName = "Nishiki Dirt Bike"
                });
                context.SaveChanges();
            }
        }
    }
}
```

</details>

---

## Update Startup

Edit the `Startup.cs` file.

Add the following using directives:

```c#
using BikeShop.API.Data;
using BikeShop.API.Repositories;
using BikeShop.API.Services;
using Steeltoe.CloudFoundry.Connector.MySql;
using Steeltoe.CloudFoundry.Connector.MySql.EFCore;
```

In the `ConfigureServices` method add the following:

```c#
services.AddMySqlConnection(Configuration);
services.AddTransient<BicycleService>();
services.AddTransient<BicycleRepository>();
services.AddDbContext<BicycleDbContext>(o => o.UseMySql(Configuration));
```

In the `Configure` method add the following:

```c#
BicycleDbInitialize.init(app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope().ServiceProvider);
```

<details>

<summary>Startup.cs</summary>

```c#
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

// next 5 lines added in lab #2
using BikeShop.API.Services;
using BikeShop.API.Repositories;
using BikeShop.API.Data;
using Steeltoe.CloudFoundry.Connector.MySql;
using Steeltoe.CloudFoundry.Connector.MySql.EFCore;

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

            // next 4 lines added in lab #2
            services.AddMySqlConnection(Configuration);
            services.AddTransient<BicycleService>();
            services.AddTransient<BicycleRepository>();
            services.AddDbContext<BicycleDbContext>(o => o.UseMySql(Configuration));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

            app.UseHttpsRedirection();
            app.UseMvc();

            // next line added in lab #2
            BicycleDbInitialize.init(app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope().ServiceProvider);
        }
    }
}
```

</details>

---

## Update Program

Edit the `Program.cs` file.

Add the following using directives:

```c#
using Steeltoe.Extensions.Configuration;
using Steeltoe.Extensions.Configuration.CloudFoundry;
```

Update the `CreateWebHostBuilder` method to add the following:

```c#
.UseKestrel()
.UseCloudFoundryHosting()
.UseContentRoot(Directory.GetCurrentDirectory())
.UseIISIntegration()
.UseStartup<Startup>()
.ConfigureAppConfiguration((builderContext, configBuilder) =>
{
    var env = builderContext.HostingEnvironment;
    configBuilder.SetBasePath(env.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
        .AddEnvironmentVariables()
        .AddCloudFoundry();
})
.ConfigureLogging((context, builder) =>
{
    builder.AddConfiguration(context.Configuration.GetSection("Logging"));
});
```

<details>

<summary>Program.cs</summary>

```c#
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Steeltoe.Extensions.Configuration;
using Steeltoe.Extensions.Configuration.CloudFoundry;

namespace BikeShop.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel()
                .UseCloudFoundryHosting()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((builderContext, configBuilder) =>
                {
                    var env = builderContext.HostingEnvironment;
                    configBuilder.SetBasePath(env.ContentRootPath)
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                        .AddEnvironmentVariables()
                        .AddCloudFoundry();
                })
                .ConfigureLogging((context, builder) =>
                {
                    builder.AddConfiguration(context.Configuration.GetSection("Logging"));
                });
    }
}

```

</details>

---

## Update App Settings

Add the following to the `appsettings.json` file:

```json
"mysql": {
    "client": {
      "sslmode": "none"
    }
  },
  "multipleMySqlDatabases": false
```

<details>

<summary>appsettings.json</summary>

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "AllowedHosts": "*",
  "mysql": {
    "client": {
      "sslmode": "none"
    }
  },
  "multipleMySqlDatabases": false
}
```

</details>

---

## Update App Settings for Development

Add the following to the `appsettings.Development.json` file:

```json
"mysql": {
"client": {
    "sslmode": "none",
    "ConnectionString": "Server=localhost;Database=BikeShop;Uid=root;Pwd=;sslmode=none;"
}
}
```

<details>

<summary>appsettings.Development.json</summary>

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "System": "Information",
      "Microsoft": "Information"
    }
  },
  "mysql": {
    "client": {
      "sslmode": "none",
      "ConnectionString": "Server=localhost;Database=newdb;Uid=root;Pwd=;sslmode=none;"
    }
  }
}
```

</details>

---

## Creating Database

1. Make sure you are logged in the PCF foundation:

```bash
cf target
```

2. Should give you the following results:

```bash
api endpoint:   https://api.cf.magenic.net
api version:    2.112.0
user:           [YOUR USER NAME]@magenic.com
org:            sandbox
space:          [YOUR USER NAME]
```

3. Check and see what services are available:

```bash
cf marketplace
```

 4. Should give you the following results:

 ```bash
service                       plans                       description
app-autoscaler                standard                    Scales bound applications in response to load
p-circuit-breaker-dashboard   standard                    Circuit Breaker Dashboard for Spring Cloud Applications
p-config-server               standard                    Config Server for Spring Cloud Applications
p-rabbitmq                    standard                    RabbitMQ service to provide shared instances of this high-performance multi-protocol messaging broker.
p-redis                       dedicated-vm, shared-vm     Redis service to provide pre-provisioned instances configured as a datastore, running on a shared or dedicated VM.
p-service-registry            standard                    Service Registry for Spring Cloud Applications
p.mysql                       db-micro                    Dedicated instances of MySQL
p.rabbitmq                    single-node-3.7             RabbitMQ service to provide dedicated instances of this high-performance multi-protocol messaging broker
p.redis                       cache-small, cache-medium   Redis service to provide on-demand dedicated instances configured as a cache.
 ```

 5. Create a MySQL instance: (note this take a few mins to complete)

 ```bash
cf create-service p.mysql db-micro BikeShopDB
 ```

 Confirm that the service has been created:

 ```bash
 cf services
 ```

You will see something like this:

```bash
name         service   plan       bound apps   last operation
BikeShopDB   p.mysql   db-micro                create succeeded
```

 6. Bind BikeShop App to the BikeShopDB (note [YOUR USER NAME] is your user name)

\* Be sure your service is done creating the instance, or the following command will fail

```bash
cf bind-service BikeShop-API-[YOUR USER NAME] BikeShopDB
```

---

## Push BikeShop to PCF

1. Change directories:

```bash
cd src/BikeShop.API/
```

2. Publish BikeShop.API

```bash
dotnet publish
```

3. Push BikeShop.API

```bash
cf push
```

---

## Testing BikeShop API

Test the api in a web browser by navigating to the values controller (`https://bikeshop-api-[YOUR AD NAME].cf.magenic.net/api/bicycle/1`).

```json
{"id":1,"productName":"Schwin Mountian Bike","price":899.99,"description":"Schwin","image":"img"}
```

---

## Recap

So far we have continued from the previous lab. Created a database and bound it to our app. We added the code to create our database, add sample data and be able to pull data out of the database by calling the web api.

## Next Steps

Next, we will see how to externalize configuration. Change to the Configuration branch to continue.