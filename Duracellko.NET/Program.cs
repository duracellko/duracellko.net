using Statiq.Markdown;

return await Bootstrapper.Factory
    .CreateWeb(args)
    .AddSetting(
        MarkdownKeys.MarkdownExtensions,
        new List<string> { "Bootstrap", "EmphasisExtra", "Math" })
    .RunAsync();
