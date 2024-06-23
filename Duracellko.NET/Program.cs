using Statiq.Markdown;

return await Bootstrapper.Factory
    .CreateWeb(args)
    .AddSetting(
        MarkdownKeys.MarkdownExtensions,
        new List<string> { "EmphasisExtra", "Math" })
    .RunAsync();
