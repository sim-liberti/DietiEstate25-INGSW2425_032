namespace DietiEstate.Shared.Models;

public class EmailData
{
    public EmailType Type { get; set; }
    public string ToName { get; set; } = string.Empty;
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}