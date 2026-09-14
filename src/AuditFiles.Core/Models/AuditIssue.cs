namespace AuditFiles.Core.Models;

public sealed class AuditIssue
{
    public required AuditIssueType Type { get; init; }

    public required AuditSeverity Severity { get; init; }

    public required string RelativePath { get; init; }

    public required string Description { get; init; }
}
