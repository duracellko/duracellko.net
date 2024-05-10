Title: Combine pages in Wyam
Published: 2019-08-12
Tags:
- Wyam
- Razor
- web
- static site generator
---
In previous post [Website refactoring using Wyam](website-refactoring-using-wyam) I wrote about how I created my website using [Wyam](https://wyam.io/). I focused on configuration and Markdown content. This time I write about **Razor pages** in Wyam. Specifically how I created [Projects](/projects) page.

The Projects page contains list of projects. Each item in the list has name, image and description. I could code all content and layout in single HTML file, but it would have few disadvantages. It would not be flexible to do updates in layout or styling. Adding new project would not be as simple. Single file for all projects is less maintainable. It would not possible to use Markdown for project description that is simpler than full HTML.

So I created separate Markdown file for every project. All project Markdown files are located in **projects** folder. Content of Markdown file is project description. And each file contains following metadata. Usage of Wyam metadata was descibed in previous post.

* **Title**: name of the project.
* **Image**: URL of project image.
* **OrderNumber**: number that defines order of the project in the list.

Example of Markdown file header:

```text
Title: Globe Time
Image: images/screenshots/GlobeTime.png
OrderNumber: 10
---
This application provides you information about local times in cities around the world...
```

Now as content is prepared, it's time to render it. By default Wyam Blog recipe would handle these Markdown files as pages and render HTML page for each project file. So it's important to exclude _projects_ folder from **Pages** pipeline and define separate **Projects** pipeline. This is done by adding following code to **config.wyam** file.

```csharp
Settings[BlogKeys.IgnoreFolders] = "projects";

// Pipeline customizations
Pipelines.Insert(0, "Projects",
    ReadFiles("projects/**/*.md"),
    FrontMatter(Yaml()),
    Markdown());
```

First line tells Blog pipeline to ignore _projects_ folder. Then new pipeline named _Projects_ is added. The pipeline is very simple. It processes all `*.md` files in _projects_ folder, reads metadata and converts Markdown to HTML. It is important that this pipeline is first to process (index is 0), because the data are used by Razor page processed in Pages pipeline.

Last step is to actually render the page. This is possible by using Razor pages in Wyam. Razor pages are used to define views in ASP.NET MVC. Razor language is combination of HTML and C# and is very powerful to render HTML. Wyam extends Razor pages to access Wyam specific objects, e.g. Documents or Pipelines. So it is possible to find all documents in _Projects_ pipeline and render HTML parts. This is **projects.cshtml** file that combines all projects into single HTML page.

```razor
Title: Projects
---
@{
    var index = 0;
    var projectDocuments = Documents["Projects"].OrderBy(d => d.Metadata.Get<int>("OrderNumber", int.MaxValue));
}
@foreach (var document in projectDocuments)
{
    @if (index != 0)
    {
        <hr />
    }

    index++;
    var title = document.Metadata.Get<string>(BlogKeys.Title);
    var image = document.Metadata.Get<string>(BlogKeys.Image);

    <div class="row">
    <div class="col-md-9 col-md-offset-3">
        <h2>@title</h2>
    </div>
    </div>
    <div class="row">
    <div class="col-md-3">
        @if (!string.IsNullOrEmpty(image))
        {
            <p><img src="@image" alt="@title" /></p>
        }
    </div>
    <div class="col-md-9">
        @Html.Raw(document)
    </div>
    </div>
}
```

The Razor page has following steps:

1. It finds all documents in _Projects_ pipeline and sorts them by **OrderNumber** metadata value.
2. For each document it reads **Title** and **Image** from metadata.
3. It renders title in header and image.
4. It renders document content (project description). Notice that it is rendered as raw HTML, because it was converted to HTML by pipeline already.

In summary Razor pages are very powerfull tool in Wyam, becuase they can access already processed pipelines and content. Full solution can be found in my [GitHub repository](https://github.com/duracellko/duracellko.net).
