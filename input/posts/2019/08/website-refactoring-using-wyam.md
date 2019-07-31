Title: Website refactoring using Wyam
Published: 2019-08-01
Tags:
- web
- Wyam
- static site generator
- CMS
---
Until now I had personal website implemented using [Orchard CMS](https://orchardproject.net/orchardcms.html). It was almost unchanged for several years. Sometimes I was thinking about upgrading the Orchard engine, but there was always something with higher priority. When I heard about [Orchard Core](https://orchardproject.net/) running on [ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet), I told myself that it's really time to do some upgrade. However, I realized that almost all content of the website is static and maybe better solution would be to use a static site generator. Then I found project [Wyam](https://wyam.io/) by [Dave Glick](https://daveaglick.com/). Wyam is flexible and extensible static site generator implemented in [.NET Core](https://dotnet.microsoft.com/).

With such a big change I also told myself to restart my blog that was hibernated for almost 10 years. So my first blog post (and few next ones) is about my experience with Wyam generating this website.

### Wyam installation

I am .NET developer, so installation and starting Wyam was super easy. I simply installed it as dotnet global tool.

```powershell
dotnet tool install -g Wyam.Tool
```

Then creating first website was also very quick. Following command creates new website using Blog recipe. Recipe in Wyam is a set of modules and steps, which turn your content into final static website.

```powershell
md duracellko.net
cd duracellko.net
wyam new -r Blog
wyam -p
```

`-p` option in the last command starts Wyam built-in web server, so that you can see your website. Additionally it's possible to use option `-w`, so that preview website is automatically updated, whenever you change any file.

### Wyam configuration

`wyam new` command creates sample content of the web. The most important file is **config.wyam**. This file configures, how the website is generated. It contains C# code that is executed and can modify **Settings** and **Pipelines** objects. But before that there should be 2 compiler directives to define recipe and theme.

```csharp
#recipe Blog
#theme CleanBlog
```

Previous directives configure Wyam to use [Blog](https://wyam.io/recipes/blog/overview) recipe and [CleanBlog](https://wyam.io/recipes/blog/themes/cleanblog) theme. Here is very simple explanation of what is recipe and theme.

* **Recipe** is a set of predefined pipelines and settings, which are applied when building the website.
* **Pipeline** is a set of steps (each step is an instance of a module), which convert content from **input** folder or previous pipelines and generate website in **output** folder. Example of a pipeline is: find all `*.md` files, convert them to HTML, prepend page header, validate links, and write to `*.html` files.
* **Theme** is a set of files, which are included as content by default. For example CleanBlog theme includes some CleanBlog CSS, [Bootstrap](https://getbootstrap.com/docs/3.3/) (CSS and JavaScript), page header and footer, navigation bar. Any file in the theme can be overridden, by including file with the same name in the **input** folder.

After recipe and theme directives `config.wyam` file can update settings of the website. Settings are updated by modifying **Settings** Dictionary using C# code.

```csharp
// Customization of settings
Settings[Keys.Host] = "duracellko.net";
Settings[BlogKeys.Title] = "Duracellko.NET";
Settings[BlogKeys.Description] = "Welcome to Duracellko.NET";
Settings[BlogKeys.Image] = "/images/background.jpg";
Settings[BlogKeys.IncludeDateInPostPath] = true;
Settings[BlogKeys.GenerateArchive] = false;
```

Previous example defines title and description of the website, and image displayed at the top of every page. Additionally it includes year and month in URL of every blog post and disables blog archive. I don't need it now and I will generate archive, when I have more blog posts.

### Website content

After configuring the website it's time to provide some content. All content is in **input** folder. Blog recipe supports 2 kinds of content: [Markdown](https://www.markdownguide.org/) files (`*.md`) and [Razor Pages](https://docs.microsoft.com/en-us/aspnet/core/razor-pages) (`*.cshtml`). In this blog post I focus on Markdown files. Razor Pages will be covered in next blog post.

In Wyam every document has content and metadata. Metadata are additional data attached to a page. For example: title of page, image of page, date of publishing. Metadata are separated from content by single line with 3 dashes `---`. Metadata are written in [YAML](https://yaml.org/) format. Content is written in file specific language (Markdown or Razor).

This is example of 'about.md' file. It defines single metadata value for 'Title' key.

```markdown
Title: About Me
---
# Welcome to Duracellko.NET

<div class="personal-photo">
    <img src="images/duracellkoHK.jpg" alt="Rasťo Novotný" class="img-rounded" />
</div>

Hi, I am Rasťo. Welcome to my homepage, where I would like to share my projects, experiences and other interesting things.
```

Now the configuration and content is defined, it's time to preview the site. Simply run `wyam -p -w` and open the site in browser.

Final source code of the website can be found on [GitHub](https://github.com/duracellko/duracellko.net). In next blog post we will look at Razor Pages in Wyam.

And at last I would like to thank [Dave Glick](https://daveaglick.com/) for this wonderful tool.
