Title: Configure Blazor app from server
Published: 2020-05-02
Tags:
- Blazor
- WebAssembly
- ASP.NET Core
- Docker
- Azure App Service
---
# Blazor app configuration

Microsoft recently release [Blazor WebAssembly 3.2.0 Release Candidate](https://devblogs.microsoft.com/aspnet/blazor-webassembly-3-2-0-release-candidate-now-available/). This version includes configuration of an application by **appsettings.json** configuration file.

It works very nicely and simple. It is possible to add **appsettings.json** file to your Blazor project. You can also add **appsettings._Environment_.json** file and configure different environments (Production, Development). And then you can read configuration in the `Main` method same way as in ASP.NET Core application. For example:

```csharp
public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        var environment = builder.HostEnvironment;
        builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(environment.BaseAddress) });

        // This will create IConfigurationRoot that is the same interface
        // as used to configure ASP.NET Core applications.
        var configuration = builder.Configuration.Build();

        // It is possible to use standard methods to read configuration.
        var useHttpClient = configuration.GetValue<bool>("UseHttpClient");
        Startup.ConfigureServices(builder.Services, false, useHttpClient);

        builder.RootComponents.Add<App>("app");

        await builder.Build().RunAsync();
    }
}
```

And example of configuration file:

```json
{
    "UseHttpClient": true
}
```

# Configure from server application

At the moment Blazor WebAssembly supports only configuration from appsettings.json file by default. Good thing is that [Microsoft.Extensions.Configuration](https://www.nuget.org/packages/Microsoft.Extensions.Configuration/) library is pretty extensible and you can write your own configuration provider. However, in certain cases, there may be easier way.

In my case I don't want that appsettings.json file is static file, but that it is dynamically generated at server. And the reason for that is that I deploy my application in Docker container hosted in [Azure App Service](https://azure.microsoft.com/en-us/services/app-service/containers/). And most of the configuration is provided via environment variables passed to the container. So the question is, how some of those variables can be passed to Blazor application configuration.

At first I created class that represents client-side configuration serializable in JSON.

```csharp
public class PlanningPokerClientConfiguration
{
    public bool UseServerSide { get; set; }

    public bool UseHttpClient { get; set; }
}
```

Then I created **configuration** endpoint. It was implemented as ASP.NET Core controller that provides single route `configuration`. The controller was very simple. It just gets configuration object injected in constructor and then returns it from action method.

```csharp
[ApiController]
[Route("[controller]")]
public class ConfigurationController : ControllerBase
{
    public ConfigurationController(PlanningPokerClientConfiguration clientConfiguration)
    {
        ClientConfiguration = clientConfiguration ?? throw new ArgumentNullException(nameof(clientConfiguration));
    }

    public PlanningPokerClientConfiguration ClientConfiguration { get; }

    [HttpGet]
    public ActionResult GetConfiguration()
    {
        return Ok(ClientConfiguration);
    }
}
```

Then I configured dependency injection in `Startup` class to include configuration.

```csharp
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // register services for dependency injection

        var clientConfiguration = GetPlanningPokerClientConfiguration();
        services.AddSingleton<PlanningPokerClientConfiguration>(clientConfiguration);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // configure application
    }

    private PlanningPokerClientConfiguration GetPlanningPokerClientConfiguration()
    {
        return Configuration.GetSection("PlanningPokerClient").Get<PlanningPokerClientConfiguration>() ?? new PlanningPokerClientConfiguration();
    }
}
```

 Unfortunately Blazor WebAssembly loads configuration only if `appsettings.json` file exists in `wwwroot` folder of Blazor application project. So I included empty JSON file in my Blazor `wwwroot` folder of my application project. This is required, because compiler then generates Blazor application bootstrap that includes reference to load the configuration file.

```json
{
  // This JSON file should not be used. It is here only to force Blazor to load configuration.
}
```

And the last problem is that Blazor WebAssembly reads configuration from `/appsettings.json` URL and not from `/configuration` URL that refers to the controller I implemented. Luckily there is [ASP.NET Core Rewrite middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/url-rewriting?view=aspnetcore-3.1). This middleware can change URL of HTTP request in ASP.NET Core application before it is processed by rest of the pipeline.

In my case I simply want ASP.NET Core application to treat URL `/appsettings.json` as `/configuration`. So I added Rewrite middleware in `Configure` method of `Startup` class.

```csharp
public class Startup
{
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Rewrite middleware should be registered before other middlewares.
        var rewriteOptions = new RewriteOptions()
            .AddRewrite(@"^appsettings\.json$", "configuration", false);
        app.UseRewriter(rewriteOptions);

        // Register other middlewares
        app.UseStaticFiles();
        app.UseBlazorFrameworkFiles();

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
```

The first parameter of `AddRewrite` method is regular expression pattern, so it is possible to define dynamic patterns for URL rewriting.

Now I can configure Blazor client-side application by environment variables or command-line arguments at server.

```powershell
# configure via command-line
dotnet .\Duracellko.PlanningPoker.Web.dll --PlanningPokerClient:UseHttpClient=true

# configure via environment variables
$env:PlanningPokerClient__UseHttpClient = 'true'
dotnet .\Duracellko.PlanningPoker.Web.dll
```

You can explore implementation in real application [Planning Poker 4 Azure](https://github.com/duracellko/planningpoker4azure).

PS: Do not include any secrets (e.g. passowrds, security keys) in your Blazor application configuration. The configuration is loaded by client and thus anyone can read those valued with standard web browser developer tools.
