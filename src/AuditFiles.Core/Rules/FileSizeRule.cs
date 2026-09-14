using AuditFiles.Core.Models;

namespace AuditFiles.Core.Rules;

/// <summary>
/// Flags files larger than the configured SharePoint upload limit.
/// </summary>
public sealed class FileSizeRule : IAuditRule
{
    public IEnumerable<AuditIssue> Evaluate(ScanEntry entry, ScanOptions options)
    {
        if (entry.Kind != ScanEntryKind.File)
        {
            yield break;
        }

        if (entry.SizeInBytes > options.MaxFileSizeInBytes)
        {
            var actualGb = entry.SizeInBytes / 1024d / 1024d / 1024d;
            var limitGb = options.MaxFileSizeInBytes / 1024d / 1024d / 1024d;

            yield return new AuditIssue
            {
                Type = AuditIssueType.FileTooLarge,
                Severity = AuditSeverity.Blocking,
                RelativePath = entry.RelativePath,
                Description = $"Le fichier pèse {actualGb:F2} Go, ce qui dépasse la limite de téléversement " +
                    $"SharePoint de {limitGb:F2} Go.",
            };
        }
    }
}
