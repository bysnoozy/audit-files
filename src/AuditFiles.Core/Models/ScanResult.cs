namespace AuditFiles.Core.Models;

public sealed class ScanResult
{
    public required string RootPath { get; init; }

    public required IReadOnlyList<AuditIssue> Issues { get; init; }

    public required VolumetrySummary Volumetry { get; init; }

    public required DateTime ScanStartedUtc { get; init; }

    public required DateTime ScanCompletedUtc { get; init; }

    /// <summary>
    /// Folders that could not be read (permission denied, I/O error, etc.) and were skipped.
    /// </summary>
    public required IReadOnlyList<string> Errors { get; init; }
}
