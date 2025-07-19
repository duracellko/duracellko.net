namespace Duracellko.WebSiteFunctions;

/// <summary>
/// Data included in HTTP request to compose email.
/// </summary>
public class EmailData
{
    public string? SenderEmail { get; set; }

    public string? SenderName { get; set; }

    public string? Subject { get; set; }

    public string? Message { get; set; }
}
