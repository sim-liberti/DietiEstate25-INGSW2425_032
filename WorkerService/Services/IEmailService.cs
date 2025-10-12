using System.Diagnostics.CodeAnalysis;

namespace DietiEstate.WorkerService.Services;

public interface IEmailService
{
    Task SendEmailAsync([StringSyntax(StringSyntaxAttribute.Json)] string emailData, CancellationToken cancellationToken);
}