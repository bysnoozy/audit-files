using AuditFiles.Core.Models;

namespace AuditFiles.Core.Rules;

/// <summary>
/// Flags files whose extension is on SharePoint's blocked file type list.
/// </summary>
public sealed class BlockedFileTypeRule : IAuditRule
{
    public IEnumerable<AuditIssue> Evaluate(ScanEntry entry, ScanOptions options)
    {
        if (entry.Kind != ScanEntryKind.File || !options.IncludeBlockedFileTypeCheck)
        {
            yield break;
        }

        var extension = Path.GetExtension(entry.Name);
        if (string.IsNullOrEmpty(extension))
        {
            yield break;
        }

        var isBlocked = SharePointLimits.BlockedFileExtensions.Contains(extension)
            || (options.ExtraBlockedExtensions?.Contains(extension) ?? false);

        if (isBlocked)
        {
            yield return new AuditIssue
            {
                Type = AuditIssueType.BlockedFileType,
                Severity = AuditSeverity.Warning,
                RelativePath = entry.RelativePath,
                Description = $"Le type de fichier '{extension}' figure dans la liste des types de fichiers " +
                    "bloqués par SharePoint ; il devra être renommé, converti ou exclu avant la migration.",
            };
        }
    }
}
