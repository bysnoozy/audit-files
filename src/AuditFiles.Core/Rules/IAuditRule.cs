using AuditFiles.Core.Models;

namespace AuditFiles.Core.Rules;

public interface IAuditRule
{
    IEnumerable<AuditIssue> Evaluate(ScanEntry entry, ScanOptions options);
}
