using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Duracellko.WebSiteFunctions;

/// <summary>
/// Sends email to configured email address with messages from HTTP request.
/// </summary>
public class SendEmail
{
    private readonly ILogger log;

    public SendEmail(ILogger<SendEmail> log)
    {
        this.log = log;
    }

    [Function("SendEmail")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "POST")] HttpRequest request,
        CancellationToken cancellationToken)
    {
        log.LogInformation("SendEmail requested.");

        if (request.Body == null)
        {
            log.LogWarning("SendEmail without HTTP request body.");
            return new BadRequestObjectResult("Expecting email data in the request body.");
        }

        var emailData = await JsonSerializer.DeserializeAsync<EmailData>(request.Body, JsonSerializerOptions.Web, cancellationToken);
        if (!ValidateEmailData(emailData))
        {
            log.LogError("Send email failed: missing email data.");
            return new BadRequestObjectResult("Missing email data: sender name, address, subject, or message.");
        }

        await SendSmtpEmail(emailData, cancellationToken);

        log.LogInformation("Email sent successfully from '{SenderName}'<{SenderEmail}>.", emailData.SenderName, emailData.SenderEmail);
        return new OkObjectResult("Email sent successfully.");
    }

    private static async Task SendSmtpEmail(EmailData emailData, CancellationToken cancellationToken)
    {
        var settingSender = Environment.GetEnvironmentVariable("DURACELLKO_MAILMESSAGE_SENDER");
        var settingRecipient = Environment.GetEnvironmentVariable("DURACELLKO_MAILMESSAGE_RECIPIENT");

        using var smtpClient = CreateSmtpClient();

        var message = string.Format(Resources.EmailMessage, emailData.SenderName, emailData.SenderEmail, emailData.Message);
        await smtpClient.SendMailAsync(settingSender!, settingRecipient!, emailData.Subject, message, cancellationToken);
    }

    private static SmtpClient CreateSmtpClient()
    {
        var settingSmtpHost = Environment.GetEnvironmentVariable("DURACELLKO_SMTP_HOST");
        var settingSmtpPort = Environment.GetEnvironmentVariable("DURACELLKO_SMTP_PORT");
        var settingUsername = Environment.GetEnvironmentVariable("DURACELLKO_SMTP_USERNAME");
        var settingPassword = Environment.GetEnvironmentVariable("DURACELLKO_SMTP_PASSWORD");

        var credentials = new NetworkCredential(settingUsername, settingPassword);

        var port = int.Parse(settingSmtpPort!, NumberStyles.Integer, CultureInfo.InvariantCulture);
        return new SmtpClient(settingSmtpHost, port)
        {
            Credentials = credentials,
            EnableSsl = true
        };
    }

    private static bool ValidateEmailData([NotNullWhen(true)] EmailData? emailData)
    {
        if (emailData == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(emailData.SenderName) ||
            string.IsNullOrWhiteSpace(emailData.SenderEmail) ||
            string.IsNullOrWhiteSpace(emailData.Subject) ||
            string.IsNullOrWhiteSpace(emailData.Message))
        {
            return false;
        }

        try
        {
            var mailAddress = new MailAddress(emailData.SenderEmail, emailData.SenderName);
        }
        catch (FormatException)
        {
            return false;
        }

        return true;
    }
}
