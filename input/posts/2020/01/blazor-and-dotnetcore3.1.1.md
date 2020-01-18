Title: Blazor and .NET Core 3.1.1
Published: 2020-01-18
Tags:
- Blazor
- .NET Core
- NuGet
- Sysinternals
---
Few days ago updated version of [.NET Core 3.1.1](https://github.com/dotnet/core/blob/master/release-notes/3.1/3.1.1/3.1.1.md) was released. It includes several security fixes, so it is strongly recommend to upgrade. However, after installation I was not able to build [Blazor](https://docs.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-3.1) projects with client-side Blazor. Build failed with error:

```plain
CSC : error CS8034: Unable to load Analyzer assembly C:\Users\rasto\.nuget\packages\microsoft.aspnetcore.components.analyzers\3.1.0\analyzers\dotnet\cs\Microsoft.AspNetCore.Components.Analyzers.dll : Assembly with same name is already loaded
```

If you are simply looking for a solution to this problem feel free to skip to the [end of this blog post](#solution). However, if you would like to read a detective story about how I solved the problem, continue reading the full post.

## Installation of .NET Core 3.1.101 SDK

The day was just after the first Patch Tuesday of year 2020. I checked [Microsoft Developer Blogs](https://devblogs.microsoft.com/) and found out about [.NET Core January 2020 Updates](https://devblogs.microsoft.com/dotnet/net-core-january-2020/). This update fixed 6 security vulnerabilities including Denial of Service and Remote Code Execution in ASP.NET Core. So I decided it's better to update sooner than later.

At first I updated **Visual Studio 2019**. I simply executed Visual Studio Installer and soon I had installed Visual Studio 2019 16.4.3 without any issues. Then I wanted to update **.NET Core SDK**. However, when I ran `choco outdated`, no update for .NET Core SDK was available. I use [Chocolatey](https://chocolatey.org/) to install and update software. I checked Chocolatey website and found out that the [.NET Core SDK Chocolatey package](https://chocolatey.org/packages/dotnetcore-sdk) is waiting for maintainer.

![.NET Core SDK Chocolatey package version history](/images/posts/2020/01/BlazorDotNetCore-Chocolatey.png)

The status hasn't changed as I am writing this post. So I decided to update manually. I just downloaded the latest .NET Core SDK from [.NET download page](https://dotnet.microsoft.com/download). And then started installation. Everything went smooth.

I ran `dotnet --info` to check installed SDKs and I noticed there were some older versions of SDKs 2.x and 3.0.

![dotnet --info](/images/posts/2020/01/BlazorDotNetCore-DotNetInfo.png)

Those were probably some leftovers from installing previous versions of Visual Studio. Well, it was new year, so I decided to do some cleanup. I didn't use those versions of SDKs, so they can be deleted. There was nothing to uninstall in **Programs and Features**. So it should be possible to simply delete specific directories in `C:\Program Files\dotnet\sdk` and `C:\Program Files\dotnet\shared`.

## Verification

After updates and changes it was time to verify that .NET Core SDK still worked on my machine. If I could build and test a project, it would mean it worked. My guinea pig project was [Planning Poker 4 Azure](https://github.com/duracellko/planningpoker4azure). It's good testing project for this purpose, because it includes several types of projects: ASP.NET Core, .NET Standard, Blazor, unit and integrations tests. And it seemed I picked the right project verification, because it failed. And the error was: 

```plain
CSC : error CS8034: Unable to load Analyzer assembly C:\Users\rasto\.nuget\packages\microsoft.aspnetcore.components.analyzers\3.1.0\analyzers\dotnet\cs\Microsoft.AspNetCore.Components.Analyzers.dll : Assembly with same name is already loaded
```

Well, at first I opened the project in Visual Studio 2019. The build was successful. Also tests were green. But then I tried to build from command-line.

```powershell
dotnet restore
dotnet build -c Release
```

And that's when it failed with error.

## Finding solution

Of course my first thought was I deleted something I was not supposed to delete. So I started with easy fixes. My first try was to repair installation of .NET Core SDK 3.1.101.

1. I opened **Settings** and then **Apps**.
2. I found "Microsoft .NET Core SDK 3.1.101".
3. I clicked **Modify** and selected **Repair**.

It didn't help. So I tried to repair whole **Visual Studio 2019**. It didn't help either.

According to error the problem was with a DLL in one of NuGet packages. Therefore I thought there could be something wrong in NuGet local cache. So I tried to clean the NuGet cache.

```powershell
dotnet nuget locals all -c
```

But I was still experiencing the same error. Then I tried to find out if the problem was with any Blazor project or only client-side Blazor. I created new server-side Blazor project and tried to build it.

```powershell
mkdir BlazorApp1
cd BlazorApp1
dotnet new blazorserver
dotnet restore
dotnet build
```

That was successful. Then next step was to do the same with client-side Blazor. Client-side Blazor was not official part of .NET Core SDK, so the template had to be obtained from NuGet.

```powershell
dotnet new --install Microsoft.AspNetCore.Blazor.Templates::3.1.0-preview4.19579.2
mkdir BlazorApp2
cd BlazorApp2
dotnet new blazorwasm --hosted
dotnet restore
dotnet build
```

And the error was there again. Well, this time only as warning, because my project treats warnings as errors. At least I knew that problem was not with the .NET Core SDK 3.1.101 itself, but with Blazor client-side.

As the error complained about mismatch of **Microsoft.AspNetCore.Components.Analyzers.dll** I tried to identify, where the DLL was loaded from. [Process Explorer](https://docs.microsoft.com/en-us/sysinternals/downloads/process-explorer) is great tool for this kind of inspections. It is part of [Sysinternals](https://docs.microsoft.com/en-us/sysinternals/) package and very easy to install using [Chocolatey](https://chocolatey.org/).

```powershell
choco install sysinternals
procexp
```

At first it was necessary to switch to Admin-privileged mode by selecting **File** → **Show Details for All Processes** in menu.

![Process Explorer - Admin mode](/images/posts/2020/01/BlazorDotNetCore-ProcessExplorerAdmin.png)

Then it was possible to search for a process that held handle to specified file by pressing **Ctrl+F**. I searched for **Microsoft.AspNetCore.Components.Analyzers.dll**.

![Process Explorer - Find](/images/posts/2020/01/BlazorDotNetCore-ProcessExplorerFind.png)

And I found it. It was in subdirectory of `C:\Users\rasto\AppData\Local\Temp\VBCSCompiler\AnalyzerAssemblyLoader\`. As the **VBCSCompiler** was only temporary directory, I decided to clean it. Then I tried to build again. After the build there was only single subdirectory in **VBCSCompiler**. So I tried to search for **Microsoft.AspNetCore.Components.Analyzers.dll**

```powershell
cd 'C:\Users\rasto\AppData\Local\Temp\VBCSCompiler\AnalyzerAssemblyLoader\'
gci Microsoft.AspNetCore.Components.Analyzers.dll -Recurse
```

![Microsoft.AspNetCore.Components.Analyzers.dll in VBCSCompiler directory](/images/posts/2020/01/BlazorDotNetCore-VBCSCompiler.png)

Bingo! There were 2 different versions of the same DLL. One was evidently coming from NuGet package **Microsoft.AspNetCore.Components.Analyzers**. And I guessed that the second one was included in .NET Core SDK. It was easy to verify. I just searched for the DLL in **dotnet** directory.

```powershell
cd 'C:\Program Files\dotnet\'
gci Microsoft.AspNetCore.Components.Analyzers.dll -Recurse
```

And I was right.

![Microsoft.AspNetCore.Components.Analyzers.dll in dotnet directory](/images/posts/2020/01/BlazorDotNetCore-ComponentsInDotnet.png)

I found [Microsoft.AspNetCore.Components.Analyzers](https://www.nuget.org/packages/Microsoft.AspNetCore.Components.Analyzers/) at NuGet website. And I noticed there was version **3.1.1** available. However, according to the error version 3.1.0 was used. And that was the conflict that caused all the problems.

The web project targeted .NET Core SDK 3.1.101 that included Microsoft.AspNetCore.Components.Analyzers version 3.1.1.

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>netcoreapp3.1</TargetFramework>
    <UserSecretsId>3eb9c6dc-6f97-473c-9043-ba48877bb22f</UserSecretsId>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Duracellko.PlanningPoker.Client\Duracellko.PlanningPoker.Client.csproj" />
    ...
  </ItemGroup>

  ...
  
</Project>

```

Additionally the project referenced **Duracellko.PlanningPoker.Client** that was project for Blazor client-side application. This project targeted .NET Standard 2.1 and referenced NuGet Package **Microsoft.AspNetCore.Blazor**.

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <RazorLangVersion>3.0</RazorLangVersion>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Blazor" Version="3.1.0-preview4.19579.2" />
    ...
  </ItemGroup>
  
  ...

</Project>

```

After investigation of [Microsoft.AspNetCore.Blazor](https://www.nuget.org/packages/Microsoft.AspNetCore.Blazor/3.1.0-preview4.19579.2) package I found out that it referenced [Microsoft.AspNetCore.Components.Web](https://www.nuget.org/packages/Microsoft.AspNetCore.Components.Web/3.1.0). However, not the newest version **3.1.1**, but older version **3.1.0**. And recursively also older version **3.1.0** of **Microsoft.AspNetCore.Components.Analyzers**.

## Solution

As mentioned above the problem is that client-side Blazor application project (Duracellko.PlanningPoker.Client) references indirectly older version 3.1.0 of **Microsoft.AspNetCore.Components.Web** (instead of version 3.1.1) via **Microsoft.AspNetCore.Blazor**. So the solution is to enforce reference to NuGet Package **Microsoft.AspNetCore.Components.Web** version **3.1.1**.

This can be achieved by adding following line to the project file.

```xml
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="3.1.1" />
  </ItemGroup>
```

Hope it helps you to resolve your problems with Blazor application compilation.
