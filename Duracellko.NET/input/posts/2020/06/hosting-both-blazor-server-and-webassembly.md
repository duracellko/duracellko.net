Title: Hosting both Blazor Server and WebAssembly in single website
Published: 2020-06-27
Tags:
- Blazor
- ASP.NET Core
- Hosting
- Web
---
[Blazor](https://blazor.net) framework supports 2 types of hosting and running of Blazor application. Blazor Server runs application on server inside ASP.NET Core application and only exchanges HTML fragments and events with client. And Blazor WebAssembly runs application completely inside web browser and in some cases does not need server at all. What if you want to use both models in the same application and run different type based on client device type.

After using [Planning Poker](https://github.com/duracellko/planningpoker4azure) application I noticed that application startup time on (especially low-end) mobile is much longer than on PC. For example opening the page on my notebook is less than half second, but loading on my Nokia 6.1 takes about 4-5 seconds. After startup the application runs smoothly, but startup is slow unfortunately.

Let's discuss what are advantages and disadvantages of each hosting model.

**Blazor WebAssembly** advantages:
* Application does not consume server resources.
* Application is still responsive, when connection to server is lost.
* Application responses faster to user input, because it does not require server round-trip.

**Blazor Server** advantages:
* Application does not need to be loaded to client, and thus startup time can be much faster.
* Application can run on browsers without WebAssembly support.

So I asked myself: Would it be possible to take advantages of WebAssembly on PC and serve Blazor Server on mobile for faster startup? Yes, it would.

# Let's start with Blazor WebAssembly

[Blazor WebAssembly template](https://docs.microsoft.com/en-us/aspnet/core/blazor/templates?view=aspnetcore-3.1) is much better starting point, because it already splits implementation into 3 projects: server, client, and shared. However, the template must be used with hosting parameter to create server application too.

```
dotnet new blazorwasm -ho -o BlazorApp1
```

The Planning Poker application implements multiple services like `MessageBoxService` (displays a message to user) or `PlanningPokerClient` (access web services on server using HttpClient). These services should be registered in Dependency Injection container. This should be done in a public static method, because registration must be done on client and on server.

```csharp
public static class Startup
{
    public static void ConfigureServices(IServiceCollection services, bool serverSide = false)
    {
        // Services are scoped, because on server-side scope is created for each client session.
        if (!serverSide)
        {
            services.AddScoped<IPlanningPokerUriProvider, PlanningPokerUriProvider>();
        }

        services.AddScoped<IPlanningPokerClient, PlanningPokerClient>();

        services.AddScoped<MessageBoxService>();
        services.AddScoped<IMessageBoxService>(p => p.GetRequiredService<MessageBoxService>());
    }
}
```

No service can be registered as singleton. In Blazor WebAssembly there is no difference between `AddScoped` and `AddSingleton`, but in Blazor Server singleton object would be shared by all clients connected to server.

And then it is possible to call `ConfigureServices` from `Main` function.

```csharp
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("app");

        builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        Startup.ConfigureServices(builder.Services, false);

        await builder.Build().RunAsync();
    }
}
```

# Use Blazor Server for mobile device

## Index Razor Page

First step is to make index page dynamic. Blazor WebAssembly template generates static `index.html` page that loads Blazor application. However, the index page must be dynamic, because it is different for mobile and PC. Therefore `Home.cshtml` Razor Page must be created in server project and can look like this.

```cs
@page
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@model Duracellko.PlanningPoker.Web.Model.HomeModel
@{
    Layout = null;
}

<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Scrum Planning Poker</title>
    <base href="/" />
    <!-- Link CSS here -->
    <link href="Content/Site.css" rel="stylesheet" />
</head>
<body>
    <app>
        @if (Model.UseServerSide)
        {
            @(await Html.RenderComponentAsync<Duracellko.PlanningPoker.Client.App>(RenderMode.Server))
        }
        else
        {
            <span class="oi oi-loop-circular"></span> <span>Loading...</span>
        }
    </app>

    <div id="blazor-error-ui" class="alert alert-warning alert-dismissible" role="alert">
        <p>
            <environment include="Staging,Production">
                An error has occurred. This application may no longer respond until reloaded.
            </environment>
            <environment include="Development">
                An unhandled exception has occurred. See browser dev tools for details.
            </environment>
        </p>
        <button type="button" class="reload btn btn-warning">Reload</button>
        <button type="button" class="dismiss close" aria-label="Dismiss">
            <span aria-hidden="true">&times;</span>
        </button>
    </div>

    <!-- Load required JavaScript here -->
    <script src="Scripts/PlanningPoker.js"></script>
    @if (Model.UseServerSide)
    {
        <script src="_framework/blazor.server.js"></script>
    }
    else
    {
        <script src="_framework/blazor.webassembly.js"></script>
    }
</body>
</html>
```

The page is very similar to `index.html`. And based on value `HomeModel.UseServerSide` it renders HTML to use Blazor Server or WebAssembly.

`index.html` should be deleted now.

And `HomeModel` class can look like this.

```csharp
public class HomeModel : PageModel
{
    // Regular Expression pattern to match mobile User Agent.
    // Source: http://detectmobilebrowsers.com/
    private const string MobileUserAgentPattern = @"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino";

    public bool UseServerSide => IsMobileBrowser;

    private bool IsMobileBrowser
    {
        get
        {
            var userAgent = Request.Headers[HeaderNames.UserAgent].ToString();
            if (string.IsNullOrEmpty(userAgent))
            {
                return false;
            }

            try
            {
                var timeout = TimeSpan.FromMilliseconds(200);
                return Regex.IsMatch(userAgent, MobileUserAgentPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline, timeout);
            }
            catch (TimeoutException)
            {
                // When User Agent is too complicated, then run Blazor on client-side.
                return false;
            }
        }
    }
}
```

The class parses User Agent string and tries to detect if client is mobile device or not. The code is based on web site [detectmobilebrowsers.com](http://detectmobilebrowsers.com/). Then the application uses Blazor Server, when client device is mobile.

## Configure ASP.NET Core Server

Next step is to configure ASP.NET Core application to serve both:

* Blazor Server Hub
* Blazor WebAssembly static files

This is configured in `Startup` class:

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
        services.AddMvc()
            .AddNewtonsoftJson();

        // Register other server services

        services.AddServerSideBlazor();
        services.AddSingleton<HttpClient>();
        services.AddSingleton<PlanningPokerServerUriProvider>();
        services.AddSingleton<Client.Service.IPlanningPokerUriProvider>(sp => sp.GetRequiredService<PlanningPokerServerUriProvider>());
        services.AddSingleton<IHostedService, HttpClientSetupService>();

        // Register services used by client on server-side.
        Client.Startup.ConfigureServices(services, true);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseWebAssemblyDebugging();
        }

        app.UseStaticFiles();
        app.UseBlazorFrameworkFiles();

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/Home");
        });
    }
}
```

`Startup` class registers following services:

* `AddMvc` to serve Web API used by the application and especially **Home** Razor page.
* `AddServerSideBlazor` to run Blazor Server infrastructure.
* `HttpClient` and other services, which are used by the the application. This will be explained in next section.

And following middlewares:

1. `UseStaticFiles` to serve static files like *.css or *.js files.
2. `UseBlazorFrameworkFiles` to serve Blazor static files like `blazor.server.js` or `blazor.webassembly.js` and application assemblies loaded in WebAssembly.
3. `MapControllers` in endpoints to serve Web API used by the application.
4. `MapBlazorHub` endpoint to serve Blazor Server endpoint.
5. `MapFallbackToPage("/Home")` to serve home page, when URL is not found. This is in case, when URL should be handled by Blazor page component, but browser needs to start Blazor infrastructure by loading Home page first.

## Configuring HttpClient

The application uses `HttpClient` to call server Web API. In Blazor WebAssembly the HttpClient is configured directly in `Main` method.

```csharp
builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
```

So the base address of the HttpClient is setup to URL of home page. Blazor framework initializes it from browser `window.location` or something similar.

However, it is not that simple in server. Blazor Server application should call Web API on itself. However, when registering HttpClient in Startup class, the application is not started yet and the URL is not known. Especially when it is configured to listen on random available TCP port.

At first it is necessary to register `HttpClient` as service. Notice it is registered as singleton. This is general recommendation in [You're using HttpClient wrong](https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/). Actually newer recommendation is to use [IHttpClientFactory](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-3.1) to handle DNS changes. However, the application is connecting to itself using "localhost" and there is no DNS involved. So singleton HttpClient is good enough.

```csharp
services.AddSingleton<HttpClient>();
services.AddSingleton<IHostedService, HttpClientSetupService>();
```

Next registered service is `HttpClientSetupService`. [IHostedService](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-3.1) can implement a background task in ASP.NET Core application. HttpClientSetupService is very simple service that waits until web server is started, then finds first URL of the server and configures `HttpClient.BaseAddress`.

```csharp
public class HttpClientSetupService : BackgroundService
{
    private readonly HttpClient _httpClient;
    private readonly IServer _server;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public HttpClientSetupService(
        HttpClient httpClient,
        IServer server,
        IHostApplicationLifetime applicationLifetime)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var applicationStartedToken = _applicationLifetime.ApplicationStarted;
        if (applicationStartedToken.IsCancellationRequested)
        {
            ConfigureHttpClient();
        }
        else
        {
            applicationStartedToken.Register(ConfigureHttpClient);
        }

        return Task.CompletedTask;
    }

    private void ConfigureHttpClient()
    {
        var serverAddresses = _server.Features.Get<IServerAddressesFeature>();
        var address = serverAddresses.Addresses.FirstOrDefault();
        if (address == null)
        {
            // Default ASP.NET Core Kestrel endpoint
            address = "http://localhost:5000";
        }
        else
        {
            address = address.Replace("*", "localhost", StringComparison.Ordinal);
            address = address.Replace("+", "localhost", StringComparison.Ordinal);
            address = address.Replace("[::]", "localhost", StringComparison.Ordinal);
        }

        var baseUri = new Uri(address);
        _httpClient.BaseAddress = baseUri;
    }
}
```

# Testing

Now it is possible to test the behavior. Start the application and open it in browser. Open browser Developer tools and open **Network** tab. There should be `blazor.webassembly.js` in list of loaded files. This indicates running Blazor WebAssembly.

![Blazor WebAssembly in browser](/images/posts/2020/06/BlazorWebAssembly.png)

Now click button to switch to mobile mode and reload the application. There should be `blazor.server.js` in list of loaded files. This indicates running Blazor Server.

![Blazor Server in browser](/images/posts/2020/06/BlazorWebServer.png)
