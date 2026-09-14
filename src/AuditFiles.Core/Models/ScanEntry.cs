namespace AuditFiles.Core.Models;

public sealed class ScanEntry
{
    public required string FullPath { get; init; }

    /// <summary>
    /// Path relative to the audited root, using '/' separators, as it would appear once the root
    /// folder's contents become a SharePoint document library.
    /// </summary>
    public required string RelativePath { get; init; }

    public required string Name { get; init; }

    public required ScanEntryKind Kind { get; init; }

    public long SizeInBytes { get; init; }

    /// <summary>
    /// Number of folder levels below the audited root (the root's direct children are depth 1).
    /// </summary>
    public int Depth { get; init; }

    public DateTime LastModifiedUtc { get; init; }

    public string? ParentRelativePath { get; init; }
}
