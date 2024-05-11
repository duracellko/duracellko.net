namespace Duracellko.NET.Deployment;

internal static class MimeTypeProvider
{
    private static readonly Dictionary<string, string> ExtensionMimeTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { ".1", "text/html" },
        { ".atom", "application/atom+xml" },
        { ".css", "text/css" },
        { ".eot", "application/vnd.ms-fontobject" },
        { ".html", "text/html" },
        { ".ico", "image/x-icon" },
        { ".jpg", "image/jpeg" },
        { ".js", "application/javascript" },
        { ".map", "application/octet-stream" },
        { ".otf", "font/otf" },
        { ".png", "image/png" },
        { ".rss", "application/rss+xml" },
        { ".svg", "image/svg+xml" },
        { ".ttf", "font/ttf" },
        { ".txt", "text/plain" },
        { ".woff", "font/woff" },
        { ".woff2", "font/woff2" },
        { ".xml", "text/xml" },
        { ".zip", "application/x-zip-compressed" }
    };

    public static string GetMimeTypeFromFileName(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentNullException(nameof(path));
        }

        var extension = Path.GetExtension(path);
        if (ExtensionMimeTypes.TryGetValue(extension, out var mimeType))
        {
            return mimeType;
        }

        throw new ArgumentException($"MIME type for extension {extension} is not defined.", nameof(path));
    }
}
