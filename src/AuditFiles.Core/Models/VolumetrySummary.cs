namespace AuditFiles.Core.Models;

public sealed class VolumetrySummary
{
    public long TotalFiles { get; init; }

    public long TotalFolders { get; init; }

    public long TotalSizeInBytes { get; init; }

    public int MaxDepthEncountered { get; init; }

    public DateTime? OldestFileModifiedUtc { get; init; }

    public DateTime? NewestFileModifiedUtc { get; init; }

    /// <summary>
    /// File extensions found during the scan, ordered by total size descending.
    /// </summary>
    public required IReadOnlyList<ExtensionVolumetry> SizeByExtension { get; init; }

    /// <summary>
    /// The largest files found during the scan, ordered by size descending.
    /// </summary>
    public required IReadOnlyList<ScanEntry> LargestFiles { get; init; }
}
