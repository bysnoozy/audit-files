using AuditFiles.Core.Models;

namespace AuditFiles.Core.Rules;

/// <summary>
/// Flags names that are structurally invalid for SharePoint: leading/trailing spaces, a trailing
/// period, names that are too long, and names that collide with a reserved Windows or SharePoint name.
/// </summary>
public sealed class InvalidNameRule : IAuditRule
{
    public IEnumerable<AuditIssue> Evaluate(ScanEntry entry, ScanOptions options)
    {
        var name = entry.Name;

        if (name.StartsWith(' ') || name.EndsWith(' '))
        {
            yield return Issue(entry, AuditIssueType.NameStartsOrEndsWithSpace,
                "Le nom commence ou se termine par un espace, ce qui n'est pas autorisé par SharePoint.");
        }

        if (name.EndsWith('.'))
        {
            yield return Issue(entry, AuditIssueType.NameEndsWithPeriod,
                "Le nom se termine par un point, ce qui n'est pas autorisé par SharePoint.");
        }

        if (name.Length > options.MaxNameLength)
        {
            yield return Issue(entry, AuditIssueType.NameTooLong,
                $"Le nom comporte {name.Length} caractères, ce qui dépasse la limite de " +
                $"{options.MaxNameLength} caractères.");
        }

        var nameWithoutExtension = Path.GetFileNameWithoutExtension(name);
        var isReserved = SharePointLimits.ReservedNames.Contains(name)
            || SharePointLimits.ReservedNames.Contains(nameWithoutExtension)
            || SharePointLimits.ReservedNamePrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

        if (isReserved)
        {
            yield return Issue(entry, AuditIssueType.ReservedName,
                "Le nom correspond à un nom réservé (nom de périphérique Windows, nom système SharePoint " +
                "ou fichier de verrouillage Office) non autorisé par SharePoint.");
        }
    }

    private static AuditIssue Issue(ScanEntry entry, AuditIssueType type, string description) => new()
    {
        Type = type,
        Severity = AuditSeverity.Blocking,
        RelativePath = entry.RelativePath,
        Description = description,
    };
}
